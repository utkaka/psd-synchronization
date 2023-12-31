using Unity.Collections;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor32 {
	public struct DecoderMask32Data {
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		private NativeArray<float> _alphaChannel;
		
		private readonly bool _present;
		private readonly int _inLayerX;
		private readonly int _inLayerY;
		private readonly int _width;
		private readonly int _height;

		public bool Present => _present;

		public DecoderMask32Data(NativeArray<float> alphaChannel) {
			_present = false;
			_inLayerX = 0;
			_inLayerY = 0;
			_width = 0;
			_height = 0;
			_alphaChannel = alphaChannel;
		}

		public DecoderMask32Data(int inLayerX, int inLayerY, int width, int height, int layerWidth, NativeArray<float> alphaChannel) {
			_present = true;
			_inLayerX = inLayerX;
			_inLayerY = inLayerY;
			_width = width;
			_height = height;
			_alphaChannel = alphaChannel;
		}

		public byte GetAlpha(int layerPixelX, int layerPixelY) {
			if (layerPixelX < _inLayerX || layerPixelY < _inLayerY || layerPixelX >= _inLayerX + _width ||
			    layerPixelY >= _inLayerY + _height) {
				return 255;
			}
			var maskPixelX = layerPixelX - _inLayerX;
			var maskPixelY = layerPixelY - _inLayerY;
			
			return ImageDecoder.RGBByteFromHDRFloat(_alphaChannel[maskPixelY * _width + maskPixelX]);
		}
	}
}