/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements.ReferenceElements {
	public class IndexReferenceElement : AbstractReferenceElement {
		public const string OSType = "indx";
		protected override string ElementType => OSType;
		
		public IndexReferenceElement(PsdBinaryReader reader) {
			throw new System.NotImplementedException();
		}
		protected override void WriteBody(PsdBinaryWriter writer) {
			throw new System.NotImplementedException();
		}
	}
}