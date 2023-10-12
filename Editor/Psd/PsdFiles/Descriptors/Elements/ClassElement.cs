/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements {
	public class ClassElement : AbstractDescriptorElement {
		public const string OSType = "type";
		private readonly string _name;
		private readonly string _key;

		public string Name => _name;

		public string Key => _key;
		protected override string ElementType => OSType;

		public ClassElement(string name, string key) {
			_name = name;
			_key = key;
		}

		public ClassElement(PsdBinaryReader reader) {
			_name = reader.ReadUnicodeString();
			_key = reader.ReadKey();
		}
		
		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_name);
			writer.WriteKey(_key);
		}
	}
}