using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace com.utkaka.Psd.ImageProcessing.Decoding.DecodeColor {
	[BurstCompile]
	public struct JobDecodeCmyk : IJobParallelFor {
		private DecoderChannelsData _channels;
		private DecoderAlphaData _alphaData;
		private ImageLayerData _layerData;
		private DecoderByteData _byteData;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		private NativeArray<Color32> _pixels;

		public JobDecodeCmyk(DecoderChannelsData channels, DecoderAlphaData alphaData, ImageLayerData layerData, DecoderByteData byteData, NativeArray<Color32> pixels) {
			_channels = channels;
			_alphaData = alphaData;
			_layerData = layerData;
			_byteData = byteData;
			_pixels = pixels;
		}

		public void Execute(int index) {
			var srcIndex = _byteData.GetShiftedIndex(index);
			var layerPixelX = _layerData.GetInLayerX(index);
			var layerPixelY = _layerData.GetInLayerY(index);
			var alpha = _alphaData.GetAlpha(index, _byteData, layerPixelX, layerPixelY);
			
			var c = 255 - _channels.Channel0[srcIndex];
			var m = 255 - _channels.Channel1[srcIndex];
			var y = 255 - _channels.Channel2[srcIndex];
			var k = 255 - _channels.Channel3[srcIndex];

			var nRed = 255 - math.min(255, c * (255 - k) / 255 + k);
			var nGreen = 255 - math.min(255, m * (255 - k) / 255 + k);
			var nBlue = 255 - math.min(255, y * (255 - k) / 255 + k);
			
			_pixels[_layerData.GetIndexInvertedByY(layerPixelX, layerPixelY)] = new Color32((byte) nRed, (byte) nGreen, (byte) nBlue, alpha);
		}
	}
}