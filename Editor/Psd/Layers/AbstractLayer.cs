using System;
using com.utkaka.Psd;
using com.utkaka.PsdPlugin.PsdFiles;
using UnityEngine;

namespace com.utkaka.PsdPlugin.Layers {
	public abstract class AbstractLayer : IDisposable {
		private float _opacity;
		public GroupLayer ParentLayer { get; set; }
		public string Name { get; set; }

		public float Opacity {
			get {
				if (ParentLayer == null) return _opacity;
				return _opacity * ParentLayer.Opacity;
			}
		}

		public bool Visible { get; set; }
		public string BlendModeKey { get; set; }
		public Rect Rectangle { get; }
		
		public AbstractLayer(Layer psdFileLayer, GroupLayer parentLayer) {
			var psdWidth = psdFileLayer.PsdFile.ColumnCount;
			var psdHeight = psdFileLayer.PsdFile.RowCount;
			Rectangle = psdFileLayer.Rect.ToRect();
			Rectangle = Rectangle.ConvertToUnitySpace(psdWidth, psdHeight);
			BlendModeKey = psdFileLayer.BlendModeKey;
			Name = psdFileLayer.Name;
			_opacity = psdFileLayer.Opacity / 255.0f;
			Visible = psdFileLayer.Visible;
			ParentLayer = parentLayer;
		}

		public virtual void WriteLayer(PsdFile psdFile) {
			psdFile.Layers.Add(ToPsdLayer(psdFile));
		}

		protected virtual Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = new Layer(psdFile);
			// Set layer metadata
			psdLayer.Name = Name;
			var psdWidth = psdFile.ColumnCount;
			var psdHeight = psdFile.RowCount;
			psdLayer.Rect = Rectangle.ConvertToPsdSpace(psdWidth, psdHeight).ToRectangle();
			psdLayer.BlendModeKey = BlendModeKey;
			psdLayer.Opacity = (byte)(_opacity * 255);
			psdLayer.Visible = Visible;
			psdLayer.Masks = new MaskInfo();
			psdLayer.BlendingRangesData = new BlendingRanges(psdLayer);
			return psdLayer;
		}

		public abstract void Dispose();
	}
}