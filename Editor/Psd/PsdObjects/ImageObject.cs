using System;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding;
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

		public ImageObject(Layer psdFileLayer, GroupObject parentObject, string name) :
			this(psdFileLayer, parentObject) {
			Name = name;
		}
		
		public ImageObject(NativeArray<Color32> pixels, Layer psdFileLayer, GroupObject parentObject, string name) : base(psdFileLayer, parentObject) {
			psdFileLayer.CreateMissingChannels();
			if (pixels.Length == 0 || psdFileLayer.Rect.Width <= 0 || psdFileLayer.Rect.Height <= 0) return;
			Name = name;
			_pixels = pixels;
		}

		public ImageObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject) {
			psdFileLayer.CreateMissingChannels();
			if (psdFileLayer.Rect.Width <= 0 || psdFileLayer.Rect.Height <= 0) return;
			_pixels = new NativeArray<Color32>(psdFileLayer.Rect.Width * psdFileLayer.Rect.Height, Allocator.Persistent);
			_jobHandle = ImageDecoder.DecodeImage(psdFileLayer, _pixels);
		}

		protected override Transform InternalCreateAsset(Transform parentObject, AssetContext assetContext) {
			if (!_pixels.IsCreated) return null;
			_jobHandle.Complete();
			var transform = assetContext.CreateImageObject(Name, Rect, parentObject, _pixels, Opacity);
			_pixels.Dispose();
			return transform;
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