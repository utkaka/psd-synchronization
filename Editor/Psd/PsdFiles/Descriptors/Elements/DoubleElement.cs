/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class DoubleElement : AbstractDescriptorElement {
		public const string OSType = "doub";
		private readonly double _value;
		protected override string ElementType => OSType;

		public double Value => _value;

		public DoubleElement(double value) {
			_value = value;
		}

		public DoubleElement(PsdBinaryReader reader) {
			_value = reader.ReadDouble();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_value);
		}
	}
}