using System;
using System.Drawing;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding.DecodeColor32;
using com.utkaka.PsdSynchronization.Editor.Psd.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding {
	public static class ImageDecoder {
		public static JobHandle DecodeImage(ImageLayer layer, Layer psdLayer, JobHandle inputDependencies = default(JobHandle)) {
			var byteDepth = Util.BytesFromBitDepth(psdLayer.PsdFile.BitDepth);
			var jobCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount;
			var execCount = layer.Pixels.Length;
			var sliceCount = execCount / jobCount;
			if (byteDepth == 4) {
				return Decode32(psdLayer.PsdFile.ColorMode, layer.Pixels, psdLayer.Channels.ToIdArray(),
					psdLayer.AlphaChannel, psdLayer.Masks, psdLayer.Rect, execCount, sliceCount, inputDependencies);
			} else {
				return Decode(psdLayer.PsdFile.ColorMode, psdLayer.PsdFile.ColorModeData, byteDepth, layer.Pixels, psdLayer.Channels.ToIdArray(),
					psdLayer.AlphaChannel, psdLayer.Masks, psdLayer.Rect, execCount, sliceCount, inputDependencies);
			}
		}
		
		private static JobHandle Decode(PsdColorMode colorMode, byte[] colorModeData, int byteDepth,
			NativeArray<Color32> pixels, Channel[] channels,
			Channel alphaChannel, MaskInfo masks, Rectangle layerRect, 
			int execCount, int sliceCount, JobHandle inputDependencies = default(JobHandle)) {
			var alphaData = new DecoderAlphaData();
			if (alphaChannel != null && alphaChannel.ImageData.Length > 0) {
				alphaData.HasAlpha = true;
				alphaData.AlphaChannel = new NativeArray<byte>(alphaChannel.ImageData, Allocator.TempJob);	
			} else {
				alphaData.AlphaChannel = new NativeArray<byte>(0, Allocator.TempJob);
			}

			if (masks != null) {
				alphaData.LayerMask = masks.LayerMask != null
					? CreateDecoderMask(masks.LayerMask, layerRect)
					: new DecoderMaskData(new NativeArray<byte>(0, Allocator.TempJob));
				alphaData.UserMask = masks.UserMask != null
					? CreateDecoderMask(masks.UserMask, layerRect)
					: new DecoderMaskData(new NativeArray<byte>(0, Allocator.TempJob));
			} else {
				alphaData.LayerMask = new DecoderMaskData(new NativeArray<byte>(0, Allocator.TempJob));
				alphaData.UserMask = new DecoderMaskData(new NativeArray<byte>(0, Allocator.TempJob));
			}
			
			var byteShift = byteDepth == 2 ? 1 : 0;
			var byteData = new DecoderByteData(byteDepth, byteShift);

			var layerData = new ImageLayerData(layerRect.Width, layerRect.Height);
			
			var channelsData = new DecoderChannelsData {
				Channel0 = channels.Length <= 0
					? new NativeArray<byte>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[0].ImageData, Allocator.TempJob),
				Channel1 = channels.Length <= 1
					? new NativeArray<byte>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[1].ImageData, Allocator.TempJob),
				Channel2 = channels.Length <= 2
					? new NativeArray<byte>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[2].ImageData, Allocator.TempJob),
				Channel3 = channels.Length <= 3
					? new NativeArray<byte>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[3].ImageData, Allocator.TempJob),
				ColorModeData = new NativeArray<byte>(colorModeData, Allocator.TempJob)
			};

			switch (colorMode) {
				case PsdColorMode.Bitmap:
					return new JobDecodeBitmap(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.Grayscale:
				case PsdColorMode.Duotone:
					return new JobDecodeGrayscale(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.Indexed:
					return new JobDecodeIndexed(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.RGB:
					return new JobDecodeRgb(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.CMYK:
					return new JobDecodeCmyk(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.Lab:
					return new JobDecodeLab(channelsData, alphaData, layerData, byteData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.Multichannel:
					throw new Exception("Cannot decode multichannel.");
				default:
					throw new Exception("Unknown color mode.");
			}
		}
		
		private static JobHandle Decode32(PsdColorMode colorMode,
			NativeArray<Color32> pixels, Channel[] channels,
			Channel alphaChannel, MaskInfo masks, Rectangle layerRect, 
			int execCount, int sliceCount, JobHandle inputDependencies = default(JobHandle)) {
			var alphaData = new DecoderAlpha32Data();
			if (alphaChannel != null && alphaChannel.ImageData.Length > 0) {
				alphaData.HasAlpha = true;
				alphaData.AlphaChannel = new NativeArray<byte>(alphaChannel.ImageData, Allocator.TempJob).Reinterpret<float>(1);	
			} else {
				alphaData.AlphaChannel = new NativeArray<float>(0, Allocator.TempJob);
			}

			if (masks != null) {
				alphaData.LayerMask = masks.LayerMask != null
					? CreateDecoderMask32(masks.LayerMask, layerRect)
					: new DecoderMask32Data(new NativeArray<float>(0, Allocator.TempJob));
				alphaData.UserMask = masks.UserMask != null
					? CreateDecoderMask32(masks.UserMask, layerRect)
					: new DecoderMask32Data(new NativeArray<float>(0, Allocator.TempJob));
			} else {
				alphaData.LayerMask = new DecoderMask32Data(new NativeArray<float>(0, Allocator.TempJob));
				alphaData.UserMask = new DecoderMask32Data(new NativeArray<float>(0, Allocator.TempJob));
			}
			
			var layerData = new ImageLayerData(layerRect.Width, layerRect.Height);

			var channelsData = new DecoderChannels32Data {
				Channel0 = channels.Length <= 0
					? new NativeArray<float>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[0].ImageData, Allocator.TempJob).Reinterpret<float>(1),
				Channel1 = channels.Length <= 1
					? new NativeArray<float>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[1].ImageData, Allocator.TempJob).Reinterpret<float>(1),
				Channel2 = channels.Length <= 2
					? new NativeArray<float>(0, Allocator.TempJob)
					: new NativeArray<byte>(channels[2].ImageData, Allocator.TempJob).Reinterpret<float>(1)
			};

			switch (colorMode) {
				case PsdColorMode.Grayscale:
					return new JobDecodeGrayscale32(channelsData, alphaData, layerData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				case PsdColorMode.RGB:
					return new JobDecodeRgb32(channelsData, alphaData, layerData, pixels).Schedule(execCount,
						sliceCount, inputDependencies);
				default:
					throw new PsdInvalidException("32-bit HDR images must be either RGB or grayscale.");
			}
		}

		private static DecoderMaskData CreateDecoderMask(Mask mask, Rectangle layerRect) {
			var maskRect = mask.Rect;
			var inLayerX = mask.PositionVsLayer ? maskRect.X : maskRect.X - layerRect.X;
			var inLayerY = mask.PositionVsLayer ? maskRect.Y : maskRect.Y - layerRect.Y;
			return new DecoderMaskData(inLayerX, inLayerY, maskRect.Width, maskRect.Height, layerRect.Width,
				new NativeArray<byte>(mask.ImageData, Allocator.TempJob));
		}
		
		private static DecoderMask32Data CreateDecoderMask32(Mask mask, Rectangle layerRect) {
			var maskRect = mask.Rect;
			var inLayerX = mask.PositionVsLayer ? maskRect.X : maskRect.X - layerRect.X;
			var inLayerY = mask.PositionVsLayer ? maskRect.Y : maskRect.Y - layerRect.Y;
			var maskData = new NativeArray<byte>(mask.ImageData, Allocator.TempJob).Reinterpret<float>(1);
			return new DecoderMask32Data(inLayerX, inLayerY, maskRect.Width, maskRect.Height, layerRect.Width,maskData);
		}
		
		public static byte RGBByteFromHDRFloat(float ptr) {
			var result = (byte) (255 * math.pow(ptr, 1.0f / 2.19921875f));
			return result;
		}
	}
}