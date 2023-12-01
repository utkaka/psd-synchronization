using System;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.Extensions;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	public class LinkedObject : AbstractPsdObject {
		[SerializeField]
		private ImageObject _imageObject;
		
		private string _linkedId;
		private Quaternion _rotation;

		public LinkedObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject) {
			_imageObject = new ImageObject(psdFileLayer, parentObject);
			if (psdFileLayer.AdditionalInfo.FirstOrDefault(i => i is PlacedLayerInfo) is not PlacedLayerInfo placedLayerInfo) {
				throw new ArgumentException("There is no PlacedLayerInfo for this LinkedObject");
			}
			_linkedId = placedLayerInfo.Id;
			var matrix = placedLayerInfo.Matrix;
			var scale = matrix.GetScale();
			var position = matrix.GetPosition();
			var psdWidth = psdFileLayer.PsdFile.ColumnCount;
			var psdHeight = psdFileLayer.PsdFile.RowCount;
			Rect = new Rect(position.x - psdWidth / 2.0f - scale.x, psdHeight / 2.0f - position.y - scale.y,
				scale.x * 2.0f, scale.y * 2.0f);
			_rotation = matrix.GetRotation();
		}
		
		protected override Transform InternalCreateAsset(Transform parentObject, AssetContext assetContext) {
			return assetContext.CreateLinkedObject(_linkedId, Name, Rect, _rotation, parentObject);
		}
	}
}