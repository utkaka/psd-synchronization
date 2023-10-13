using Unity.Mathematics;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing {
	public struct ImageLayerData {
		private int _width;
		private int _height;

		public ImageLayerData(int width, int height) {
			_width = width;
			_height = height;
		}

		public int GetInLayerX(int index) {
			return index % _width;
		}
		
		public int GetInLayerY(int index) {
			return index / _width;
		}
		
		public int GetIndexInvertedByY(int inLayerX, int inLayerY) {
			return (_height - inLayerY - 1) * _width + inLayerX;
		}
	}
}