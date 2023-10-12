/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements {
	public class UnitFloatElement : AbstractDescriptorElement {
		public const string OSType = "UntF";
		private readonly string _type;
		private readonly double _value;

		protected override string ElementType => OSType;

		public string Type => _type;

		public double Value => _value;

		public UnitFloatElement(string type, double value) {
			_type = type;
			_value = value;
		}

		public UnitFloatElement(PsdBinaryReader reader) {
			_type = reader.ReadAsciiChars(4);
			_value = reader.ReadDouble();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteAsciiChars(_type);
			writer.Write(_value);
		}
	}
}