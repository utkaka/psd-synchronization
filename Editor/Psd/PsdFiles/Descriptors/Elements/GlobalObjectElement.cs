/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class GlobalObjectElement : DescriptorElement {
		public const string OSType = "GlbO";
		protected override string ElementType => OSType;
		
		public GlobalObjectElement(string classIdName, string classId, Dictionary<string, AbstractDescriptorElement> items) : base(classIdName, classId, items) { }
		public GlobalObjectElement(PsdBinaryReader reader) : base(reader) { }
	}
}