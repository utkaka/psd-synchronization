using System.Collections.Generic;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Combining;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	public class PsdLinkedObject {
		private string _id;
		private string _name;
		private int _height;
		private int _width;
		private ImageObject _baseImage;
		
		public PsdLinkedObject(string id, string name, PsdFile psdFile) {
			_id = id;
			_name = AbstractPsdObject.FixName(name);
			_width = psdFile.ColumnCount;
			_height = psdFile.RowCount;
			// Multichannel images are loaded by processing each channel as a
			// grayscale layer.
			if (psdFile.ColorMode == PsdColorMode.Multichannel) {
				psdFile.CreateLayersFromChannels();
				psdFile.ColorMode = PsdColorMode.Grayscale;
			}
			
			var jobHandle = default(JobHandle);
			int sliceCount;
			
			var pixels = new NativeArray<Color32>(_width * _height, Allocator.Persistent);

			psdFile.VerifyLayerSections();
			var layers = new List<Layer>();
			var layersPixels = new List<NativeArray<Color32>>();
			var totalPixelsCount = 0;
			for (var i = psdFile.Layers.Count - 1; i >= 0; i--) {
				var psdFileLayer = psdFile.Layers[i];
				var sectionInfo = (LayerSectionInfo)psdFileLayer.AdditionalInfo
					.SingleOrDefault(x => x is LayerSectionInfo);
				AbstractPsdObject psdObject = null;
				if (!psdFileLayer.Visible) continue;
				if (sectionInfo == null || sectionInfo.SectionType == LayerSectionType.Layer) {
					totalPixelsCount += psdFileLayer.Rect.Width * psdFileLayer.Rect.Height;
					var layerPixels = new NativeArray<Color32>(psdFileLayer.Rect.Width * psdFileLayer.Rect.Height, Allocator.TempJob);
					layersPixels.Add(layerPixels);
					layers.Add(psdFileLayer);
					jobHandle = JobHandle.CombineDependencies(jobHandle, ImageDecoder.DecodeImage(psdFileLayer, layerPixels));
				}
			}
			jobHandle.Complete();
				
			var combinedPixels = new NativeArray<Color32>(totalPixelsCount, Allocator.TempJob);
			var combinedImages = new NativeArray<CombinedImageData>(layers.Count, Allocator.TempJob);
			var offset = 0;
			for (var i = 0; i < layers.Count; i++) {
				var layer = layers[i];
				var pixelCount = layersPixels[i].Length; 
				combinedImages[i] = new CombinedImageData(offset, layer.Rect.X, _height - layer.Rect.Y - layer.Rect.Height, layer.Rect.Width,
					layer.Rect.Height, layer.Opacity);
				NativeArray<Color32>.Copy(layersPixels[i], 0, combinedPixels, offset, pixelCount);
				offset += pixelCount;
				layersPixels[i].Dispose();
			}

			var combineJob = new CombineImagesJob(new ImageLayerData(_width, _height), combinedImages, combinedPixels,
				pixels);
			var jobCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount;
			var execCount = pixels.Length;
			sliceCount = execCount / jobCount;
			combineJob.Schedule(pixels.Length, sliceCount).Complete();
			_baseImage = new ImageObject(pixels, psdFile.BaseLayer, null, _name);
		}
		
		public void CreateLinkedAsset(AssetContext assetContext, string path) {
			assetContext.CreateLinkedRootObject(_id, _name, path, new Vector2(_width, _height), _baseImage);
		}
	}
}