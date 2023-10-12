/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class OffsetReferenceElement : AbstractReferenceElement {
		public const string OSType = "rele";
		private readonly string _classIdName;
		private readonly string _classId;
		private readonly int _value;
		protected override string ElementType => OSType;

		public string ClassIdName => _classIdName;

		public string ClassId => _classId;

		public int Value => _value;

		public OffsetReferenceElement(string classIdName, string classId, int value) {
			_classIdName = classIdName;
			_classId = classId;
			_value = value;
		}

		public OffsetReferenceElement(PsdBinaryReader reader) {
			_classIdName = reader.ReadUnicodeString();
			_classId = reader.ReadKey();
			_value = reader.ReadInt32();
		}
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_classIdName);
			writer.WriteKey(_classId);
			writer.Write(_value);
		}
	}
}