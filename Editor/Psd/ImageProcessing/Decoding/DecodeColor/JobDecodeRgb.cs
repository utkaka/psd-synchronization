using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor {
	[BurstCompile]
	public struct JobDecodeRgb : IJobParallelFor {
		private DecoderChannelsData _channels;
		private DecoderAlphaData _alphaData;
		private ImageLayerData _layerData;
		private DecoderByteData _byteData;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		private NativeArray<Color32> _pixels;

		public JobDecodeRgb(DecoderChannelsData channels, DecoderAlphaData alphaData, ImageLayerData layerData, DecoderByteData byteData, NativeArray<Color32> pixels) {
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
			var alpha = _alphaData.GetAlpha(srcIndex, _byteData, layerPixelX, layerPixelY);
			_pixels[_layerData.GetIndexInvertedByY(layerPixelX, layerPixelY)] = new Color32(_channels.Channel0[srcIndex], _channels.Channel1[srcIndex],
				_channels.Channel2[srcIndex], alpha);
		}
	}
}