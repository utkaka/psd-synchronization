/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdPlugin.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class IdentifierReferenceElement : AbstractReferenceElement {
		public const string OSType = "Idnt";
		protected override string ElementType => OSType;
		
		public IdentifierReferenceElement(PsdBinaryReader reader) {
			throw new System.NotImplementedException();
		}
		protected override void WriteBody(PsdBinaryWriter writer) {
			throw new System.NotImplementedException();
		}
	}
}