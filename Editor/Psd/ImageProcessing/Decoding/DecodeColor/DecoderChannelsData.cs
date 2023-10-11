using Unity.Collections;

namespace com.utkaka.Psd.ImageProcessing.Decoding.DecodeColor {
	public struct DecoderChannelsData {
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> Channel0;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> Channel1;
		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> Channel2;
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> Channel3;
		[NativeDisableParallelForRestriction]
		[DeallocateOnJobCompletion]
		public NativeArray<byte> ColorModeData;
	}
}