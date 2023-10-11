using Unity.Collections;

namespace com.utkaka.Psd.ImageProcessing.Decoding.DecodeColor {
	public struct DecoderMaskData {
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		private NativeArray<byte> _alphaChannel;
		
		private readonly bool _present;
		private readonly int _inLayerX;
		private readonly int _inLayerY;
		private readonly int _width;
		private readonly int _height;
		private readonly int _layerWidth;

		public bool Present => _present;

		public DecoderMaskData(NativeArray<byte> alphaChannel) {
			_present = false;
			_inLayerX = 0;
			_inLayerY = 0;
			_width = 0;
			_height = 0;
			_layerWidth = 0;
			_alphaChannel = alphaChannel;
		}

		public DecoderMaskData(int inLayerX, int inLayerY, int width, int height, int layerWidth, NativeArray<byte> alphaChannel) {
			_present = true;
			_inLayerX = inLayerX;
			_inLayerY = inLayerY;
			_width = width;
			_height = height;
			_layerWidth = layerWidth;
			_alphaChannel = alphaChannel;
		}

		public byte GetAlpha(DecoderByteData decoderByteData, int layerPixelX, int layerPixelY) {
			if (layerPixelX < _inLayerX || layerPixelY < _inLayerY || layerPixelX >= _inLayerX + _width ||
			    layerPixelY >= _inLayerY + _height) {
				return 255;
			}
			var maskPixelX = layerPixelX - _inLayerX;
			var maskPixelY = layerPixelY - _inLayerY;

			var srcIndex = decoderByteData.GetShiftedIndex(maskPixelY * _width + maskPixelX);
			return _alphaChannel[srcIndex];
		}
	}
}