/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements {
	public class RawDataElement : AbstractDescriptorElement {
		public const string OSType = "tdta";
		private readonly byte[] _rawData;

		protected override string ElementType => OSType;

		public byte[] RawData => _rawData;

		public RawDataElement(byte[] rawData) {
			_rawData = rawData;
		}

		public RawDataElement(PsdBinaryReader reader) {
			var length = reader.ReadInt32();
			_rawData = reader.ReadBytes(length);
		}

		protected override void WriteBody(PsdBinaryWriter writer) {
			writer.Write(_rawData.Length);
			writer.Write(_rawData);
		}
	}
}