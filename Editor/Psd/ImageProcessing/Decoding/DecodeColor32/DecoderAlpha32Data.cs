using Unity.Collections;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor32 {
	public struct DecoderAlpha32Data {
		public bool HasAlpha;
		public DecoderMask32Data LayerMask;
		public DecoderMask32Data UserMask;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<float> AlphaChannel;

		public byte GetAlpha(int srcIndex, int layerPixelX, int layerPixelY) {
			var alpha = HasAlpha ?  ImageDecoder.RGBByteFromHDRFloat(AlphaChannel[srcIndex]) : (byte)255;
			if (LayerMask.Present && !UserMask.Present) {
				alpha = (byte)(alpha * LayerMask.GetAlpha(layerPixelX, layerPixelY) / 255);
			} else if (!LayerMask.Present && UserMask.Present) {
				alpha = (byte)(alpha * UserMask.GetAlpha(layerPixelX, layerPixelY) / 255);
			} else if (LayerMask.Present && UserMask.Present) {
				alpha = (byte)(alpha * LayerMask.GetAlpha(layerPixelX, layerPixelY) * UserMask.GetAlpha(layerPixelX, layerPixelY) / 65025);
			}
			return alpha;
		}
	}
}