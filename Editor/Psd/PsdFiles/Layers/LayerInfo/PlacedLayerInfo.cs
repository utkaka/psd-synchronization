/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo {
	public class PlacedLayerInfo : AbstractLayerInfo {
		public override string Key => "SoLd";
		public override string Signature => "8BIM";
		
		private readonly string _id;
		private readonly string _type;
		private readonly int _version;
		private readonly int _descriptorVersion;
		private readonly Descriptor _descriptor;
		private readonly Matrix3X3 _matrix;
		
		public string Id => _id;
		public Matrix3X3 Matrix => _matrix;

		public PlacedLayerInfo(PsdBinaryReader reader) {
			_type = reader.ReadAsciiChars(4);
			_version = reader.ReadInt32();
			_descriptorVersion = reader.ReadInt32();
			_descriptor = new Descriptor(reader);
			_id = ((TextElement) _descriptor.Items["Idnt"]).Value;
			_id = _id[..^1];
			var transformPoints =
				((ListElement) _descriptor.Items["Trnf"]).Items.Select(p => ((DoubleElement) p).Value).ToArray();

			_matrix = new Matrix3X3(transformPoints);
		}
		
		protected override void WriteData(PsdBinaryWriter writer) {
			writer.WriteAsciiChars(_type);
			writer.Write(_version);
			writer.Write(_descriptorVersion);
			_descriptor.Write(writer);
		}
	}
}