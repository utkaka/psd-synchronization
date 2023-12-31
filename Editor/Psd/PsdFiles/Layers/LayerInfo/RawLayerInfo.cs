﻿/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2017 Tao Yue
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo {
	[DebuggerDisplay("Layer Info: { key }")]
	public class RawLayerInfo : AbstractLayerInfo {
		private string signature;
		public override string Signature => signature;

		private string key;
		public override string Key => key;

		public byte[] Data { get; private set; }

		public RawLayerInfo(string key, string signature = "8BIM") {
			this.signature = signature;
			this.key = key;
		}

		public RawLayerInfo(PsdBinaryReader reader, string signature, string key,
			long dataLength) {
			this.signature = signature;
			this.key = key;

			Util.CheckByteArrayLength(dataLength);
			Data = reader.ReadBytes((int) dataLength);
		}

		protected override void WriteData(PsdBinaryWriter writer) {
			writer.Write(Data);
		}
	}
}