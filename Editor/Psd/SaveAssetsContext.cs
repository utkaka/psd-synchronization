using System;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd {
	public class SaveAssetsContext {
		public PsdPrefabType ImportPrefabType { get; set; }
		public string BasePath { get; set; }
		public string RootObjectName { get; set; }
		public string WorkingSceneName { get; set; }
		public string SpritesFolderName { get; set; }
	}
}