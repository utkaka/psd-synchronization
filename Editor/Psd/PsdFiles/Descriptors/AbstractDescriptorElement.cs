/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements;

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors {
	public abstract class AbstractDescriptorElement {
		protected abstract string ElementType { get; }

		public void Write(PsdBinaryWriter writer) {
			writer.WriteAsciiChars(ElementType);
			WriteBody(writer);
		}

		protected abstract void WriteBody(PsdBinaryWriter writer);
	}
	
	public class DescriptorElementFactory {
		public static AbstractDescriptorElement Create(PsdBinaryReader reader) {
			var elementType = reader.ReadAsciiChars(4);
			switch (elementType) {
	            case ReferenceElement.OSType:
		            return new ReferenceElement(reader);
	            case DescriptorElement.OSType:
            		return new DescriptorElement(reader);
	            case ListElement.OSType:
		            return new ListElement(reader);
	            case DoubleElement.OSType:
		            return new DoubleElement(reader);
	            case UnitFloatElement.OSType:
		            return new UnitFloatElement(reader);
	            case TextElement.OSType:
		            return new TextElement(reader);
	            case EnumElement.OSType:
		            return new EnumElement(reader);
	            case IntegerElement.OSType:
		            return new IntegerElement(reader);
	            case LongElement.OSType:
		            return new LongElement(reader);
	            case BoolElement.OSType:
		            return new BoolElement(reader);
	            case GlobalObjectElement.OSType:
		            return new GlobalObjectElement(reader);
	            case ClassElement.OSType:
		            return new ClassElement(reader);
	            case GlobalClassElement.OSType:
		            return new GlobalClassElement(reader);
	            case AliasElement.OSType:
		            return new AliasElement(reader);
	            case RawDataElement.OSType:
		            return new RawDataElement(reader);
	            case ObjectArrayElement.OSType:
		            return new ObjectArrayElement(reader);
	            default:
		            throw new NotImplementedException($"Unknown descriptor element type {elementType}");
			}
		}
	}
}