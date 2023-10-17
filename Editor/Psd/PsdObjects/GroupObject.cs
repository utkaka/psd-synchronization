using System;
using System.Collections.Generic;
using System.Drawing;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	[Serializable]
	public class GroupObject : AbstractPsdObject {
		[SerializeReference]
		private List<AbstractPsdObject> _children;
		
		public GroupObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject) {
			_children = new List<AbstractPsdObject>();
		}

		public void AddChild(AbstractPsdObject child) {
			_children.Add(child);
		}
		
		protected override Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = base.ToPsdLayer(psdFile);
			var sectionInfo = new LayerSectionInfo(LayerSectionSubtype.Normal, LayerSectionType.ClosedFolder);
			psdLayer.AdditionalInfo.Add(sectionInfo);
			return psdLayer;
		}
		
		public override void SaveAssets(string psdName, GameObject parentObject, SaveAssetsContext saveAssetsContext) {
			var gameObject = CreateGameObject(saveAssetsContext);
			gameObject.transform.SetParent(parentObject.transform);
			for (var i = _children.Count - 1; i >= 0; i--) {
				var child = _children[i];
				child.SaveAssets(psdName, gameObject, saveAssetsContext);
			}
		}

		protected override GameObject CreateGameObject(SaveAssetsContext saveAssetsContext) {
			GameObject gameObject;
			switch (saveAssetsContext.ImportPrefabType) {
				case PsdPrefabType.World:
					gameObject = new GameObject(Name, typeof(SortingLayer));
					gameObject.transform.localPosition = new Vector2(Rect.center.x / 100.0f, Rect.center.y/ 100.0f);
					break;
				case PsdPrefabType.UGUIWithoutCanvas:
				case PsdPrefabType.UGUIWithCanvas:
					gameObject = new GameObject(Name, typeof(RectTransform));
					((RectTransform)gameObject.transform).sizeDelta = new Vector2(Rect.width, Rect.height);
					gameObject.transform.localPosition = new Vector2(Rect.center.x, Rect.center.y);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return gameObject;
		}

		public override void Write(PsdFile psdFile) {
			base.Write(psdFile);
			foreach (var layer in _children) {
				layer.Write(psdFile);
			}
			var endSectionLayer = new Layer(psdFile) {
				Name = "</Layer group>",
				Rect = new Rectangle(),
				BlendModeKey = "norm",
				Opacity = 255,
				Visible = true,
				Masks = new MaskInfo()
			};
			endSectionLayer.BlendingRangesData = new BlendingRanges(endSectionLayer);
			var sectionInfo = new LayerSectionInfo(LayerSectionSubtype.Normal, LayerSectionType.SectionDivider);
			endSectionLayer.AdditionalInfo.Add(sectionInfo);
			psdFile.Layers.Add(endSectionLayer);
		}
	}
}