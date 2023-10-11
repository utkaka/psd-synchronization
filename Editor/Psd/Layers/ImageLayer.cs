using com.utkaka.Psd.ImageProcessing.Decoding;
using com.utkaka.Psd.ImageProcessing.Encoding;
using com.utkaka.Psd.PsdFiles;
using com.utkaka.Psd.PsdFiles.Layers;
using Unity.Collections;
using UnityEngine;

namespace com.utkaka.Psd.Layers {
	public class ImageLayer : AbstractLayer {
		private NativeArray<Color32> _pixels;
		public NativeArray<Color32> Pixels => _pixels;

		public ImageLayer(Layer psdFileLayer, GroupLayer parentLayer) : base(psdFileLayer, parentLayer){
			psdFileLayer.CreateMissingChannels();
			_pixels = new NativeArray<Color32>(psdFileLayer.Rect.Width * psdFileLayer.Rect.Height, Allocator.Persistent);
			ImageDecoder.DecodeImage(this, psdFileLayer).Complete();
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
			ImageEncoder.EncodeImage(channelsArray, psdLayer.AlphaChannel, Pixels, psdLayer.Rect);
			return psdLayer;
		}

		public override void Dispose() {
			if (_pixels.IsCreated) {
				_pixels.Dispose();
			}
		}
	}
}