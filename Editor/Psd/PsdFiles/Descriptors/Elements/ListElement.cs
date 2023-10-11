/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.Psd.PsdFiles.Descriptors.Elements {
	public class ListElement : AbstractDescriptorElement {
		public const string OSType = "VlLs";
		private readonly AbstractDescriptorElement[] _items; 
		protected override string ElementType => OSType;

		public AbstractDescriptorElement[] Items => _items;

		public ListElement(AbstractDescriptorElement[] items) {
			_items = items;
		}

		public ListElement(PsdBinaryReader reader) {
			var length = reader.ReadInt32();
			_items = new AbstractDescriptorElement[length];
			for (var i = 0; i < length; i++) {
				_items[i] = DescriptorElementFactory.Create(reader);
			}
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_items.Length);
			for (var i = 0; i < _items.Length; i++) {
				_items[i].Write(writer);
			}
		}
	}
}