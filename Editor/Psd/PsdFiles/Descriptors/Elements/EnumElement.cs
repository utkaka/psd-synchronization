/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.Psd.PsdFiles.Descriptors.Elements {
	public class EnumElement : AbstractDescriptorElement {
		public const string OSType = "enum";
		private readonly string _enumType;
		private readonly string _enumValue;

		protected override string ElementType => OSType;

		public string EnumType => _enumType;

		public string EnumValue => _enumValue;

		public EnumElement(string enumType, string enumValue) {
			_enumType = enumType;
			_enumValue = enumValue;
		}

		public EnumElement(PsdBinaryReader reader) {
			_enumType = reader.ReadKey();
			_enumValue = reader.ReadKey();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteKey(_enumType);
			writer.WriteKey(_enumValue);
		}
	}
}