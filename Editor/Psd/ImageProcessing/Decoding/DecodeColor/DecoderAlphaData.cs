using Unity.Collections;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor {
	public struct DecoderAlphaData {
		public bool HasAlpha;
		public DecoderMaskData LayerMask;
		public DecoderMaskData UserMask;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> AlphaChannel;

		public byte GetAlpha(int srcIndex, DecoderByteData decoderByteData, int layerPixelX, int layerPixelY) {
			var alpha = HasAlpha ? AlphaChannel[srcIndex] : (byte)255;
			if (LayerMask.Present && !UserMask.Present) {
				alpha = (byte)(alpha * LayerMask.GetAlpha(decoderByteData, layerPixelX, layerPixelY) / 255);
			} else if (!LayerMask.Present && UserMask.Present) {
				alpha = (byte)(alpha * UserMask.GetAlpha(decoderByteData, layerPixelX, layerPixelY) / 255);
			} else if (LayerMask.Present && UserMask.Present) {
				alpha = (byte)(alpha * LayerMask.GetAlpha(decoderByteData, layerPixelX, layerPixelY) * UserMask.GetAlpha(decoderByteData, layerPixelX, layerPixelY) / 65025);
			}
			return alpha;
		}
	}
}