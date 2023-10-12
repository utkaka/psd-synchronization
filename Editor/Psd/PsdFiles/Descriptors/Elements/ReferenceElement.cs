/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements.ReferenceElements;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements {
	public class ReferenceElement : AbstractDescriptorElement {
		public const string OSType = "obj";
		
		private readonly AbstractReferenceElement[] _items;
		protected override string ElementType => OSType;

		public AbstractReferenceElement[] Items => _items;

		public ReferenceElement(AbstractReferenceElement[] items) {
			_items = items;
		}

		public ReferenceElement(PsdBinaryReader reader) {
			var itemsCount = reader.ReadInt32();
			_items = new AbstractReferenceElement[itemsCount];
			for (var i = 0; i < itemsCount; i++) {
				_items[i] = ReferenceElementFactory.Create(reader);
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