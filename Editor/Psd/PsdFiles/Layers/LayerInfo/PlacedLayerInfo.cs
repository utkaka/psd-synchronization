/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo {
	public class PlacedLayerInfo : AbstractLayerInfo {
		public override string Key => "SoLd";
		public override string Signature => "8BIM";

		public string Id => _id;
		
		private readonly string _id;
		private readonly string _type;
		private readonly int _version;
		private readonly int _descriptorVersion;
		private readonly Descriptor _descriptor;
		
		public PlacedLayerInfo(PsdBinaryReader reader) {
			_type = reader.ReadAsciiChars(4);
			_version = reader.ReadInt32();
			_descriptorVersion = reader.ReadInt32();
			_descriptor = new Descriptor(reader);
			_id = ((TextElement) _descriptor.Items["Idnt"]).Value;
			_id = _id[..^1];
		}
		
		protected override void WriteData(PsdBinaryWriter writer) {
			writer.WriteAsciiChars(_type);
			writer.Write(_version);
			writer.Write(_descriptorVersion);
			_descriptor.Write(writer);
		}
	}
}