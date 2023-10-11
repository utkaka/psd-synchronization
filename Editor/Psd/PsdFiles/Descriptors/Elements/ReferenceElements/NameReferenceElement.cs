/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.Psd.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class NameReferenceElement : AbstractReferenceElement {
		public const string OSType = "name";
		protected override string ElementType => OSType;
		
		public NameReferenceElement(PsdBinaryReader reader) {
			throw new System.NotImplementedException();
		}
		protected override void WriteBody(PsdBinaryWriter writer) {
			throw new System.NotImplementedException();
		}
	}
}