using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.AlphaTrimming {
	[BurstCompile]
	public struct DetectOpaqueRectJob : IJobFor {
		[ReadOnly]
		private NativeArray<Color32> _pixels;
		private NativeArray<int4> _opaqueRect;
		private readonly int _width;
		private readonly int _pixelsCountMinusOne;

		public DetectOpaqueRectJob(NativeArray<Color32> pixels, NativeArray<int4> opaqueRect, int width) {
			_pixels = pixels;
			_opaqueRect = opaqueRect;
			_width = width;
			_pixelsCountMinusOne = _pixels.Length - 1;
		}

		public void Execute(int index) {
			_opaqueRect[0] = CheckIndex(_pixels, index, _opaqueRect[0], _width, _pixelsCountMinusOne);
		}

		private static int4 CheckIndex(NativeArray<Color32> pixels, int index, int4 rect, int width, int pixelsCountMinusOne) {
			if (pixels[index].a > 0) {
				var x = index % width;
				var y = index / width;
				if (rect.x == -1 || x < rect.x) {
					rect.x = x;
				}
				if (rect.y == -1 || y < rect.y) {
					rect.y = y;
				}	
			}
			var reversedIndex = pixelsCountMinusOne - index;
			if (pixels[reversedIndex].a == 0) return rect;
			var w = reversedIndex % width;
			var h = reversedIndex / width;
			if (rect.z == -1 || w > rect.z) {
				rect.z = w;
			}

			if (rect.w == -1 || h > rect.w) {
				rect.w = h;
			}
			return rect;
		}
	}
}