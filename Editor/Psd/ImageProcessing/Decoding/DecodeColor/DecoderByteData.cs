namespace com.utkaka.Psd.ImageProcessing.Decoding.DecodeColor {
	public struct DecoderByteData {
		private int _shift;
		private int _byteDepth;

		public DecoderByteData(int byteDepth, int shift) {
			_byteDepth = byteDepth;
			_shift = shift;
		}

		public int GetShiftedIndex(int index) {
			return index * _byteDepth + _shift;
		}
	}
}