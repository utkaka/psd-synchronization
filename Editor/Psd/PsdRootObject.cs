using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.ImageResources;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd {
	[Serializable]
	public class PsdRootObject {
		[SerializeField]
		private string _id;
		[SerializeField]
		private string _name;
		[SerializeField]
		private ImageObject _baseImage;
		[SerializeField]
		private int _height;
		[SerializeField]
		private int _width;
		[SerializeField]
		private List<PsdRootObject> _linkedRootObjects;
		[SerializeField]
		private List<AbstractPsdObject> _psdObjects;
		[SerializeField]
		public ResolutionInfo _resolution;
		[SerializeField]
		public ImageCompression _imageCompression;
		
		public PsdRootObject(string id, string name, Stream input, Context context) : this(id, name, new PsdFile(input, context)) { }

		public PsdRootObject(string id, string name, PsdFile psdFile) {
			_id = id;
			_name = name;
			// Multichannel images are loaded by processing each channel as a
			// grayscale layer.
			if (psdFile.ColorMode == PsdColorMode.Multichannel) {
				CreateLayersFromChannels(psdFile);
				psdFile.ColorMode = PsdColorMode.Grayscale;
			}

			_width = psdFile.ColumnCount;
			_height = psdFile.RowCount;
			_imageCompression = psdFile.ImageCompression;
			_resolution = psdFile.Resolution;
			_baseImage = new ImageObject(psdFile.BaseLayer, null);
			_linkedRootObjects = new List<PsdRootObject>();
			_psdObjects = new List<AbstractPsdObject>();

			psdFile.VerifyLayerSections();

			foreach (var additionalInfo in psdFile.AdditionalInfo) {
				if (additionalInfo is not LinkedFilesInfo linkedFilesInfo) continue;
				foreach (var linkedFile in linkedFilesInfo.LinkedFiles) {
					if (linkedFile.File == null) continue;
					_linkedRootObjects.Add(new PsdRootObject(linkedFile.ID,
						Path.GetFileNameWithoutExtension(linkedFile.Name.Remove(linkedFile.Name.Length - 1)),
						linkedFile.File));
				}
			}

			GroupObject parentObject = null;
			for (var i = psdFile.Layers.Count - 1; i >= 0; i--) {
				var psdFileLayer = psdFile.Layers[i];
				var sectionInfo = (LayerSectionInfo)psdFileLayer.AdditionalInfo
					.SingleOrDefault(x => x is LayerSectionInfo);
				AbstractPsdObject psdObject = null;
				if (sectionInfo == null || sectionInfo.SectionType == LayerSectionType.Layer) {
					if (psdFileLayer.AdditionalInfo.FirstOrDefault(i => i is TypeToolInfo) != null) {
						psdObject = new TextObject(psdFileLayer, parentObject);
					} else {
						psdObject = new ImageObject(psdFileLayer, parentObject);
					}
				} else if (sectionInfo.SectionType is LayerSectionType.ClosedFolder or LayerSectionType.OpenFolder) {
					psdObject = new GroupObject(psdFileLayer, parentObject);
				} else {
					parentObject = parentObject?.ParentObject;
				}
				if (psdObject != null) {
					if (parentObject != null) parentObject.AddChild(psdObject);
					else _psdObjects.Add(psdObject);	
				}
				if (psdObject is GroupObject groupObject) parentObject = groupObject;
			}
		}

		public void SaveAssets(SaveAssetsContext saveAssetsContext) {
			for (var i = 0; i < _linkedRootObjects.Count; i++) {
				var linkedRootObject = _linkedRootObjects[i];
				linkedRootObject.SaveAssets(saveAssetsContext);
			}

			for (var i = 0; i < _psdObjects.Count; i++) {
				var psdObject = _psdObjects[i];
				psdObject.SaveAssets(_name, saveAssetsContext);
			}
		}

		public void Save(Stream output, Context context) {
			/*var psdVersion = ((input.Height > 30000) || (input.Width > 30000))
				? PsdFileVersion.PsbLargeDocument
				: PsdFileVersion.Psd;
			var psdFile = new PsdFile(psdVersion);

			psdFile.RowCount = input.Height;
			psdFile.ColumnCount = input.Width;
			psdFile.ChannelCount = 4;
			psdFile.ColorMode = PsdColorMode.RGB;
			psdFile.BitDepth = 8;
			psdFile.Resolution = input.Resolution;
			psdFile.ImageCompression = input.ImageCompression;
			
			for (short i = 0; i < psdFile.ChannelCount; i++) {
				var channel = new Channel(i, psdFile.BaseLayer) {
					ImageData = new byte[input.BaseLayer.Pixels.Length],
					ImageCompression = psdFile.ImageCompression
				};
				psdFile.BaseLayer.Channels.Add(channel);
			}
			var channelsArray = psdFile.BaseLayer.Channels.ToIdArray();
			ImageEncoder.EncodeImage(channelsArray, channelsArray[3],
				input.BaseLayer.Pixels, psdFile.BaseLayer.Rect);

			foreach (var pdnLayer in input.Layers) {
				pdnLayer.WriteLayer(psdFile);
			}
			psdFile.Layers.Reverse();
			psdFile.Save(output, context);*/
		}
		
		private static void CreateLayersFromChannels(PsdFile psdFile) {
			if (psdFile.ColorMode != PsdColorMode.Multichannel) throw new Exception("Not a multichannel image.");
			if (psdFile.Layers.Count > 0) throw new PsdInvalidException("Multichannel image should not have layers.");
			// Get alpha channel names, preferably in Unicode.
			var alphaChannelNames = (AlphaChannelNames) psdFile.ImageResourceList.Get(ResourceID.AlphaChannelNames);
			var unicodeAlphaNames = (UnicodeAlphaNames) psdFile.ImageResourceList.Get(ResourceID.UnicodeAlphaNames);
			if (alphaChannelNames == null && unicodeAlphaNames == null) throw new PsdInvalidException("No channel names found.");

			var channelNames = unicodeAlphaNames != null ? unicodeAlphaNames.ChannelNames : alphaChannelNames.ChannelNames;
			var channels = psdFile.BaseLayer.Channels;
			if (channels.Count > channelNames.Count) throw new PsdInvalidException("More channels than channel names.");

			// Channels are stored from top to bottom, but layers are stored from bottom to top.
			var channelsNamesReversed = channels.Zip(channelNames, Tuple.Create).Reverse();
			foreach (var (channel, channelName) in channelsNamesReversed) {
				// Copy metadata over from base layer
				var layer = new Layer(psdFile) {
					Rect = psdFile.BaseLayer.Rect,
					Visible = true,
					Masks = new MaskInfo()
				};
				layer.BlendingRangesData = new BlendingRanges(layer);

				// We do not attempt to reconstruct the appearance of the image, but
				// only to provide access to the channels image data.
				layer.Name = channelName;
				layer.BlendModeKey = PsdBlendMode.Darken;
				layer.Opacity = 255;

				// Copy channel image data into the new grayscale layer
				var layerChannel = new Channel(0, layer) {
					ImageCompression = channel.ImageCompression,
					ImageData = channel.ImageData
				};
				layer.Channels.Add(layerChannel);
				psdFile.Layers.Add(layer);
			}
		}
	}
}