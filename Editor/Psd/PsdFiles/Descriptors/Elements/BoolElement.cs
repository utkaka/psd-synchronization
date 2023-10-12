/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements {
	public class BoolElement : AbstractDescriptorElement {
		public const string OSType = "bool";
		private readonly bool _value;

		protected override string ElementType => OSType;

		public bool Value => _value;

		public BoolElement(bool value) {
			_value = value;
		}

		public BoolElement(PsdBinaryReader reader) {
			_value = reader.ReadBoolean();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_value);
		}
	}
}