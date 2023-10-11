/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class EnumReferenceElement : AbstractReferenceElement {
		public const string OSType = "Enmr";
		
		private readonly string _classIdName;
		private readonly string _classId;
		private readonly string _typeId;
		private readonly string _enumValue;
		protected override string ElementType => OSType;

		public string ClassIdName => _classIdName;

		public string ClassId => _classId;

		public string TypeId => _typeId;

		public string EnumValue => _enumValue;

		public EnumReferenceElement(string classIdName, string classId, string typeId, string enumValue) {
			_classIdName = classIdName;
			_classId = classId;
			_typeId = typeId;
			_enumValue = enumValue;
		}

		public EnumReferenceElement(PsdBinaryReader reader) {
			_classIdName = reader.ReadUnicodeString();
			_classId = reader.ReadKey();
			_typeId = reader.ReadKey();
			_enumValue = reader.ReadKey();
		}
		
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_classIdName);
			writer.WriteKey(_classId);
			writer.WriteKey(_typeId);
			writer.WriteKey(_enumValue);
		}
	}
}