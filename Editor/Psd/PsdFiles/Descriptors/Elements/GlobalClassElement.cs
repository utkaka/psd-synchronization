/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements {
	public class GlobalClassElement : ClassElement {
		public const string OSType = "GlbC";
		protected override string ElementType => OSType;
		
		public GlobalClassElement(string name, string key) : base(name, key) { }
		public GlobalClassElement(PsdBinaryReader reader) : base(reader) { }
	}
}