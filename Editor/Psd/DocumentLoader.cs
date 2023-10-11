using System;
using System.Linq;
using com.utkaka.Psd.Layers;
using com.utkaka.Psd.PsdFiles;
using com.utkaka.Psd.PsdFiles.ImageResources;
using com.utkaka.Psd.PsdFiles.Layers;
using com.utkaka.Psd.PsdFiles.Layers.LayerInfo;

namespace com.utkaka.Psd {
	public static class DocumentLoader {
		public static Document Load(System.IO.Stream input) {
			var psdFile = new PsdFile(input, new LoadContext());

			// Multichannel images are loaded by processing each channel as a
			// grayscale layer.
			if (psdFile.ColorMode == PsdColorMode.Multichannel) {
				CreateLayersFromChannels(psdFile);
				psdFile.ColorMode = PsdColorMode.Grayscale;
			}

			var document = new Document {
				Width = psdFile.ColumnCount,
				Height = psdFile.RowCount,
				ImageCompression = psdFile.ImageCompression,
				Resolution = psdFile.Resolution,
				BaseLayer = new ImageLayer(psdFile.BaseLayer, null)
			};

			psdFile.VerifyLayerSections();
			GroupLayer parentLayer = null;
			for (var i = psdFile.Layers.Count - 1; i >= 0; i--) {
				var psdFileLayer = psdFile.Layers[i];
				var sectionInfo = (AbstractLayerSectionInfo)psdFileLayer.AdditionalInfo
					.SingleOrDefault(x => x is AbstractLayerSectionInfo);
				AbstractLayer layer = null;
				if (sectionInfo == null || sectionInfo.SectionType == LayerSectionType.Layer) {
					var typeToolInfo = psdFileLayer.AdditionalInfo
						.SingleOrDefault(x => x is TypeToolInfo) as TypeToolInfo;
					layer = typeToolInfo != null ? new TextLayer(psdFileLayer, parentLayer, typeToolInfo) : new ImageLayer(psdFileLayer, parentLayer);
				} else if (sectionInfo.SectionType == LayerSectionType.ClosedFolder || sectionInfo.SectionType == LayerSectionType.OpenFolder) {
					layer = new GroupLayer(psdFileLayer, parentLayer);
				} else {
					parentLayer = parentLayer?.ParentLayer;
				}
				if (layer != null) {
					if (parentLayer != null) {
						parentLayer.ChildLayer.Add(layer);
					} else {
						document.Layers.Add(layer);	
					}	
				}

				if (layer is GroupLayer) {
					parentLayer = layer as GroupLayer;
				}
			}

			return document;
		}

		/// <summary>
		/// Creates a layer for each channel in a multichannel image.
		/// </summary>
		private static void CreateLayersFromChannels(PsdFile psdFile) {
			if (psdFile.ColorMode != PsdColorMode.Multichannel) {
				throw new Exception("Not a multichannel image.");
			}

			if (psdFile.Layers.Count > 0) {
				throw new PsdInvalidException("Multichannel image should not have layers.");
			}

			// Get alpha channel names, preferably in Unicode.
			var alphaChannelNames = (AlphaChannelNames) psdFile.ImageResourceList
				.Get(ResourceID.AlphaChannelNames);
			var unicodeAlphaNames = (UnicodeAlphaNames) psdFile.ImageResourceList
				.Get(ResourceID.UnicodeAlphaNames);
			if (alphaChannelNames == null && unicodeAlphaNames == null) {
				throw new PsdInvalidException("No channel names found.");
			}

			var channelNames = (unicodeAlphaNames != null)
				? unicodeAlphaNames.ChannelNames
				: alphaChannelNames.ChannelNames;
			var channels = psdFile.BaseLayer.Channels;
			if (channels.Count > channelNames.Count) {
				throw new PsdInvalidException("More channels than channel names.");
			}

			// Channels are stored from top to bottom, but layers are stored from
			// bottom to top.
			var channelsNamesReversed = channels.Zip(channelNames, Tuple.Create).Reverse();
			foreach (var (channel, channelName) in channelsNamesReversed) {
				// Copy metadata over from base layer
				var layer = new Layer(psdFile) {
					Rect = psdFile.BaseLayer.Rect,
					Visible = true,
					Masks = new MaskInfo()
				};
				layer.BlendingRangesData = new BlendingRanges(layer);

				// We do not attempt to reconstruct the appearance of the image, but
				// only to provide access to the channels image data.
				layer.Name = channelName;
				layer.BlendModeKey = PsdBlendMode.Darken;
				layer.Opacity = 255;

				// Copy channel image data into the new grayscale layer
				var layerChannel = new Channel(0, layer);
				layerChannel.ImageCompression = channel.ImageCompression;
				layerChannel.ImageData = channel.ImageData;
				layer.Channels.Add(layerChannel);

				psdFile.Layers.Add(layer);
			}
		}
	}
}