/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using com.utkaka.Psd.PsdFiles.Descriptors;
using com.utkaka.Psd.PsdFiles.Descriptors.Elements;
using com.utkaka.Psd.PsdFiles.EngineData;

namespace com.utkaka.Psd.PsdFiles.Layers.LayerInfo {
	public struct TypeToolMatrix {
		public double XX;
		public double XY;
		public double YX;
		public double YY;
		public double TX;
		public double TY;
	}
	public class TypeToolInfo : AbstractLayerInfo {
		public override string Key => "TySh";
		public override string Signature => "8BIM";
		
		private readonly short _version;
		private readonly short _textVersion;
		private readonly int _textDescriptorVersion;
		private readonly Descriptor _textDescriptor;
		private readonly short _warpVersion;
		private readonly int _warpDescriptorVersion;
		private readonly Descriptor _warpDescriptor;
		private readonly byte[] _rect;
		private readonly TypeToolMatrix _transformMatrix;
		private readonly EngineDataObject _engineData;

		public short Version => _version;

		public short TextVersion => _textVersion;

		public int TextDescriptorVersion => _textDescriptorVersion;

		public Descriptor TextDescriptor => _textDescriptor;

		public short WarpVersion => _warpVersion;

		public int WarpDescriptorVersion => _warpDescriptorVersion;

		public Descriptor WarpDescriptor => _warpDescriptor;

		public byte[] Rect => _rect;

		public TypeToolMatrix TransformMatrix => _transformMatrix;

		public EngineDataObject EngineData => _engineData;

		public TypeToolInfo(PsdBinaryReader reader) {
			_version = reader.ReadInt16();

			_transformMatrix = new TypeToolMatrix {
				XX = reader.ReadDouble(),
				XY = reader.ReadDouble(),
				YX = reader.ReadDouble(),
				YY = reader.ReadDouble(),
				TX = reader.ReadDouble(),
				TY = reader.ReadDouble(),
			};

			_textVersion = reader.ReadInt16();
			_textDescriptorVersion = reader.ReadInt32();
			_textDescriptor = new Descriptor(reader);
			_warpVersion = reader.ReadInt16();
			_warpDescriptorVersion = reader.ReadInt32();
			_warpDescriptor = new Descriptor(reader);
			
			//According to the documentation there should be 4 * 8 bytes (left, top, right, bottom), but there are only 4
			_rect = reader.ReadBytes(4);
			
			_engineData = EngineDataReader.Read((_textDescriptor.Items["EngineData"] as RawDataElement)?.RawData);

		}

		public TypeToolInfo(TypeToolMatrix matrix, Descriptor textDescriptor, Descriptor warpDescriptor, EngineDataObject engineData) {
			_version = 1;
			_textVersion = 50;
			_textDescriptorVersion = 16;
			_warpVersion = 1;
			_warpDescriptorVersion = 16;
			_rect = new byte[] {0, 0, 0, 0};

			_transformMatrix = matrix;
			_textDescriptor = textDescriptor;
			_warpDescriptor = warpDescriptor;
			_engineData = engineData;
		}

		protected override void WriteData(PsdBinaryWriter writer) {
			_textDescriptor.Items["EngineData"] = new RawDataElement(EngineDataWriter.Write(_engineData));
			writer.Write(_version);
			writer.Write(_transformMatrix.XX);
			writer.Write(_transformMatrix.XY);
			writer.Write(_transformMatrix.YX);
			writer.Write(_transformMatrix.YY);
			writer.Write(_transformMatrix.TX);
			writer.Write(_transformMatrix.TY);
			writer.Write(_textVersion);
			writer.Write(_textDescriptorVersion);
			_textDescriptor.Write(writer);
			writer.Write(_warpVersion);
			writer.Write(_warpDescriptorVersion);
			_warpDescriptor.Write(writer);
			writer.Write(_rect);
		}
	}
}