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
		private readonly Transform _linkedObjectsRoot;
		private Dictionary<string, LinkedPrefab> _linkedObjects;
		private readonly string _prefabsFolder;

		public AssetContext(AssetContextConfig config) {
			_config = config;
			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
			scene.name = _config.WorkingSceneName;
			var scenePath = $"{Path.Combine("Assets", _config.BasePath, scene.name)}.unity";
			EditorSceneManager.SaveScene(scene, scenePath);
			EditorSceneManager.OpenScene(scenePath);
			_linkedObjectsRoot = CreateGameObject("Linked Objects",
				_config.ImportPrefabMode != ImportPrefabMode.World ? typeof(Canvas) : null);
			_linkedObjectsRoot.gameObject.SetActive(false);
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

		public Transform CreateLinkedRootObject(string id, string name, string path, Vector2 originalSize, ImageObject baseImage) {
			if (_linkedObjects.ContainsKey(id)) return null;
			var transform = _config.ImportPrefabMode switch {
				ImportPrefabMode.World => CreateGameObject(name, Rect.zero, _linkedObjectsRoot),
				ImportPrefabMode.UGUIWithoutCanvas => CreateGameObject(name,  Rect.zero, _linkedObjectsRoot, typeof(RectTransform)),
				ImportPrefabMode.UGUIWithCanvas => CreateGameObject(name,  Rect.zero, _linkedObjectsRoot, typeof(RectTransform)),
				_ => throw new ArgumentOutOfRangeException()
			};
			if (_config.ImportLinkedObjectsMode == ImportLinkedObjectsMode.SingleImageFromInnerLayers) {
				baseImage.CreateAsset(transform, this);
			}
			var prefabFolder = Path.Combine(_prefabsFolder, path);
			if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);
			var linkedPrefab = new LinkedPrefab(PrefabUtility.SaveAsPrefabAssetAndConnect(transform.gameObject,
				Path.Combine(prefabFolder, $"{name}.prefab"),
				InteractionMode.AutomatedAction), originalSize);
			_linkedObjects.Add(id, linkedPrefab);
			return _config.ImportLinkedObjectsMode == ImportLinkedObjectsMode.FullInnerLayers ? transform : null;
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
			var assetPath = $"{Path.Combine(spritesFolder, name)}.png";
			var texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
			texture.SetPixels32(pixels.ToArray());
			texture.Apply();
			if (!Directory.Exists(spritesFolder)) Directory.CreateDirectory(spritesFolder);
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

		public Transform CreateLinkedObject(string id, string name, Rect rect, Transform parent, ImageObject imageObject) {
			var linkedPrefab = _linkedObjects[id];
			if (_config.ImportLinkedObjectsMode == ImportLinkedObjectsMode.SingleImageFromFirstUsage &&
			    linkedPrefab.Prefab.transform.childCount == 0) {
				var prefabTransform = ((GameObject) PrefabUtility.InstantiatePrefab(linkedPrefab.Prefab))
					.GetComponent<Transform>();
				imageObject.CreateAsset(prefabTransform, this);
				prefabTransform.GetChild(0).localPosition = Vector3.zero;
				PrefabUtility.ApplyPrefabInstance(prefabTransform.gameObject, InteractionMode.AutomatedAction);
				Object.DestroyImmediate(prefabTransform.gameObject);
			}
			var transform = ((GameObject) PrefabUtility.InstantiatePrefab(linkedPrefab.Prefab))
				.GetComponent<Transform>();
			transform.gameObject.name = name;
			//transform.localScale = rect.size / linkedPrefab.OriginalSize;
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