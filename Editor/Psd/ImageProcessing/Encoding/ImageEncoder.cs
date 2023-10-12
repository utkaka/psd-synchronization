using System.Drawing;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Encoding {
	public static class ImageEncoder {
		public static void EncodeImage(Channel[] destChannels, Channel destAlphaChannel, NativeArray<Color32> sourcePixels,
			Rectangle rect) {
			var jobCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount;
			var execCount = sourcePixels.Length;
			var sliceCount = execCount / jobCount;

			var channel0 = new NativeArray<byte>(destChannels[0].ImageData, Allocator.TempJob);
			var channel1 = new NativeArray<byte>(destChannels[1].ImageData, Allocator.TempJob);
			var channel2 = new NativeArray<byte>(destChannels[2].ImageData, Allocator.TempJob);
			var alphaChannel = new NativeArray<byte>(destAlphaChannel.ImageData, Allocator.TempJob);
			
			var job = new JobEncodeRgb {
				LayerData = new ImageLayerData(rect.Width, rect.Height),
				Pixels = sourcePixels,
				Channel0 = channel0,
				Channel1 = channel1,
				Channel2 = channel2,
				AlphaChannel = alphaChannel
			};
			job.Schedule(execCount, sliceCount).Complete();
			
			NativeArray<byte>.Copy(channel0, destChannels[0].ImageData);
			NativeArray<byte>.Copy(channel1, destChannels[1].ImageData);
			NativeArray<byte>.Copy(channel2, destChannels[2].ImageData);
			NativeArray<byte>.Copy(alphaChannel, destAlphaChannel.ImageData);

			channel0.Dispose();
			channel1.Dispose();
			channel2.Dispose();
			alphaChannel.Dispose();

			foreach (var channel in destChannels) {
				channel.CompressImageData();
			}
			destAlphaChannel.CompressImageData();
		}
	}
}