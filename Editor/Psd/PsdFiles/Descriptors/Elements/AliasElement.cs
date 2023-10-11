/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class AliasElement : AbstractDescriptorElement {
		public const string OSType = "alis";
		
		private readonly byte[] _data;
		protected override string ElementType => OSType;

		public byte[] Data => _data;

		public AliasElement(byte[] data) {
			_data = data;
		}

		public AliasElement(PsdBinaryReader reader) {
			var length = reader.ReadInt32();
			_data = reader.ReadBytes(length);
		}
		
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_data.Length);
			writer.Write(_data);
		}
	}
}