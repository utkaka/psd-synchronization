using System;
using System.IO;
using com.utkaka.PsdSynchronization.Editor.Psd;
using com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.utkaka.PsdSynchronization.Editor {
	public class ImportPsdWizard : ScriptableWizard {
		private static string _lastPsdPath;
		private AssetContextConfig _assetContextConfig;
		private string _psdPath;

		[MenuItem("Assets/Import PSD file")]
		private static void CreateWizard() {
			var psdPath = EditorUtility.OpenFilePanel("Choose a PSD file", _lastPsdPath, "psd,psb");
			if (string.IsNullOrEmpty(psdPath)) return;
			_lastPsdPath = Directory.GetParent(psdPath)?.FullName;
			var wizard = DisplayWizard<ImportPsdWizard>("Import PSD", "Save", "");
			var psdFileName = Path.GetFileNameWithoutExtension(psdPath);
			wizard._assetContextConfig = new AssetContextConfig {
				RootObjectName = psdFileName,
				WorkingSceneName = $"{psdFileName} PSD Scene",
				PrefabsFolderName = "Prefabs",
				SpritesFolderName = "Sprites"
			};
			wizard._psdPath = psdPath;
		}
		
		private void OnWizardCreate() {
			var loggerType = PsdSynchronizationSettingsProvider.GetLoggerType();
			var context = loggerType == PsdSynchronizationSettingsProvider.LoggerType.Console
				? new Context(Debug.unityLogger)
				: new Context(); 

			var stream = File.OpenRead(_psdPath);
			/*var psdFile = new PsdFile(stream, context);
			
			return;*/
			var psdObject = new PsdRootObject("", _assetContextConfig.RootObjectName, stream, context);
			stream.Close();
			psdObject.CreateMainAsset(new AssetContext(_assetContextConfig));
		}

		protected override bool DrawWizardGUI() {
			EditorGUI.BeginChangeCheck();
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Save path", EditorStyles.label, GUILayout.Width(EditorGUIUtility.labelWidth - 1),
				GUILayout.Height(EditorGUIUtility.singleLineHeight));
			GUILayout.Label(string.IsNullOrEmpty(_assetContextConfig.BasePath) ? "..." : $"Assets/{_assetContextConfig.BasePath}");
			if (GUILayout.Button("Browse...", GUILayout.ExpandWidth(false),
				    GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
				var newPath = EditorUtility.SaveFolderPanel("Where to save assets", "Assets",
					_assetContextConfig.RootObjectName);
				if (!string.IsNullOrEmpty(newPath)) {
					_assetContextConfig.BasePath = Path.GetRelativePath(Application.dataPath, newPath);
				}
				GUIUtility.ExitGUI();
			} 
			GUILayout.EndHorizontal();

			_assetContextConfig.ImportPrefabMode =
				(ImportPrefabMode)EditorGUILayout.EnumPopup("Import Prefab Mode", _assetContextConfig.ImportPrefabMode);
			_assetContextConfig.RootObjectName =
				EditorGUILayout.TextField("Root Object Name", _assetContextConfig.RootObjectName);
			_assetContextConfig.WorkingSceneName =
				EditorGUILayout.TextField("Working Scene Name", _assetContextConfig.WorkingSceneName);
			_assetContextConfig.PrefabsFolderName =
				EditorGUILayout.TextField("Prefabs Folder Name", _assetContextConfig.PrefabsFolderName);
			_assetContextConfig.SpritesFolderName =
				EditorGUILayout.TextField("Sprites Folder Name", _assetContextConfig.SpritesFolderName);
			
			
			GUILayout.Space(10);
			
			if (string.IsNullOrEmpty(_assetContextConfig.BasePath)) {
				errorString = "Please specify save path";
				isValid = false;
			} else {
				errorString = "";
				isValid = true;
			}
			return EditorGUI.EndChangeCheck();
		}

		/*private void ProcessLayers(Transform parent, List<AbstractLayer> layers) {
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
		}*/
	}
}