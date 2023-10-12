/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements.ReferenceElements {
	public abstract class AbstractReferenceElement : AbstractDescriptorElement{ }
	
	public class ReferenceElementFactory {
		public static AbstractReferenceElement Create(PsdBinaryReader reader) {
			var elementType = reader.ReadAsciiChars(4);
			switch (elementType) {
				case PropertyReferenceElement.OSType:
					return new PropertyReferenceElement(reader);
				case ClassReferenceElement.OSType:
					return new ClassReferenceElement(reader);
				case EnumReferenceElement.OSType:
					return new EnumReferenceElement(reader);
				case OffsetReferenceElement.OSType:
					return new OffsetReferenceElement(reader);
				case IdentifierReferenceElement.OSType:
					return new IdentifierReferenceElement(reader);
				case IndexReferenceElement.OSType:
					return new IndexReferenceElement(reader);
				case NameReferenceElement.OSType:
					return new NameReferenceElement(reader);
			}

			return null;
		}
	}
}