using Unity.Collections;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor32 {
	public struct DecoderChannels32Data {
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<float> Channel0;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<float> Channel1;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<float> Channel2;
	}
}