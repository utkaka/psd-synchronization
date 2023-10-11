using System.Collections.Generic;
using System.Drawing;
using com.utkaka.Psd.PsdFiles;
using com.utkaka.Psd.PsdFiles.Layers;
using com.utkaka.Psd.PsdFiles.Layers.LayerInfo;

namespace com.utkaka.Psd.Layers {
	public class GroupLayer : AbstractLayer {
		public List<AbstractLayer> ChildLayer { get; }

		public GroupLayer(Layer psdFileLayer, GroupLayer parentLayer) : base(psdFileLayer, parentLayer){
			ChildLayer = new List<AbstractLayer>();
		}

		protected override Layer ToPsdLayer(PsdFile psdFile) {
			var psdLayer = base.ToPsdLayer(psdFile);
			var sectionInfo = new AbstractLayerSectionInfo("lsct", LayerSectionSubtype.Normal, LayerSectionType.ClosedFolder);
			psdLayer.AdditionalInfo.Add(sectionInfo);
			return psdLayer;
		}

		public override void WriteLayer(PsdFile psdFile) {
			base.WriteLayer(psdFile);
			foreach (var layer in ChildLayer) {
				layer.WriteLayer(psdFile);
			}
			var endSectionLayer = new Layer(psdFile);
			endSectionLayer.Name = "</Layer group>";
			endSectionLayer.Rect = new Rectangle();
			endSectionLayer.BlendModeKey = "norm";
			endSectionLayer.Opacity = 255;
			endSectionLayer.Visible = true;
			endSectionLayer.Masks = new MaskInfo();
			endSectionLayer.BlendingRangesData = new BlendingRanges(endSectionLayer);
			var sectionInfo = new AbstractLayerSectionInfo("lsct", LayerSectionSubtype.Normal, LayerSectionType.SectionDivider);
			endSectionLayer.AdditionalInfo.Add(sectionInfo);
			psdFile.Layers.Add(endSectionLayer);
		}

		public override void Dispose() {
			foreach (var layer in ChildLayer) {
				layer.Dispose();	
			}
		}
	}
}