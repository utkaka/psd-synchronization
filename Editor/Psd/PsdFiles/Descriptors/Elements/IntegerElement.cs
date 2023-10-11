/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class IntegerElement : AbstractDescriptorElement {
		public const string OSType = "long";
		private readonly int _value;

		protected override string ElementType => OSType;

		public int Value => _value;

		public IntegerElement(int value) {
			_value = value;
		}

		public IntegerElement(PsdBinaryReader reader) {
			_value = reader.ReadInt32();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_value);
		}
	}
}