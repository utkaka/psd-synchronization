using System.Collections.Generic;
using System.IO;
using System.Text;
using com.utkaka.PsdSynchronization.Editor.Psd.Layers;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor {
	public class ImportPsdWizard : ScriptableWizard {
		private static string _lastPsdPath;
		[SerializeField]
		private PsdPrefabType _importPrefabType;
		private string _psdPath;

		[MenuItem("Assets/Import PSD file")]
		private static void CreateWizard() {
			var psdPath = EditorUtility.OpenFilePanel("Choose a PSD file", _lastPsdPath, "psd,psb");
			if (string.IsNullOrEmpty(psdPath)) return;
			_lastPsdPath = Directory.GetParent(psdPath)?.FullName;
			var wizard = DisplayWizard<ImportPsdWizard>("Import PSD", "Import", "Cancel");
			wizard._psdPath = psdPath;
		}
		
		private void OnWizardCreate() {
			var loggerType = PsdSynchronizationSettingsProvider.GetLoggerType();
			var stream = File.OpenRead(_psdPath);
			var psdFile = new PsdFile(stream, loggerType == PsdSynchronizationSettingsProvider.LoggerType.Console ? new Context(Debug.unityLogger) : new Context());
			stream.Close();
			
			stream = File.OpenWrite(Path.Combine(Directory.GetParent(_psdPath)?.FullName, "Copy.psd"));
			psdFile.Save(stream, loggerType == PsdSynchronizationSettingsProvider.LoggerType.Console ? new Context(Debug.unityLogger) : new Context());
			stream.Close();
			

			/*var stream = File.OpenRead(_psdPath);

			var timer = new Stopwatch();
			timer.Start();
			var document = DocumentLoader.Load(stream);
			timer.Stop();
			var timeTaken = timer.ElapsedMilliseconds / 1000.0f;
			stream.Close();

			//ProcessLayers(new GameObject("PSD", typeof(SortingGroup)).GetComponent<Transform>(), document.Layers);

			document.Dispose();*/
		}

		private void OnWizardOtherButton() {
			Close();
		}
		
		private void ProcessLayers(Transform parent, List<AbstractLayer> layers) {
			for (var i = layers.Count - 1; i >= 0; i--) {
				var layer = layers[i];
				if (layer is TextLayer textLayer) {
					//CreateText(parent, textLayer);
					CreateImage(parent, textLayer);
				} else if (layer is ImageLayer imageLayer) {
					CreateImage(parent, imageLayer);
				} else if (layer is GroupLayer groupLayer) {
					ProcessLayers(parent, groupLayer.ChildLayer);
				}	
			}
		}
		
		private void CreateText(Transform parent, TextLayer layer) {
			var text = new GameObject(layer.Name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
			text.transform.parent = parent;
			text.rectTransform.sizeDelta = new Vector2(layer.TextRect.width, layer.TextRect.height);
			text.rectTransform.rotation = layer.Matrix.rotation;
			text.rectTransform.localPosition = new Vector2(layer.TextRect.center.x, layer.TextRect.center.y);
			text.text = layer.Text;
			//text.transform.ma
		}

		private void CreateImage(Transform parent, ImageLayer layer) {
			var testImage = new GameObject(layer.Name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
			var transform = testImage.transform;
			transform.parent = parent;
			//testImage.rectTransform.sizeDelta = new Vector2(layer.Rectangle.Width, layer.Rectangle.Height);
			transform.localPosition = new Vector2(layer.Rectangle.center.x / 100.0f, layer.Rectangle.center.y/ 100.0f);
				
			var texture = new Texture2D((int)layer.Rectangle.width, (int)layer.Rectangle.height);
			texture.SetPixels32(layer.Pixels.ToArray());
			texture.Apply();

			var sprite = Sprite.Create(texture,
				new Rect(0.0f, 0.0f, layer.Rectangle.width, layer.Rectangle.height), Vector2.one * 0.5f);
			testImage.sprite = sprite;
			testImage.color = new Color(1.0f, 1.0f, 1.0f, layer.Opacity); 
		}
	}
}