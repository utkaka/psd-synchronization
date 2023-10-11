using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace com.utkaka.Psd.ImageProcessing.Decoding.DecodeColor {
	[BurstCompile]
	public struct JobDecodeLab : IJobParallelFor {
		private DecoderChannelsData _channels;
		private DecoderAlphaData _alphaData;
		private ImageLayerData _layerData;
		private DecoderByteData _byteData;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		private NativeArray<Color32> _pixels;

		public JobDecodeLab(DecoderChannelsData channels, DecoderAlphaData alphaData, ImageLayerData layerData, DecoderByteData byteData, NativeArray<Color32> pixels) {
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

			double exL = _channels.Channel0[srcIndex];
			double exA = _channels.Channel1[srcIndex];
			double exB = _channels.Channel2[srcIndex];

			var l = (int) (exL / 2.55);
			var a = (int) (exA - 127.5);
			var b = (int) (exB - 127.5);

			// First, convert from Lab to XYZ.
			// Standards used Observer = 2, Illuminant = D65

			const double refX = 95.047;
			const double refY = 100.000;
			const double refZ = 108.883;

			var varY = (l + 16.0) / 116.0;
			var varX = a / 500.0 + varY;
			var varZ = varY - b / 200.0;

			var varX3 = varX * varX * varX;
			var varY3 = varY * varY * varY;
			var varZ3 = varZ * varZ * varZ;

			if (varY3 > 0.008856)
				varY = varY3;
			else
				varY = (varY - (double)16 / 116) / 7.787;

			if (varX3 > 0.008856)
				varX = varX3;
			else
				varX = (varX - (double)16 / 116) / 7.787;

			if (varZ3 > 0.008856)
				varZ = varZ3;
			else
				varZ = (varZ - (double)16 / 116) / 7.787;

			var x = refX * varX;
			var y = refY * varY;
			var z = refZ * varZ;

			// Then, convert from XYZ to RGB.
			// Standards used Observer = 2, Illuminant = D65
			// ref_X = 95.047, ref_Y = 100.000, ref_Z = 108.883

			var varR = x * 0.032406 + y * (-0.015372) + z * (-0.004986);
			var varG = x * (-0.009689) + y * 0.018758 + z * 0.000415;
			var varB = x * 0.000557 + y * (-0.002040) + z * 0.010570;

			if (varR > 0.0031308)
				varR = 1.055 * (math.pow(varR, 1 / 2.4)) - 0.055;
			else
				varR = 12.92 * varR;

			if (varG > 0.0031308)
				varG = 1.055 * (math.pow(varG, 1 / 2.4)) - 0.055;
			else
				varG = 12.92 * varG;

			if (varB > 0.0031308)
				varB = 1.055 * (math.pow(varB, 1 / 2.4)) - 0.055;
			else
				varB = 12.92 * varB;

			var nRed = (int) (varR * 256.0);
			var nGreen = (int) (varG * 256.0);
			var nBlue = (int) (varB * 256.0);

			if (nRed < 0)
				nRed = 0;
			else if (nRed > 255)
				nRed = 255;
			if (nGreen < 0)
				nGreen = 0;
			else if (nGreen > 255)
				nGreen = 255;
			if (nBlue < 0)
				nBlue = 0;
			else if (nBlue > 255)
				nBlue = 255;

			_pixels[_layerData.GetIndexInvertedByY(layerPixelX, layerPixelY)] = new Color32((byte) nRed, (byte) nGreen, (byte) nBlue, alpha);
		}
	}
}