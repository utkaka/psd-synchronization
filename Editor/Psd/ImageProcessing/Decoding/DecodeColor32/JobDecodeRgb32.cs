using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor32 {
	[BurstCompile]
	public struct JobDecodeRgb32 : IJobParallelFor {
		private DecoderChannels32Data _channels;
		private DecoderAlpha32Data _alphaData;
		private ImageLayerData _layerData;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		private NativeArray<Color32> _pixels;

		public JobDecodeRgb32(DecoderChannels32Data channels, DecoderAlpha32Data alphaData, ImageLayerData imageLayerData, NativeArray<Color32> pixels) {
			_channels = channels;
			_alphaData = alphaData;
			_pixels = pixels;
			_layerData = imageLayerData;
		}

		public void Execute(int index) {
			var layerPixelX = _layerData.GetInLayerX(index);
			var layerPixelY = _layerData.GetInLayerY(index);
			var alpha = _alphaData.GetAlpha(index, layerPixelX, layerPixelY);
			
			_pixels[_layerData.GetIndexInvertedByY(layerPixelX, layerPixelY)] = new Color32(ImageDecoder.RGBByteFromHDRFloat(_channels.Channel0[index]),
				ImageDecoder.RGBByteFromHDRFloat(_channels.Channel1[index]),
				ImageDecoder.RGBByteFromHDRFloat(_channels.Channel2[index]), alpha);
		}
	}
}