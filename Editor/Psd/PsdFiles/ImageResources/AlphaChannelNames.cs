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

using System;
using System.Collections.Generic;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.ImageResources {
	/// <summary>
	/// The names of the alpha channels
	/// </summary>
	public class AlphaChannelNames : ImageResource {
		public override ResourceID ID => ResourceID.AlphaChannelNames;

		private List<string> channelNames = new List<string>();
		public List<string> ChannelNames => channelNames;

		public AlphaChannelNames() : base(String.Empty) { }

		public AlphaChannelNames(PsdBinaryReader reader, string name, int resourceDataLength)
			: base(name) {
			var endPosition = reader.BaseStream.Position + resourceDataLength;

			// Alpha channel names are Pascal strings, with no padding in-between.
			while (reader.BaseStream.Position < endPosition) {
				var channelName = reader.ReadPascalString(1);
				ChannelNames.Add(channelName);
			}
		}

		protected override void WriteData(PsdBinaryWriter writer) {
			foreach (var channelName in ChannelNames) {
				writer.WritePascalString(channelName, 1);
			}
		}
	}
}