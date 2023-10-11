using System.Collections.Generic;

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class ObjectArrayItem {
		private readonly string _key;
		private readonly string _type;
		private readonly string _units;
		private readonly double[] _values;

		public string Key => _key;

		public string Type => _type;

		public string Units => _units;

		public double[] Values => _values;

		public ObjectArrayItem(PsdBinaryReader reader) {
			_key = reader.ReadKey(); // type Hrzn | Vrtc
			_type = reader.ReadAsciiChars(4); // UnFl
			_units = reader.ReadAsciiChars(4); // units ? '#Pxl'
			var valuesCount = reader.ReadInt32();
			_values = new double[valuesCount];
			for (var i = 0; i < valuesCount; i++) {
				_values[i] = reader.ReadDouble();
			}
		}
		
		public void WriteBody(PsdBinaryWriter writer) {
			writer.WriteKey(_key);
			writer.WriteAsciiChars(_type);
			writer.WriteAsciiChars(_units);
			writer.Write(_values.Length);
			foreach (var value in _values) {
				writer.Write(value);
			}
		}
	}
	
	public class ObjectArrayElement : AbstractDescriptorElement {
		public const string OSType = "ObAr";
		private readonly int _version;
		private readonly string _name;
		private readonly string _key;
		private readonly Dictionary<string, ObjectArrayItem> _items;
		protected override string ElementType => OSType;

		public ObjectArrayElement(PsdBinaryReader reader) {
			_version = reader.ReadInt32();
			_name = reader.ReadUnicodeString();
			_key = reader.ReadKey();
			var length = reader.ReadInt32();

			_items = new Dictionary<string, ObjectArrayItem>();
			for (var i = 0; i < length; i++) {
				var item = new ObjectArrayItem(reader);
				_items.Add(item.Key, item);
			}
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_version);
			writer.WriteUnicodeString(_name);
			writer.WriteKey(_key);
			writer.Write(_items.Count);
			foreach (var item in _items.Values) {
				item.WriteBody(writer);
			}
		}
	}
}