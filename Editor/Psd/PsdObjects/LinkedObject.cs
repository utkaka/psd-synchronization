using System;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	public class LinkedObject : AbstractPsdObject {
		[SerializeField]
		private ImageObject _imageObject;
		
		private string _linkedId;

		public LinkedObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject) {
			_imageObject = new ImageObject(psdFileLayer, parentObject);
			if (psdFileLayer.AdditionalInfo.FirstOrDefault(i => i is PlacedLayerInfo) is not PlacedLayerInfo placedLayerInfo) {
				throw new ArgumentException("There is no PlacedLayerInfo for this LinkedObject");
			}
			_linkedId = placedLayerInfo.Id;
		}
		
		protected override Transform InternalCreateAsset(Transform parentObject, AssetContext assetContext) {
			return assetContext.CreateLinkedObject(_linkedId, Name, Rect, parentObject, _imageObject);
		}
	}
}