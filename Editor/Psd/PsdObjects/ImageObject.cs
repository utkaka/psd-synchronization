using System;
using System.IO;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Decoding;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdObjects {
	[Serializable]
	public class ImageObject : AbstractPsdObject {

		private JobHandle _jobHandle;
		private NativeArray<Color32> _pixels;
		
		public ImageObject(Layer psdFileLayer, GroupObject parentObject) : base(psdFileLayer, parentObject){
			psdFileLayer.CreateMissingChannels();
			if (psdFileLayer.Rect.Width <= 0 || psdFileLayer.Rect.Height <= 0) return;
			_pixels = new NativeArray<Color32>(psdFileLayer.Rect.Width * psdFileLayer.Rect.Height, Allocator.TempJob);
			_jobHandle = ImageDecoder.DecodeImage(psdFileLayer, _pixels);
		}

		public override void SaveAssets(string psdName, GameObject parentObject, SaveAssetsContext saveAssetsContext) {
			if (!_pixels.IsCreated) return;
			_jobHandle.Complete();
			var spritesFolder = string.IsNullOrEmpty(psdName)
				? saveAssetsContext.SpritesFolderName
				: Path.Combine(saveAssetsContext.SpritesFolderName, psdName);
			var assetParentDirectory = Path.Combine("Assets", saveAssetsContext.BasePath, spritesFolder);
			var assetPath = $"{Path.Combine(assetParentDirectory, Name)}.png";
			var texture = new Texture2D((int)Rect.width, (int)Rect.height, TextureFormat.RGBA32, false);
			texture.SetPixels32(_pixels.ToArray());
			_pixels.Dispose();
			texture.Apply();
			if (!Directory.Exists(assetParentDirectory)) Directory.CreateDirectory(assetParentDirectory);
			File.WriteAllBytes(assetPath, texture.EncodeToPNG());
			AssetDatabase.ImportAsset(assetPath);

			var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
			
			var gameObject = CreateGameObject(saveAssetsContext);
			gameObject.transform.SetParent(parentObject.transform);
			switch (saveAssetsContext.ImportPrefabType) {
				case PsdPrefabType.World:
					gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
					break;
				case PsdPrefabType.UGUIWithoutCanvas:
				case PsdPrefabType.UGUIWithCanvas:
					gameObject.GetComponent<Image>().sprite = sprite;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		protected override GameObject CreateGameObject(SaveAssetsContext saveAssetsContext) {
			GameObject gameObject;
			switch (saveAssetsContext.ImportPrefabType) {
				case PsdPrefabType.World:
					gameObject = new GameObject(Name, typeof(SpriteRenderer));
					gameObject.transform.localPosition = new Vector2(Rect.center.x / 100.0f, Rect.center.y/ 100.0f);
					break;
				case PsdPrefabType.UGUIWithoutCanvas:
				case PsdPrefabType.UGUIWithCanvas:
					gameObject = new GameObject(Name, typeof(Image));
					((RectTransform)gameObject.transform).sizeDelta = new Vector2(Rect.width, Rect.height);
					gameObject.transform.localPosition = new Vector2(Rect.center.x, Rect.center.y);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return gameObject;
		}
		
		protected override Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = base.ToPsdLayer(psdFile);
			// Store channel metadata
			int layerSize = _pixels.Length;
			for (int i = -1; i < 3; i++) {
				var ch = new Channel((short) i, psdLayer);
				ch.ImageCompression = psdFile.ImageCompression;
				ch.ImageData = new byte[layerSize];
				psdLayer.Channels.Add(ch);
			}

			// Store and compress channel image data
			var channelsArray = psdLayer.Channels.ToIdArray();
			//TODO: Fix encoding
			//ImageEncoder.EncodeImage(channelsArray, psdLayer.AlphaChannel, Pixels, psdLayer.Rect);
			return psdLayer;
		}
	}
}