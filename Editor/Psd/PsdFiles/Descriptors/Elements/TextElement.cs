/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class TextElement : AbstractDescriptorElement {
		public const string OSType = "TEXT";
		private readonly string _value;

		protected override string ElementType => OSType;

		public string Value => _value;

		public TextElement(string value) {
			_value = value;
		}

		public TextElement(PsdBinaryReader reader) {
			_value = reader.ReadUnicodeString();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_value);
		}
	}
}