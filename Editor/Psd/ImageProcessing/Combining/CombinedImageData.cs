namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Combining {
	public struct CombinedImageData {
		public readonly byte Opacity;
		private readonly int _offset;
		private readonly int _inLayerX;
		private readonly int _inLayerY;
		private readonly int _width;
		private readonly int _height;

		public CombinedImageData(int offset, int inLayerX, int inLayerY, int width, int height, byte opacity) {
			_offset = offset;
			_inLayerX = inLayerX;
			_inLayerY = inLayerY;
			_width = width;
			_height = height;
			Opacity = opacity;
		}

		public int GetPixelIndex(int layerPixelX, int layerPixelY) {
			if (layerPixelX < _inLayerX || layerPixelY < _inLayerY || layerPixelX >= _inLayerX + _width ||
			    layerPixelY >= _inLayerY + _height) {
				return -1;
			}
			var imagePixelX = layerPixelX - _inLayerX;
			var imagePixelY = layerPixelY - _inLayerY;
			return _offset + imagePixelY * _width + imagePixelX;
		}
	}
}