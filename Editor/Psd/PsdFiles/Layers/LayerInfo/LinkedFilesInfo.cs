/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.utkaka.Psd.PsdFiles.Descriptors;
using com.utkaka.Psd.PsdFiles.Descriptors.Elements;

namespace com.utkaka.Psd.PsdFiles.Layers.LayerInfo {
    public class LinkedFilesInfo : AbstractLayerInfo {
        public override string Key => _key;
        public override string Signature => "8BIM";

        private readonly string _key;
		
        private readonly List<LinkedFile> _linkedFiles;

        public LinkedFilesInfo(string key, PsdBinaryReader reader, long length) {
            _key = key;
            _linkedFiles = new List<LinkedFile>();
            var endPosition = reader.BaseStream.Position + length;
			
            while (reader.BaseStream.Position < endPosition - 8) {
                var size = reader.ReadUInt64();
                _linkedFiles.Add(new LinkedFile(reader, size));
            }
			
            reader.BaseStream.Position = endPosition;
        }
		
        protected override void WriteData(PsdBinaryWriter writer) {
            foreach (var linkedFile in _linkedFiles) {
                var startPosition = writer.BaseStream.Position;
                writer.Write(0ul);
                linkedFile.WriteData(writer);
                var endPosition = writer.BaseStream.Position;
                var dataSize = endPosition - startPosition - 8;
                writer.BaseStream.Position = startPosition;
                writer.Write((ulong)dataSize);
                writer.BaseStream.Position = endPosition;
            }
        }
    }
    
	public class LinkedFile {
        private readonly string _type;
        private readonly int _version;
        private readonly string _id;
        private readonly string _name;
        private readonly string _fileType;
        private readonly string _fileCreator;
        private readonly bool _hasFileOpenDescriptor;
        private readonly uint _fileOpenDescriptorVersion;
        private readonly Descriptor _fileOpenDescriptor;
        private readonly uint _linkedFileDescriptorVersion;
        private readonly Descriptor _linkedFileDescriptor;
        private readonly DateTime _date;
        private readonly PsdFile _file;
        private readonly string _childDocumentID;
        private readonly double _assetModTime;
        private readonly bool _assetLockedState;

        public LinkedFile(PsdBinaryReader reader, ulong size) {
			var startPosition = reader.BaseStream.Position;
            /*
            liFD - linked file data
            liFE - linked file external
            liFA - linked file alias
            */
            _type = reader.ReadAsciiChars(4);
            _version  = reader.ReadInt32();
            _id  = reader.ReadPascalString(1);
            _name = reader.ReadUnicodeString();
            _fileType  = reader.ReadAsciiChars(4).Trim();
            _fileCreator  = reader.ReadAsciiChars(4).Trim();
            var dataSize  = reader.ReadUInt64();
            _hasFileOpenDescriptor = reader.ReadBoolean();

            if (_hasFileOpenDescriptor) {
                _fileOpenDescriptorVersion = reader.ReadUInt32();
                _fileOpenDescriptor = new Descriptor(reader);
            }

            if (_type == "liFE") {
                _linkedFileDescriptorVersion = reader.ReadUInt32();
                _linkedFileDescriptor = new Descriptor(reader);	
            }
            
            if (_type == "liFE" && _version > 3) {
                var year  = reader.ReadInt32();
                var month  = reader.ReadByte();
                var day  = reader.ReadByte();
                var hour  = reader.ReadByte();
                var minute  = reader.ReadByte();
                var seconds  = reader.ReadDouble();
                _date = new DateTime(year, month, day, hour, minute, (int)Math.Floor(seconds));
            }

            byte[] fileData = null;
            var fileSize = _type == "liFE" ? reader.ReadUInt64() : 0;
            if (_type == "liFA") reader.SkipBytes(8);
            if (_type == "liFD") fileData = reader.ReadBytes(dataSize); // seems to be a typo in docs
            if (_version >= 5) _childDocumentID = reader.ReadUnicodeString();
            if (_version >= 6) _assetModTime = reader.ReadDouble();
            if (_version >= 7) _assetLockedState = reader.ReadBoolean();
            if (_type == "liFE" && _version == 2) fileData = reader.ReadBytes(fileSize);

            if (fileData != null) {
                _file = new PsdFile(new MemoryStream(fileData), new LoadContext());
            }
            while (size % 4 > 0) size++;
            reader.BaseStream.Position = startPosition + (long)size;
		}

        public LinkedFile(PsdFile psdFile, string id, string name) {
            _type = "liFD";
            _version = 7;
            _id = id;
            _name = name;
            _fileType = "8BPB";
            _fileCreator = "8BIM";
            _hasFileOpenDescriptor = true;
            _fileOpenDescriptorVersion = 16;
            
            var compInfoDescriptor =
                new DescriptorElement("\0", "null", new Dictionary<string, AbstractDescriptorElement> {
                    {"compID", new IntegerElement(-1)},
                    {"originalCompID", new IntegerElement(-1)}
                });
            _fileOpenDescriptor = new Descriptor("\0", "null",
                new Dictionary<string, AbstractDescriptorElement> {{"compInfo", compInfoDescriptor}});
            
            _date = DateTime.Now;
            _childDocumentID = "\0";
            _file = psdFile;
        }
        
        public void WriteData(PsdBinaryWriter writer) {
            var startPosition = writer.BaseStream.Position;
            writer.WriteAsciiChars(_type);
            writer.Write(_version);
            writer.WritePascalString(_id, 1);
            writer.WriteUnicodeString(_name);
            writer.WriteAsciiChars(_fileType);
            writer.WriteAsciiChars(_fileCreator);

            var psdStream = new MemoryStream();
            _file.Save(psdStream, Encoding.Default);
            writer.Write((ulong)psdStream.Length);
            writer.Write(_hasFileOpenDescriptor);
            if (_hasFileOpenDescriptor) {
                writer.Write(_fileOpenDescriptorVersion);
                _fileOpenDescriptor.Write(writer);
            }
            if (_type == "liFE" && _linkedFileDescriptor != null) {
                writer.Write(_linkedFileDescriptorVersion);
                _linkedFileDescriptor.Write(writer);
            }
            
            if (_type == "liFE" && _version > 3) {
                writer.Write(_date.Year);
                writer.Write((byte)_date.Month);
                writer.Write((byte)_date.Day);
                writer.Write((byte)_date.Hour);
                writer.Write((byte)_date.Minute);
                writer.Write((double)_date.Second);
            }

            if (_type == "liFE") writer.Write((ulong)psdStream.Length);
            else if (_type == "liFA") writer.Write(0L);
            else if (_type == "liFD") writer.Write(psdStream.ToArray());
            if (_version >= 5) writer.WriteUnicodeString(_childDocumentID);
            if (_version >= 6) writer.Write(_assetModTime);
            if (_version >= 7) writer.Write(_assetLockedState);
            if (_type == "liFE" && _version == 2) writer.Write(psdStream.ToArray());;
            
            writer.WritePadding(startPosition, 4);
        }
    }
}