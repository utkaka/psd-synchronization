using System;
using System.Collections.Generic;
using System.IO;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts {
	public class AssetContext {
		private readonly AssetContextConfig _config;
		private Dictionary<string, LinkedPrefab> _linkedObjects;
		private readonly string _prefabsFolder;

		public AssetContext(AssetContextConfig config) {
			_config = config;
			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
			scene.name = _config.WorkingSceneName;
			var scenePath = $"{Path.Combine("Assets", _config.BasePath, scene.name)}.unity";
			EditorSceneManager.SaveScene(scene, scenePath);
			EditorSceneManager.OpenScene(scenePath);
			_linkedObjects = new Dictionary<string, LinkedPrefab>();
			_prefabsFolder = Path.Combine("Assets", _config.BasePath, _config.PrefabsFolderName);
			if (!Directory.Exists(_prefabsFolder)) Directory.CreateDirectory(_prefabsFolder);
		}
		
		public Transform CreateRootObject(string name) {
			var root = _config.ImportPrefabMode switch {
				ImportPrefabMode.World => CreateGameObject(name, typeof(SortingGroup)),
				ImportPrefabMode.UGUIWithoutCanvas => CreateGameObject(name, typeof(RectTransform)),
				ImportPrefabMode.UGUIWithCanvas => CreateGameObject(name, typeof(Canvas), typeof(CanvasScaler)),
				_ => throw new ArgumentOutOfRangeException()
			};
			PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject,
				Path.Combine(_prefabsFolder, $"{name}.prefab"),
				InteractionMode.AutomatedAction);
			return root;
		}

		public void CreateLinkedRootObject(string id, string name, string path, Vector2 originalSize, ImageObject baseImage) {
			if (_linkedObjects.ContainsKey(id)) return;
			var transform = baseImage.CreateAsset(null, this);
			var prefabFolder = Path.Combine(_prefabsFolder, path);
			if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);
			var prefabPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(prefabFolder, $"{name}.prefab"));
			var linkedPrefab = new LinkedPrefab(PrefabUtility.SaveAsPrefabAssetAndConnect(transform.gameObject,
				prefabPath,
				InteractionMode.AutomatedAction), originalSize);
			_linkedObjects.Add(id, linkedPrefab);
			Object.DestroyImmediate(transform.gameObject);
		}
		
		public Transform CreateGroupObject(string name, Rect rect, Transform parent) {
			var transform = _config.ImportPrefabMode switch {
				ImportPrefabMode.World => CreateGameObject(name, rect, parent),
				ImportPrefabMode.UGUIWithoutCanvas => CreateGameObject(name, rect, parent, typeof(RectTransform)),
				ImportPrefabMode.UGUIWithCanvas => CreateGameObject(name, rect, parent, typeof(RectTransform)),
				_ => throw new ArgumentOutOfRangeException()
			};
			return transform;
		}

		public Transform CreateImageObject(string name, Rect rect, Transform parent, NativeArray<Color32> pixels,
			float opacity) {
			var spritesFolder = "";
			var tempParent = parent;
			while (tempParent != null) {
				spritesFolder = Path.Combine(tempParent.name, spritesFolder);
				tempParent = tempParent.parent;
			}
			spritesFolder = Path.Combine(Path.Combine("Assets", _config.BasePath, _config.SpritesFolderName), spritesFolder);
			if (!Directory.Exists(spritesFolder)) Directory.CreateDirectory(spritesFolder);
			var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(spritesFolder, name)}.png");
			var texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
			texture.SetPixels32(pixels.ToArray());
			texture.Apply();
			File.WriteAllBytes(assetPath, texture.EncodeToPNG());
			AssetDatabase.ImportAsset(assetPath);
			var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
			importer.textureType = TextureImporterType.Sprite;
			EditorUtility.SetDirty(importer);
			importer.SaveAndReimport();

			var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
			Transform transform;
			var color = Color.white;
			color.a = opacity;
			switch (_config.ImportPrefabMode) {
				case ImportPrefabMode.World:
					transform = CreateGameObject(name, rect, parent, typeof(SpriteRenderer));
					var spriteRenderer = transform.GetComponent<SpriteRenderer>(); 
					spriteRenderer.sprite = sprite;
					spriteRenderer.color = color;
					break;
				case ImportPrefabMode.UGUIWithoutCanvas:
				case ImportPrefabMode.UGUIWithCanvas:
					transform = CreateGameObject(name, rect, parent, typeof(Image));
					((RectTransform)transform).sizeDelta = new Vector2(rect.width, rect.height);
					var image = transform.GetComponent<Image>();
					image.sprite = sprite;
					image.color = color;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return transform;
		}

		public Transform CreateLinkedObject(string id, string name, Rect rect, Quaternion quaternion, Transform parent) {
			var linkedPrefab = _linkedObjects[id];
			var transform = ((GameObject) PrefabUtility.InstantiatePrefab(linkedPrefab.Prefab))
				.GetComponent<Transform>();
			transform.gameObject.name = name;
			transform.localScale = rect.size / linkedPrefab.OriginalSize;
			transform.rotation = quaternion;
			PlaceGameObject(transform, rect, parent);
			return transform;
		}

		private void PlaceGameObject(Transform transform, Rect rect, Transform parent) {
			if (_config.ImportPrefabMode == ImportPrefabMode.World) {
				transform.position = new Vector2(rect.center.x / 100.0f, rect.center.y / 100.0f);	
			} else {
				var rectTransform = (RectTransform)transform;
				rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
				rectTransform.localPosition = new Vector2(rect.center.x, rect.center.y);
			}
			transform.SetParent(parent);
		}

		private Transform CreateGameObject(string name, Rect rect, Transform parent, params Type[] components) {
			var transform = CreateGameObject(name, components);
			PlaceGameObject(transform, rect, parent);
			return transform;
		}

		private static Transform CreateGameObject(string name, params Type[] components) {
			return new GameObject(name, components).GetComponent<Transform>();
		}
	}
}