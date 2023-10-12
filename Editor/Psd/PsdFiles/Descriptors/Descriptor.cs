/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors {
	public class Descriptor {
		private readonly string _classIdName;
		private readonly string _classId;
		private readonly Dictionary<string, AbstractDescriptorElement> _items;

		public string ClassIdName => _classIdName;

		public string ClassId => _classId;

		public Dictionary<string, AbstractDescriptorElement> Items => _items;

		public Descriptor(string classIdName, string classId, Dictionary<string, AbstractDescriptorElement> items) {
			_classIdName = classIdName;
			_classId = classId;
			_items = items;
		}

		public Descriptor(PsdBinaryReader reader) {
			_items = new Dictionary<string, AbstractDescriptorElement>();
			_classIdName = reader.ReadUnicodeString();
			_classId = reader.ReadKey();
			var itemsCount = reader.ReadInt32();
			for (var i = 0; i < itemsCount; i++) {
				var key = reader.ReadKey();
				_items[key] = DescriptorElementFactory.Create(reader);
			}
		}

		public void Write(PsdBinaryWriter writer) {
			writer.WriteUnicodeString(_classIdName);
			writer.WriteKey(_classId);
			writer.Write(_items.Count);
			foreach (var keyValue in _items) {
				writer.WriteKey(keyValue.Key);
				keyValue.Value.Write(writer);
			}
		}
	}
}