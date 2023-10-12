using System;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Encoding;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	[Serializable]
	public class ImageObject : AbstractPsdObject {

		private JobHandle _jobHandle;
		private NativeArray<Color32> _pixels;
		
		public ImageObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject){
			psdFileLayer.CreateMissingChannels();
			_pixels = new NativeArray<Color32>(psdFileLayer.Rect.Width * psdFileLayer.Rect.Height, Allocator.TempJob);
			_jobHandle = ImageDecoder.DecodeImage(psdFileLayer, _pixels);
		}

		public override void SaveAssets(string path) {
			_jobHandle.Complete();
			//TODO: Save sprite
			_pixels.Dispose();
		}
		
		protected override Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = base.ToPsdLayer(psdFile);
			// Store channel metadata
			int layerSize = _pixels.Length;
			for (int i = -1; i < 3; i++) {
				var ch = new Channel((short) i, psdLayer);
				ch.ImageCompression = psdFile.ImageCompression;
				ch.ImageData = new byte[layerSize];
				psdLayer.Channels.Add(ch);
			}

			// Store and compress channel image data
			var channelsArray = psdLayer.Channels.ToIdArray();
			//TODO: Fix encoding
			//ImageEncoder.EncodeImage(channelsArray, psdLayer.AlphaChannel, Pixels, psdLayer.Rect);
			return psdLayer;
		}
	}
}