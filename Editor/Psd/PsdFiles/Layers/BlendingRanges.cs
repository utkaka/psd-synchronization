/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers {
	public class BlendingRanges {
		/// <summary>
		/// The layer to which this channel belongs
		/// </summary>
		public Layer Layer { get; private set; }

		public byte[] Data { get; set; }

		///////////////////////////////////////////////////////////////////////////

		public BlendingRanges(Layer layer) {
			Layer = layer;
			Data = new byte[0];
		}

		///////////////////////////////////////////////////////////////////////////

		public BlendingRanges(PsdBinaryReader reader, Layer layer) {
			reader.Log(LogType.Log, "Load, Begin, BlendingRanges");

			Layer = layer;
			var dataLength = reader.ReadInt32();
			if (dataLength <= 0) {
				return;
			}

			Data = reader.ReadBytes(dataLength);

			reader.Log(LogType.Log, "Load, End, BlendingRanges");
		}

		///////////////////////////////////////////////////////////////////////////

		public void Save(PsdBinaryWriter writer) {
			writer.Log(LogType.Log, "Save, Begin, BlendingRanges");

			if (Data == null) {
				writer.Write((UInt32) 0);
				return;
			}

			writer.Write((UInt32) Data.Length);
			writer.Write(Data);

			writer.Log(LogType.Log, "Save, End, BlendingRanges");
		}
	}
}