/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.ImageResources;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles {
	public enum PsdColorMode {
		Bitmap = 0,
		Grayscale = 1,
		Indexed = 2,
		RGB = 3,
		CMYK = 4,
		Multichannel = 7,
		Duotone = 8,
		Lab = 9
	};

	public enum PsdFileVersion : short {
		Psd = 1,
		PsbLargeDocument = 2
	}

	public class PsdFile {
		#region Constructors

		public PsdFile(PsdFileVersion version = PsdFileVersion.Psd) {
			Version = version;

			BaseLayer = new Layer(this);
			ImageResourceList = new ImageResourceList();
			Layers = new List<Layer>();
			AdditionalInfo = new List<AbstractLayerInfo>();
		}

		public PsdFile(Stream stream, Context context)
			: this() {
			Load(stream, context);
		}

		#endregion

		#region Load and save

		//internal Context Context { get; private set; }

		private void Load(Stream stream, Context context) {
			var reader = new PsdBinaryReader(stream, context);

			LoadHeader(reader);
			LoadColorModeData(reader);
			LoadImageResources(reader);
			LoadLayerAndMaskInfo(reader);

			LoadImage(reader);
			DecompressImages();
		}

		public void Save(Stream stream, Context context) {
			PrepareSave();

			using (var writer = new PsdBinaryWriter(stream, context)) {
				SaveHeader(writer);
				SaveColorModeData(writer);
				SaveImageResources(writer);
				SaveLayerAndMaskInfo(writer);
				SaveImage(writer);
			}
		}

		#endregion

		#region Header

		/// <summary>
		/// Photoshop file format version.
		/// </summary>
		public PsdFileVersion Version { get; private set; }

		public bool IsLargeDocument =>
			(Version == PsdFileVersion.PsbLargeDocument);

		private Int16 channelCount;

		/// <summary>
		/// The number of channels in the image, including any alpha channels.
		/// </summary>
		public Int16 ChannelCount {
			get => channelCount;
			set {
				if (value < 1 || value > 56) {
					throw new ArgumentException("Number of channels must be from 1 to 56.");
				}

				channelCount = value;
			}
		}

		private void CheckDimension(int dimension) {
			if (dimension < 1) {
				throw new ArgumentException("Image dimension must be at least 1.");
			}

			if ((Version == PsdFileVersion.Psd) && (dimension > 30000)) {
				throw new ArgumentException("PSD image dimension cannot exceed 30000.");
			}

			if ((Version == PsdFileVersion.PsbLargeDocument) && (dimension > 300000)) {
				throw new ArgumentException("PSB image dimension cannot exceed 300000.");
			}
		}

		/// <summary>
		/// The height of the image in pixels.
		/// </summary>
		public int RowCount {
			get => this.BaseLayer.Rect.Height;
			set {
				CheckDimension(value);
				BaseLayer.Rect = new Rectangle(0, 0, BaseLayer.Rect.Width, value);
			}
		}


		/// <summary>
		/// The width of the image in pixels. 
		/// </summary>
		public int ColumnCount {
			get => this.BaseLayer.Rect.Width;
			set {
				CheckDimension(value);
				BaseLayer.Rect = new Rectangle(0, 0, value, BaseLayer.Rect.Height);
			}
		}

		private int bitDepth;

		/// <summary>
		/// The number of bits per channel. Supported values are 1, 8, 16, and 32.
		/// </summary>
		public int BitDepth {
			get => bitDepth;
			set {
				switch (value) {
					case 1:
					case 8:
					case 16:
					case 32:
						bitDepth = value;
						break;
					default:
						throw new NotImplementedException("Invalid bit depth.");
				}
			}
		}

		/// <summary>
		/// The color mode of the file.
		/// </summary>
		public PsdColorMode ColorMode { get; set; }

		///////////////////////////////////////////////////////////////////////////

		private void LoadHeader(PsdBinaryReader reader) {
			reader.Log(LogType.Log, "Load, Begin, File header");

			var signature = reader.ReadAsciiChars(4);
			if (signature != "8BPS") {
				throw new PsdInvalidException("The given stream is not a valid PSD file");
			}

			Version = (PsdFileVersion) reader.ReadInt16();
			reader.Log(LogType.Log, $"Load, Info, Version {(int) Version}");
			if ((Version != PsdFileVersion.Psd)
			    && (Version != PsdFileVersion.PsbLargeDocument)) {
				throw new PsdInvalidException("The PSD file has an unknown version");
			}

			//6 bytes reserved
			reader.BaseStream.Position += 6;

			this.ChannelCount = reader.ReadInt16();
			this.RowCount = reader.ReadInt32();
			this.ColumnCount = reader.ReadInt32();
			BitDepth = reader.ReadInt16();
			ColorMode = (PsdColorMode) reader.ReadInt16();

			reader.Log(LogType.Log, "Load, End, File header");
		}

		///////////////////////////////////////////////////////////////////////////

		private void SaveHeader(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, File header");

			string signature = "8BPS";
			writer.WriteAsciiChars(signature);
			writer.Write((Int16) Version);
			writer.Write(new byte[] {0x0, 0x0, 0x0, 0x0, 0x0, 0x0,});
			writer.Write(ChannelCount);
			writer.Write(RowCount);
			writer.Write(ColumnCount);
			writer.Write((Int16) BitDepth);
			writer.Write((Int16) ColorMode);

			writer.Log(LogType.Log, "Save, End, File header");
		}

		#endregion

		///////////////////////////////////////////////////////////////////////////

		#region ColorModeData

		/// <summary>
		/// If ColorMode is ColorModes.Indexed, the following 768 bytes will contain 
		/// a 256-color palette. If the ColorMode is ColorModes.Duotone, the data 
		/// following presumably consists of screen parameters and other related information. 
		/// Unfortunately, it is intentionally not documented by Adobe, and non-Photoshop 
		/// readers are advised to treat duotone images as gray-scale images.
		/// </summary>
		public byte[] ColorModeData = new byte[0];

		private void LoadColorModeData(PsdBinaryReader reader) {
			reader.Log(LogType.Log, "Load, Begin, ColorModeData");

			var paletteLength = reader.ReadUInt32();
			if (paletteLength > 0) {
				ColorModeData = reader.ReadBytes((int) paletteLength);
			}

			reader.Log(LogType.Log, "Load, End, ColorModeData");
		}

		private void SaveColorModeData(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, ColorModeData");

			writer.Write((UInt32) ColorModeData.Length);
			writer.Write(ColorModeData);

			writer.Log(LogType.Log, "Save, End, ColorModeData");
		}

		#endregion

		///////////////////////////////////////////////////////////////////////////

		#region ImageResources

		/// <summary>
		/// The Image resource blocks for the file
		/// </summary>
		public ImageResourceList ImageResourceList { get; set; }

		public ResolutionInfo Resolution {
			get => (ResolutionInfo) ImageResourceList.Get(ResourceID.ResolutionInfo);
			set => ImageResourceList.Set(value);
		}


		///////////////////////////////////////////////////////////////////////////

		private void LoadImageResources(PsdBinaryReader reader) {
			reader.Log(LogType.Log, "Load, Begin, ImageResources");

			var imageResourcesLength = reader.ReadUInt32();
			if (imageResourcesLength <= 0) {
				return;
			}

			var startPosition = reader.BaseStream.Position;
			var endPosition = startPosition + imageResourcesLength;
			while (reader.BaseStream.Position < endPosition) {
				var imageResource = ImageResourceFactory.CreateImageResource(reader);
				ImageResourceList.Add(imageResource);
			}

			reader.Log(LogType.Log, "Load, End, ImageResources");

			//-----------------------------------------------------------------------
			// make sure we are not on a wrong offset, so set the stream position 
			// manually
			reader.BaseStream.Position = startPosition + imageResourcesLength;
		}

		///////////////////////////////////////////////////////////////////////////

		private void SaveImageResources(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, ImageResources");

			using (new PsdBlockLengthWriter(writer)) {
				foreach (var imgRes in ImageResourceList) {
					imgRes.Save(writer);
				}
			}

			writer.Log(LogType.Log, "Save, End, ImageResources");
		}

		#endregion

		///////////////////////////////////////////////////////////////////////////

		#region LayerAndMaskInfo

		public List<Layer> Layers { get; private set; }

		public List<AbstractLayerInfo> AdditionalInfo { get; private set; }

		public bool AbsoluteAlpha { get; set; }

		///////////////////////////////////////////////////////////////////////////

		private void LoadLayerAndMaskInfo(PsdBinaryReader reader) {
			reader.Log(LogType.Log, "Load, Begin, Layer and mask info");

			var layersAndMaskLength = IsLargeDocument
				? reader.ReadInt64()
				: reader.ReadUInt32();
			if (layersAndMaskLength <= 0) {
				return;
			}

			var startPosition = reader.BaseStream.Position;
			var endPosition = startPosition + layersAndMaskLength;

			LoadLayers(reader, true);
			LoadGlobalLayerMask(reader, endPosition);
			LayerInfoFactory.LoadAll(reader, this, AdditionalInfo, endPosition, true);

			foreach (var layerInfo in AdditionalInfo) {
				switch (layerInfo.Key) {
					case "LMsk":
						GlobalLayerMaskData = ((RawLayerInfo) layerInfo).Data;
						break;
				}
			}

			reader.Log(LogType.Log, "Load, End, Layer and mask info");
		}

		///////////////////////////////////////////////////////////////////////////

		private void SaveLayerAndMaskInfo(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, Layer and mask info");

			using (new PsdBlockLengthWriter(writer, IsLargeDocument)) {
				var startPosition = writer.BaseStream.Position;

				SaveLayers(writer);
				SaveGlobalLayerMask(writer);

				foreach (var info in AdditionalInfo) {
					info.Save(writer,
						globalLayerInfo: true,
						isLargeDocument: IsLargeDocument);
				}

				writer.WritePadding(startPosition, 2);
			}

			writer.Log(LogType.Log, "Save, End, Layer and mask info");
		}

		///////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Load Layers Info section, including image data.
		/// </summary>
		/// <param name="reader">PSD reader.</param>
		/// <param name="hasHeader">Whether the Layers Info section has a length header.</param>
		internal void LoadLayers(PsdBinaryReader reader, bool hasHeader) {
			reader.Log(LogType.Log, "Load, Begin, Layers Info section");

			long sectionLength = 0;
			if (hasHeader) {
				sectionLength = IsLargeDocument
					? reader.ReadInt64()
					: reader.ReadUInt32();

				if (sectionLength <= 0) {
					reader.Log(LogType.Log, "Load, End, Layers Info section");
					return;
				}
			}

			var startPosition = reader.BaseStream.Position;
			var numLayers = reader.ReadInt16();

			// If numLayers < 0, then number of layers is absolute value,
			// and the first alpha channel contains the transparency data for
			// the merged result.
			if (numLayers < 0) {
				AbsoluteAlpha = true;
				numLayers = Math.Abs(numLayers);
			}

			for (int i = 0; i < numLayers; i++) {
				var layer = new Layer(reader, this);
				Layers.Add(layer);
			}

			//-----------------------------------------------------------------------

			// Load image data for all channels.
			foreach (var layer in Layers) {
				reader.Log(LogType.Log,
					$"Load, Begin, Layer image, {layer.Name}");
				foreach (var channel in layer.Channels) {
					channel.LoadPixelData(reader);
				}

				reader.Log(LogType.Log,
					$"Load, End, Layer image, {layer.Name}");
			}

			// Length is set to 0 when called on higher bitdepth layers.
			if (sectionLength > 0) {
				// Layers Info section is documented to be even-padded, but Photoshop
				// actually pads to 4 bytes.
				var endPosition = startPosition + sectionLength;
				var positionOffset = reader.BaseStream.Position - endPosition;
				Debug.Assert(positionOffset > -4,
					"LoadLayers did not read the full length of the Layers Info section.");
				Debug.Assert(positionOffset <= 0,
					"LoadLayers read past the end of the Layers Info section.");

				if (reader.BaseStream.Position < endPosition) {
					reader.BaseStream.Position = endPosition;
				}
			}

			reader.Log(LogType.Log, "Load, End, Layers");
		}

		///////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Decompress the document image data and all the layers' image data, in parallel.
		/// </summary>
		private void DecompressImages() {
			var layersAndComposite = Layers.Concat(new[] {BaseLayer});
			var channels = layersAndComposite.SelectMany(x => x.Channels);
			Parallel.ForEach(channels, channel => { channel.DecodeImageData(); });

			foreach (var layer in Layers) {
				foreach (var channel in layer.Channels) {
					if (channel.ID == -2) {
						layer.Masks.LayerMask.ImageData = channel.ImageData;
					} else if (channel.ID == -3) {
						layer.Masks.UserMask.ImageData = channel.ImageData;
					}
				}
			}
		}

		/// <summary>
		/// Check the validity of the PSD file and generate necessary data.
		/// </summary>
		public void PrepareSave() {
			CheckDimension(ColumnCount);
			CheckDimension(RowCount);
			VerifyInfoLayers();
			VerifyLayerSections();

			var imageLayers = Layers.Concat(new List<Layer>() {this.BaseLayer}).ToList();

			foreach (var layer in imageLayers) {
				layer.PrepareSave();
			}

			SetVersionInfo();
		}

		/// <summary>
		/// Verifies that any Additional Info layers are consistent.
		/// </summary>
		private void VerifyInfoLayers() {
			var infoLayersCount = AdditionalInfo.Count(x => x is InfoLayers);
			if (infoLayersCount > 1) {
				throw new PsdInvalidException(
					$"Cannot have more than one {nameof(InfoLayers)} in a PSD file.");
			}

			if ((infoLayersCount > 0) && (Layers.Count == 0)) {
				throw new PsdInvalidException(
					$"{nameof(InfoLayers)} cannot exist when there are 0 layers.");
			}
		}

		/// <summary>
		/// Verify validity of layer sections.  Each start marker should have a
		/// matching end marker.
		/// </summary>
		internal void VerifyLayerSections() {
			int depth = 0;
			foreach (var layer in Enumerable.Reverse(Layers)) {
				var layerSectionInfo = layer.AdditionalInfo.SingleOrDefault(
					x => x is LayerSectionInfo);
				if (layerSectionInfo == null) {
					continue;
				}

				var sectionInfo = (LayerSectionInfo) layerSectionInfo;
				switch (sectionInfo.SectionType) {
					case LayerSectionType.Layer:
						break;

					case LayerSectionType.OpenFolder:
					case LayerSectionType.ClosedFolder:
						depth++;
						break;

					case LayerSectionType.SectionDivider:
						depth--;
						if (depth < 0) {
							throw new PsdInvalidException("Layer section ended without matching start marker.");
						}

						break;

					default:
						throw new PsdInvalidException("Unrecognized layer section type.");
				}
			}

			if (depth != 0) {
				throw new PsdInvalidException("Layer section not closed by end marker.");
			}
		}

		/// <summary>
		/// Set the VersionInfo resource on the file.
		/// </summary>
		public void SetVersionInfo() {
			var versionInfo = (VersionInfo) ImageResourceList.Get(ResourceID.VersionInfo);
			if (versionInfo == null) {
				versionInfo = new VersionInfo();
				ImageResourceList.Set(versionInfo);

				// Get the version string.  We don't use the fourth part (revision).
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				var version = assembly.GetName().Version;
				var versionString = version.Major + "." + version.Minor + "." + version.Build;

				// Strings are not localized since they are not shown to the user.
				versionInfo.Version = 1;
				versionInfo.HasRealMergedData = true;
				versionInfo.ReaderName = "Paint.NET PSD Plugin";
				versionInfo.WriterName = "Paint.NET PSD Plugin " + versionString;
				versionInfo.FileVersion = 1;
			}
		}


		/// <summary>
		/// Saves the Layers Info section, including headers and padding.
		/// </summary>
		/// <param name="writer">The PSD writer.</param>
		private void SaveLayers(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, Layers Info section");

			using (new PsdBlockLengthWriter(writer, IsLargeDocument)) {
				var startPosition = writer.BaseStream.Position;

				// Only one set of Layers can exist in the file.  If layers will be
				// written to the Additional Info section, then the Layers section
				// must be empty to avoid conflict.
				var hasInfoLayers = AdditionalInfo.Exists(x => x is InfoLayers);
				if (!hasInfoLayers) {
					SaveLayersData(writer);
				}

				// Documentation states that the Layers Info section is even-padded,
				// but it is actually padded to a multiple of 4.
				writer.WritePadding(startPosition, 4);
			}

			writer.Log(LogType.Log, "Save, End, Layers Info section");
		}

		/// <summary>
		/// Saves the layer data, excluding headers and padding.
		/// </summary>
		/// <param name="writer">The PSD writer.</param>
		internal void SaveLayersData(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, Layers");

			var numLayers = (Int16) Layers.Count;
			if (AbsoluteAlpha) {
				numLayers = (Int16) (-numLayers);
			}

			// Photoshop will not load files that have a layer count of 0 in the
			// compatible Layers section.  Instead, the Layers section must be
			// entirely empty.
			if (numLayers == 0) {
				return;
			}

			writer.Write(numLayers);

			foreach (var layer in Layers) {
				layer.Save(writer);
			}

			foreach (var layer in Layers) {
				writer.Log(LogType.Log,
					$"Save, Begin, Layer image, {layer.Name}");
				foreach (var channel in layer.Channels) {
					channel.SavePixelData(writer);
				}

				writer.Log(LogType.Log,
					$"Save, End, Layer image, {layer.Name}");
			}

			// The caller is responsible for padding.  Photoshop writes padded
			// lengths for compatible layers, but unpadded lengths for Additional
			// Info layers.

			writer.Log(LogType.Log, "Save, End, Layers");
		}

		///////////////////////////////////////////////////////////////////////////

		byte[] GlobalLayerMaskData = new byte[0];

		private void LoadGlobalLayerMask(PsdBinaryReader reader, long endPosition) {
			reader.Log(LogType.Log, "Load, Begin, GlobalLayerMask");

			if (endPosition - reader.BaseStream.Position >= 4) {
				var maskLength = reader.ReadUInt32();

				if (maskLength > 0) {
					GlobalLayerMaskData = reader.ReadBytes((int) maskLength);
				}
			}

			reader.Log(LogType.Log, "Load, End, GlobalLayerMask");
		}

		///////////////////////////////////////////////////////////////////////////

		private void SaveGlobalLayerMask(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, GlobalLayerMask");

			if (AdditionalInfo.Exists(x => x.Key == "LMsk")) {
				writer.Write((UInt32) 0);
			} else {
				writer.Write((UInt32) GlobalLayerMaskData.Length);
				writer.Write(GlobalLayerMaskData);
			}

			writer.Log(LogType.Log, "Save, End, GlobalLayerMask");
		}

		///////////////////////////////////////////////////////////////////////////

		#endregion

		///////////////////////////////////////////////////////////////////////////

		#region Composite image

		/// <summary>
		/// Represents the composite image.
		/// </summary>
		public Layer BaseLayer { get; set; }

		public ImageCompression ImageCompression { get; set; }

		private void LoadImage(PsdBinaryReader reader) {
			reader.Log(LogType.Log, "Load, Begin, Composite image");

			ImageCompression = (ImageCompression) reader.ReadInt16();

			// Create channels
			for (Int16 i = 0; i < ChannelCount; i++) {
				reader.Log(LogType.Log, "Load, Begin, Channel image data");

				var channel = new Channel(i, this.BaseLayer);
				channel.ImageCompression = ImageCompression;
				channel.Length = this.RowCount
				                 * Util.BytesPerRow(BaseLayer.Rect.Size, BitDepth);

				// The composite image stores all RLE headers up-front, rather than
				// with each channel.
				if (ImageCompression == ImageCompression.Rle) {
					channel.RleRowLengths = new RleRowLengths(reader, RowCount, IsLargeDocument);
					channel.Length = channel.RleRowLengths.Total;
				}

				BaseLayer.Channels.Add(channel);
				reader.Log(LogType.Log, "Load, End, Channel image data");
			}

			foreach (var channel in this.BaseLayer.Channels) {
				reader.Log(LogType.Log, "Load, Begin, Channel image data");
				Util.CheckByteArrayLength(channel.Length);
				channel.ImageDataRaw = reader.ReadBytes((int) channel.Length);
				reader.Log(LogType.Log, "Load, End, Channel image data");
			}

			// If there is exactly one more channel than we need, then it is the
			// alpha channel.
			if ((ColorMode != PsdColorMode.Multichannel)
			    && (ChannelCount == ColorMode.MinChannelCount() + 1)) {
				var alphaChannel = BaseLayer.Channels.Last();
				alphaChannel.ID = -1;
			}

			reader.Log(LogType.Log, "Load, End, Composite image");
		}

		///////////////////////////////////////////////////////////////////////////

		private void SaveImage(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, Composite image");

			writer.Write((short) this.ImageCompression);
			if (this.ImageCompression == ImageCompression.Rle) {
				foreach (var channel in this.BaseLayer.Channels) {
					writer.Log(LogType.Log, "Save, Begin, RLE header");
					channel.RleRowLengths.Write(writer, IsLargeDocument);
					writer.Log(LogType.Log, "Save, End, RLE header");
				}
			}

			foreach (var channel in this.BaseLayer.Channels) {
				writer.Log(LogType.Log, "Save, Begin, Channel image data");
				writer.Write(channel.ImageDataRaw);
				writer.Log(LogType.Log, "Save, End, Channel image data");
			}

			writer.Log(LogType.Log, "Save, End, Composite image");
		}

		#endregion
	}


	/// <summary>
	/// The possible Compression methods.
	/// </summary>
	public enum ImageCompression {
		/// <summary>
		/// Raw data
		/// </summary>
		Raw = 0,

		/// <summary>
		/// RLE compressed
		/// </summary>
		Rle = 1,

		/// <summary>
		/// ZIP without prediction.
		/// </summary>
		Zip = 2,

		/// <summary>
		/// ZIP with prediction.
		/// </summary>
		ZipPrediction = 3
	}
}