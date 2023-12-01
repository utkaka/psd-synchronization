using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.AlphaTrimming {
	[BurstCompile]
	public struct TrimAlphaJob : IJobParallelFor{
		[ReadOnly, NativeDisableParallelForRestriction]
		private NativeArray<Color32> _originalPixels;
		[WriteOnly]
		private NativeArray<Color32> _trimmedPixels;

		private readonly int _opaqueAreaStartIndex;
		private readonly int _opaqueAreaWidth;
		private readonly int _widthDiff;

		public TrimAlphaJob(NativeArray<Color32> originalPixels, NativeArray<Color32> trimmedPixels, int4 opaqueRect, int originalWidth) {
			_originalPixels = originalPixels;
			_trimmedPixels = trimmedPixels;
			_opaqueAreaStartIndex = originalWidth * opaqueRect.y + opaqueRect.x;
			_opaqueAreaWidth = opaqueRect.z;
			_widthDiff = originalWidth - _opaqueAreaWidth;
		}

		public void Execute(int index) {
			var y = index / _opaqueAreaWidth;
			var originalIndex = index + _opaqueAreaStartIndex + y * _widthDiff;
			_trimmedPixels[index] = _originalPixels[originalIndex];
		}
	}
}