/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class PropertyReferenceElement : AbstractReferenceElement {
		private readonly string _classIdName;
		private readonly string _classId;
		private readonly string _keyId;
		public const string OSType = "prop";
		protected override string ElementType => OSType;

		public string ClassIdName => _classIdName;

		public string ClassId => _classId;

		public string KeyId => _keyId;

		public PropertyReferenceElement(string classIdName, string classId, string keyId) {
			_classIdName = classIdName;
			_classId = classId;
			_keyId = keyId;
		}

		public PropertyReferenceElement(PsdBinaryReader reader) {
			_classIdName = reader.ReadUnicodeString();
			_classId = reader.ReadKey();
			_keyId = reader.ReadKey();
		}
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_classIdName);
			writer.WriteKey(_classId);
			writer.WriteKey(_keyId);
		}
	}
}