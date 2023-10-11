/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class ClassReferenceElement : AbstractReferenceElement {
		public const string OSType = "Clss";
		private readonly string _name;
		private readonly string _key;

		public string Name => _name;

		public string Key => _key;
		protected override string ElementType => OSType;
		
		public ClassReferenceElement(string name, string key) {
			_name = name;
			_key = key;
		}

		public ClassReferenceElement(PsdBinaryReader reader) {
			_name = reader.ReadUnicodeString();
			_key = reader.ReadKey();
		}
		
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_name);
			writer.WriteKey(_key);
		}
	}
}