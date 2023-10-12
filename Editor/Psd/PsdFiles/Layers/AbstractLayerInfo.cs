﻿/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers {
	internal static class LayerInfoFactory {
		internal static void LoadAll(PsdBinaryReader reader, PsdFile psdFile,
			List<AbstractLayerInfo> layerInfoList, long endPosition, bool globalLayerInfo) {
			// LayerInfo has a 12-byte minimum length.  Anything shorter should be
			// ignored as padding.
			while (endPosition - reader.BaseStream.Position >= 12) {
				var layerInfo = Load(reader, psdFile, globalLayerInfo);
				layerInfoList.Add(layerInfo);
			}

			if (reader.BaseStream.Position < endPosition) {
				reader.BaseStream.Position = endPosition;
			} else if (reader.BaseStream.Position > endPosition) {
				throw new PsdInvalidException(
					"Read past the end of the LayerInfo fields.");
			}
		}

		/// <summary>
		/// Loads the next LayerInfo record.
		/// </summary>
		/// <param name="reader">The file reader</param>
		/// <param name="psdFile">The PSD file.</param>
		/// <param name="globalLayerInfo">True if the LayerInfo record is being
		///   loaded from the end of the Layer and Mask Information section;
		///   false if it is being loaded from the end of a Layer record.</param>
		/// <returns>LayerInfo object if it was successfully read, or null if
		///   padding was found.</returns>
		private static AbstractLayerInfo Load(PsdBinaryReader reader, PsdFile psdFile,
			bool globalLayerInfo) {
			reader.Log(LogType.Log, "Load, Begin, LayerInfo");

			// Most keys have undocumented signatures, so we always accept either one.
			var signature = reader.ReadAsciiChars(4);
			if ((signature != "8BIM") && (signature != "8B64")) {
				throw new PsdInvalidException(
					$"{nameof(AbstractLayerInfo)} signature invalid, must be 8BIM or 8B64.");
			}

			var key = reader.ReadAsciiChars(4);
			var hasLongLength = LayerInfoUtil.HasLongLength(signature, key, psdFile.IsLargeDocument);
			var length = hasLongLength
				? reader.ReadInt64()
				: reader.ReadInt32();
			var startPosition = reader.BaseStream.Position;
			AbstractLayerInfo result;
			switch (key) {
				case "Layr":
				case "Lr16":
				case "Lr32":
					result = new InfoLayers(reader, psdFile, key, length);
					break;
				case "lsct":
				case "lsdk":
					result = new LayerSectionInfo(reader, key, (int) length);
					break;
				case "luni":
					result = new LayerUnicodeName(reader);
					break;
				case "TySh":
					result = new TypeToolInfo(reader);
					break;
				case "SoLd":
					result = new PlacedLayerInfo(reader);
					break;
				case "lyid":
					result = new LayerIdInfo(reader);
					break;
				case "lnkD":
				case "lnkE":
				case "lnk2":
				case "lnk3":
					result = new LinkedFilesInfo(key, reader, length);
					break;
				default:
					result = new RawLayerInfo(reader, signature, key, length);
					break;
			}

			// May have additional padding applied.
			var endPosition = startPosition + length;
			if (reader.BaseStream.Position < endPosition) {
				reader.BaseStream.Position = endPosition;
			}

			// Documentation states that the length is even-padded.  Actually:
			//   1. Most keys have 4-padded lengths.
			//   2. However, some keys (LMsk) have even-padded lengths.
			//   3. Other keys (Txt2, Lr16, Lr32) have unpadded lengths.
			//
			// Photoshop writes data that is always 4-padded, even when the stated
			// length is not a multiple of 4.  The length mismatch seems to occur
			// only on global layer info.  We do not read extra padding in other
			// cases because third-party programs are likely to follow the spec.

			if (globalLayerInfo) {
				reader.ReadPadding(startPosition, 4);
			}

			reader.Log(LogType.Log, $"Load, End, LayerInfo, {result.Signature}, {result.Key}");
			return result;
		}
	}

	internal static class LayerInfoUtil {
		internal static bool HasLongLength(string signature, string key, bool isLargeDocument) {
			if (!isLargeDocument) {
				return false;
			}

			// Keys with 8B64 signatures always have 8-byte lengths in PSB files.
			if (signature == "8B64") {
				return true;
			}

			switch (key) {
				// These keys are documented to have 8-byte lengths in PSB files.  Some
				// keys have 8BIM signatures.  Other keys have 8B64 signatures, but this
				// fact is undocumented, so they are still hardcoded in the list.
				case "LMsk":
				case "Lr16":
				case "Lr32":
				case "Layr":
				case "Mt16":
				case "Mt32":
				case "Mtrn":
				case "Alph":
				case "FMsk":
				case "lnk2":
				case "FEid":
				case "FXid":
				case "PxSD":
					return true;

				default:
					return false;
			}
		}
	}

	public abstract class AbstractLayerInfo {
		public abstract string Signature { get; }

		public abstract string Key { get; }

		protected abstract void WriteData(PsdBinaryWriter writer);

		public void Save(PsdBinaryWriter writer, bool globalLayerInfo,
			bool isLargeDocument) {
			writer.Log(LogType.Log, $"Save, Begin, LayerInfo, {Signature}, {Key}");

			writer.WriteAsciiChars(Signature);
			writer.WriteAsciiChars(Key);

			var startPosition = writer.BaseStream.Position;
			using (var lengthWriter = new PsdBlockLengthWriter(writer,
				       LayerInfoUtil.HasLongLength(Signature, Key, isLargeDocument))) {
				// Depending on the key, the length may be unpadded, 2-padded, or
				// 4-padded.  Thus, it is up to each implementation of WriteData to
				// pad the length correctly.
				WriteData(writer);
			}

			// Data for global layer info is always padded to a multiple of 4,
			// even if this causes the stated length to be incorrect.
			if (globalLayerInfo) {
				writer.WritePadding(startPosition, 4);
			}

			writer.Log(LogType.Log, $"Save, End, LayerInfo, {Signature}, {Key}");
		}
	}
}