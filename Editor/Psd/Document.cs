using System.Collections.Generic;
using com.utkaka.PsdPlugin.Layers;
using com.utkaka.PsdPlugin.PsdFiles;

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