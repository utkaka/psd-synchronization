using System.Collections.Generic;
using com.utkaka.Psd.Layers;
using com.utkaka.Psd.PsdFiles;
using com.utkaka.Psd.PsdFiles.ImageResources;

namespace com.utkaka.Psd {
	public class Document {
		public ImageLayer BaseLayer { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public List<AbstractLayer> Layers { get; set; }
		public ResolutionInfo Resolution { get; set; }
		public ImageCompression ImageCompression { get; set; }

		public Document() {
			Layers = new List<AbstractLayer>();
		}

		public void Dispose() {
			BaseLayer.Dispose();
			foreach (var layer in Layers)
				layer.Dispose();
		}
	}
}