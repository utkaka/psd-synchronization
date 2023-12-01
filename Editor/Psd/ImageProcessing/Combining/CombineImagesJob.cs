using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Combining {
	public struct CombineImagesJob : IJobParallelFor {
		private ImageLayerData _layerData;
		[ReadOnly, NativeDisableParallelForRestriction, DeallocateOnJobCompletion]
		private NativeArray<CombinedImageData> _combinedImages;
		[ReadOnly, NativeDisableParallelForRestriction, DeallocateOnJobCompletion]
		private NativeArray<Color32> _combinedPixels;
		private NativeArray<Color32> _pixels;

		public CombineImagesJob(ImageLayerData layerData, NativeArray<CombinedImageData> combinedImages, NativeArray<Color32> combinedPixels, NativeArray<Color32> pixels) {
			_layerData = layerData;
			_combinedImages = combinedImages;
			_combinedPixels = combinedPixels;
			_pixels = pixels;
		}

		public void Execute(int index) {
			var layerPixelX = _layerData.GetInLayerX(index);
			var layerPixelY = _layerData.GetInLayerY(index);
			for (var i = _combinedImages.Length - 1; i >= 0; i--) {
				var combinedImage = _combinedImages[i];
				var localPixelIndex = combinedImage.GetPixelIndex(layerPixelX, layerPixelY);
				if (localPixelIndex < 0) continue;
				var color = _combinedPixels[localPixelIndex];
				var oldColor = _pixels[index];
				if (oldColor.a == 0 && oldColor.r == 0 && oldColor.g == 0 && oldColor.b == 0) {
					_pixels[index] = color;
					return;
				}
				var alpha = (color.a / 255.0f) * (combinedImage.Opacity / 255.0f);
				_pixels[index] = new Color32((byte) (alpha * color.r + (1.0f - alpha) * oldColor.r),
					(byte)(alpha * color.g + (1.0f - alpha) * oldColor.g),
					(byte)(alpha * color.b + (1.0f - alpha) * oldColor.b),
					(byte)(255 * (1.0f - (1.0f - alpha) * (1.0f - color.b / 255.0f))));
			}
		}
	}
}