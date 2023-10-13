using System;
using System.Collections.Generic;
using System.Linq;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Descriptors.Elements;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.EngineData;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	[Serializable]
	public class TextObject : AbstractPsdObject {
		[SerializeField]
		private ImageObject _imageObject;
		[SerializeField]
		private string _text;
		[SerializeField]
		private Matrix4x4 _matrix;
		[SerializeField]
		private Rect _textRect;
		//TODO: Parse And Serialize EngineData
		private EngineDataObject _engineData;
		private readonly TypeToolMatrix _matrixValues;
		private readonly Descriptor _textDescriptor;
		private readonly Descriptor _warpDescriptor;

		public TextObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject) {
			_imageObject = new ImageObject(psdFileLayer, parentObject);
			var typeToolInfo = psdFileLayer.AdditionalInfo.FirstOrDefault(i => i is TypeToolInfo) as TypeToolInfo;
			if (typeToolInfo == null) {
				throw new ArgumentException("There is no TypeToolInfo for this TextObject");
			}
			_engineData = typeToolInfo.EngineData;
			_textDescriptor = typeToolInfo.TextDescriptor;
			_warpDescriptor = typeToolInfo.WarpDescriptor;
			_matrixValues = typeToolInfo.TransformMatrix;
			_text = typeToolInfo.EngineData["EngineDict"]["Editor"]["Text"].GetStringValue();
			var bounds = ((DescriptorElement)_textDescriptor.Items["bounds"]).Descriptor.Items;
			
			_matrix = new Matrix4x4();
			_matrix.SetRow(0, new Vector4((float)_matrixValues.XX, (float)_matrixValues.XY, 0.0f, (float)_matrixValues.TX));
			_matrix.SetRow(1, new Vector4((float)_matrixValues.YX, (float)_matrixValues.YY, 0.0f, (float)_matrixValues.TY));
			_matrix.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
			_matrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
			
			_textRect = RectUtils.RectFromBounds(((UnitFloatElement) bounds["Left"]).Value * _matrix.lossyScale.x,
				((UnitFloatElement) bounds["Top "]).Value * _matrix.lossyScale.y, ((UnitFloatElement) bounds["Rght"]).Value * _matrix.lossyScale.x,
				((UnitFloatElement) bounds["Btom"]).Value * _matrix.lossyScale.y);

			_textRect.position += new Vector2(_matrix.m03, _matrix.m13);

			_textRect = _textRect.ConvertToUnitySpace(psdFileLayer.PsdFile.ColumnCount, psdFileLayer.PsdFile.RowCount);
		}

		public override void SaveAssets(string psdName, SaveAssetsContext saveAssetsContext) {
			base.SaveAssets(psdName, saveAssetsContext);
			_imageObject.SaveAssets(psdName, saveAssetsContext);
		}

		protected override Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = base.ToPsdLayer(psdFile);
			_engineData["ResourceDict"]["KinsokuSet"] = new EngineDataObject(new List<EngineDataObject>());
			_engineData["DocumentResources"]["KinsokuSet"] = new EngineDataObject(new List<EngineDataObject>());
			var typeToolInfo = new TypeToolInfo(_matrixValues, _textDescriptor, _warpDescriptor, _engineData);
			psdLayer.AdditionalInfo.Add(typeToolInfo);
			return psdLayer;
		}
	}
}