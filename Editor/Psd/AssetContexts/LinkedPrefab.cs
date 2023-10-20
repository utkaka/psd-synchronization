using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.AssetContexts {
	public class LinkedPrefab {
		public readonly GameObject Prefab;
		public readonly Vector2 OriginalSize;

		public LinkedPrefab(GameObject prefab, Vector2 originalSize) {
			Prefab = prefab;
			OriginalSize = originalSize;
		}
	}
}