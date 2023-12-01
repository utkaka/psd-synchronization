using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.ImageResources;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects;
using UnityEditor;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd {
	[Serializable]
	public class PsdRootObject {
		[SerializeField]
		private string _name;
		[SerializeField]
		private ImageObject _baseImage;
		[SerializeField]
		private int _height;
		[SerializeField]
		private int _width;
		[SerializeField]
		private List<PsdLinkedObject> _linkedRootObjects;
		[SerializeField]
		private List<AbstractPsdObject> _psdObjects;
		[SerializeField]
		public ResolutionInfo _resolution;
		[SerializeField]
		public ImageCompression _imageCompression;
		
		public PsdRootObject(string id, string name, Stream input, Context context) : this(id, name, new PsdFile(input, context)) { }

		public PsdRootObject(string id, string name, PsdFile psdFile) {
			_name = name;
			// Multichannel images are loaded by processing each channel as a
			// grayscale layer.
			if (psdFile.ColorMode == PsdColorMode.Multichannel) {
				psdFile.CreateLayersFromChannels();
				psdFile.ColorMode = PsdColorMode.Grayscale;
			}

			_width = psdFile.ColumnCount;
			_height = psdFile.RowCount;
			_imageCompression = psdFile.ImageCompression;
			_resolution = psdFile.Resolution;
			_baseImage = new ImageObject(psdFile.BaseLayer, null, _name);
			_linkedRootObjects = new List<PsdLinkedObject>();
			_psdObjects = new List<AbstractPsdObject>();

			psdFile.VerifyLayerSections();

			foreach (var additionalInfo in psdFile.AdditionalInfo) {
				if (additionalInfo is not LinkedFilesInfo linkedFilesInfo) continue;
				foreach (var linkedFile in linkedFilesInfo.LinkedFiles) {
					if (linkedFile.File == null) continue;
					_linkedRootObjects.Add(new PsdLinkedObject(linkedFile.ID,
						Path.GetFileNameWithoutExtension(linkedFile.Name.Remove(linkedFile.Name.Length - 1)),
						linkedFile.File));
				}
			}

			GroupObject parentObject = null;
			for (var i = psdFile.Layers.Count - 1; i >= 0; i--) {
				var psdFileLayer = psdFile.Layers[i];
				var sectionInfo = (LayerSectionInfo)psdFileLayer.AdditionalInfo
					.SingleOrDefault(x => x is LayerSectionInfo);
				AbstractPsdObject psdObject = null;
				if (sectionInfo == null || sectionInfo.SectionType == LayerSectionType.Layer) {
					if (psdFileLayer.AdditionalInfo.FirstOrDefault(l => l is TypeToolInfo) != null) {
						psdObject = new TextObject(psdFileLayer, parentObject);
					} else if (psdFileLayer.AdditionalInfo.FirstOrDefault(l => l is PlacedLayerInfo) != null) {
						psdObject = new LinkedObject(psdFileLayer, parentObject);
					} else {
						psdObject = new ImageObject(psdFileLayer, parentObject);
					}
				} else if (sectionInfo.SectionType is LayerSectionType.ClosedFolder or LayerSectionType.OpenFolder) {
					psdObject = new GroupObject(psdFileLayer, parentObject);
				} else {
					parentObject = parentObject?.ParentObject;
				}
				if (psdObject != null) {
					if (parentObject != null) parentObject.AddChild(psdObject);
					else _psdObjects.Add(psdObject);	
				}
				if (psdObject is GroupObject groupObject) parentObject = groupObject;
			}
		}

		public void CreateMainAsset(AssetContext assetContext) {
			_baseImage.CreateAsset(null, assetContext);
			var root = assetContext.CreateRootObject(_name);
			CreateAssets(assetContext, root, "");
			PrefabUtility.ApplyPrefabInstance(root.gameObject, InteractionMode.AutomatedAction);
		}

		private void CreateAssets(AssetContext assetContext, Transform parent, string path) {
			for (var i = 0; i < _linkedRootObjects.Count; i++) {
				var linkedRootObject = _linkedRootObjects[i];
				linkedRootObject.CreateLinkedAsset(assetContext, path);
			}
			for (var i = _psdObjects.Count - 1; i >= 0; i--) {
				var psdObject = _psdObjects[i];
				psdObject.CreateAsset(parent, assetContext);
			}
		}

		public void Save(Stream output, Context context) {
			/*var psdVersion = ((input.Height > 30000) || (input.Width > 30000))
				? PsdFileVersion.PsbLargeDocument
				: PsdFileVersion.Psd;
			var psdFile = new PsdFile(psdVersion);

			psdFile.RowCount = input.Height;
			psdFile.ColumnCount = input.Width;
			psdFile.ChannelCount = 4;
			psdFile.ColorMode = PsdColorMode.RGB;
			psdFile.BitDepth = 8;
			psdFile.Resolution = input.Resolution;
			psdFile.ImageCompression = input.ImageCompression;
			
			for (short i = 0; i < psdFile.ChannelCount; i++) {
				var channel = new Channel(i, psdFile.BaseLayer) {
					ImageData = new byte[input.BaseLayer.Pixels.Length],
					ImageCompression = psdFile.ImageCompression
				};
				psdFile.BaseLayer.Channels.Add(channel);
			}
			var channelsArray = psdFile.BaseLayer.Channels.ToIdArray();
			ImageEncoder.EncodeImage(channelsArray, channelsArray[3],
				input.BaseLayer.Pixels, psdFile.BaseLayer.Rect);

			foreach (var pdnLayer in input.Layers) {
				pdnLayer.WriteLayer(psdFile);
			}
			psdFile.Layers.Reverse();
			psdFile.Save(output, context);*/
		}
	}
}