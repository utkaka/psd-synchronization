/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.Psd.PsdFiles.Descriptors.Elements {
	public class LongElement : AbstractDescriptorElement {
		public const string OSType = "comp";
		private readonly long _value;

		protected override string ElementType => OSType;

		public long Value => _value;

		public LongElement(long value) {
			_value = value;
		}

		public LongElement(PsdBinaryReader reader) {
			_value = reader.ReadInt64();
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_value);
		}
	}
}