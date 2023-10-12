using System.IO;
using System.Text;
using com.utkaka.PsdSynchronization.Editor.Psd.ImageProcessing.Encoding;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers;

namespace com.utkaka.PsdSynchronization.Editor.Psd {
	public static class DocumentSaver {
		public static void Save(Document input, Stream output) {
			var psdVersion = ((input.Height > 30000) || (input.Width > 30000))
				? PsdFileVersion.PsbLargeDocument
				: PsdFileVersion.Psd;
			var psdFile = new PsdFile(psdVersion);

			psdFile.RowCount = input.Height;
			psdFile.ColumnCount = input.Width;

			// We only save in RGBA format, 8 bits per channel, which corresponds to
			// Paint.NET's internal representation.

			psdFile.ChannelCount = 4;
			psdFile.ColorMode = PsdColorMode.RGB;
			psdFile.BitDepth = 8;
			psdFile.Resolution = input.Resolution;
			psdFile.ImageCompression = input.ImageCompression;
			
			for (short i = 0; i < psdFile.ChannelCount; i++)
			{
				var channel = new Channel(i, psdFile.BaseLayer) {
					ImageData = new byte[input.BaseLayer.Pixels.Length],
					ImageCompression = psdFile.ImageCompression
				};
				psdFile.BaseLayer.Channels.Add(channel);
			}
			var channelsArray = psdFile.BaseLayer.Channels.ToIdArray();
			ImageEncoder.EncodeImage(channelsArray, channelsArray[3],
				input.BaseLayer.Pixels, psdFile.BaseLayer.Rect);

			foreach (var pdnLayer in input.Layers) {
				pdnLayer.WriteLayer(psdFile);
			}

			psdFile.Layers.Reverse();

			psdFile.Save(output, new Context());
		}
	}
}