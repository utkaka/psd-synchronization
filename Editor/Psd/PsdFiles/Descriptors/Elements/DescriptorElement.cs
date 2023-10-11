/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.utkaka.Psd.PsdFiles.Descriptors.Elements {
	public class DescriptorElement : AbstractDescriptorElement{
		public const string OSType = "Objc";
		private Descriptor _descriptor;
		protected override string ElementType => OSType;
		public Descriptor Descriptor => _descriptor;

		public DescriptorElement(string classIdName, string classId, Dictionary<string, AbstractDescriptorElement> items) {
			_descriptor = new Descriptor(classIdName, classId, items);
		}

		public DescriptorElement(PsdBinaryReader reader) {
			_descriptor = new Descriptor(reader);
		}
		
		protected override void WriteBody(PsdBinaryWriter writer) {
			_descriptor.Write(writer);
		}
	}
}