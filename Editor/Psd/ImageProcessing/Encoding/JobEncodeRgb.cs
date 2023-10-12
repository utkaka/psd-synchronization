using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Encoding {
	[BurstCompile]	
	public struct JobEncodeRgb : IJobParallelFor {
		public ImageLayerData LayerData;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<Color32> Pixels;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<byte> Channel0;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<byte> Channel1;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<byte> Channel2;
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<byte> AlphaChannel;
		
		public void Execute(int index) {
			var destIndex = LayerData.GetIndexInvertedByY(LayerData.GetInLayerX(index), LayerData.GetInLayerY(index));
			Channel0[destIndex] = Pixels[index].r;
			Channel1[destIndex] = Pixels[index].g;
			Channel2[destIndex] = Pixels[index].b;
			AlphaChannel[destIndex] = Pixels[index].a;
		}
	}
}