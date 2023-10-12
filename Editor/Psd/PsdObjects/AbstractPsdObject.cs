using System;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	[Serializable]
	public abstract class AbstractPsdObject {
		[SerializeField]
		private int _id;
		[SerializeField]
		private string _name;
		[SerializeReference]
		private GroupObject _parentObject;
		[SerializeField]
		private bool _visible;
		[SerializeField]
		private float _opacity;
		[SerializeField]
		private string _blendModeKey;
		[SerializeField]
		private Rect _rect;

		public GroupObject ParentObject => _parentObject;
		public float Opacity => _opacity * _parentObject?.Opacity ?? _opacity;

		public AbstractPsdObject(Layer psdFileLayer, GroupObject parentObject) {
			_id = (psdFileLayer.AdditionalInfo.FirstOrDefault(i => i is LayerIdInfo) as LayerIdInfo)?.Id ?? 0;
			_name = psdFileLayer.Name;
			_parentObject = parentObject;
			_visible = psdFileLayer.Visible;
			_opacity = psdFileLayer.Opacity / 255.0f;
			_blendModeKey = psdFileLayer.BlendModeKey;
			
			var psdWidth = psdFileLayer.PsdFile.ColumnCount;
			var psdHeight = psdFileLayer.PsdFile.RowCount;
			_rect = psdFileLayer.Rect.ToRect().ConvertToUnitySpace(psdWidth, psdHeight);
		}

		public virtual void SaveAssets(string path) { }

		public virtual void Write(PsdFile psdFile) {
			psdFile.Layers.Add(ToPsdLayer(psdFile));
		}

		protected virtual Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = new Layer(psdFile);
			//TODO: Add LayerIdInfo info
			psdLayer.Name = _name;
			var psdWidth = psdFile.ColumnCount;
			var psdHeight = psdFile.RowCount;
			psdLayer.Rect = _rect.ConvertToPsdSpace(psdWidth, psdHeight).ToRectangle();
			psdLayer.BlendModeKey = _blendModeKey;
			psdLayer.Opacity = (byte)(_opacity * 255);
			psdLayer.Visible = _visible;
			psdLayer.Masks = new MaskInfo();
			psdLayer.BlendingRangesData = new BlendingRanges(psdLayer);
			return psdLayer;
		}
	}
}