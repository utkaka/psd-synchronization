/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.IO;
using System.Text;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles {
	/// <summary>
	/// Reads PSD data types in big-endian byte order.
	/// </summary>
	public class PsdBinaryReader : IDisposable {
		private BinaryReader _reader;
		private Context _context;

		public Stream BaseStream => _reader.BaseStream;

		public Context Context => _context;

		public PsdBinaryReader(Stream stream, PsdBinaryReader reader)
			: this(stream, reader._context) { }

		public PsdBinaryReader(Stream stream, Context context) {
			// ReadPascalString and ReadUnicodeString handle encoding explicitly.
			// BinaryReader.ReadString() is never called, so it is constructed with
			// ASCII encoding to make accidental usage obvious.
			_reader = new BinaryReader(stream, Encoding.ASCII);
			_context = context;
		}
		
		public void Log(LogType logType, string message) {
			_context.Logger.Log(logType, $"PsdBinaryReader: 0x{BaseStream.Position:x}, {BaseStream.Position}, {message}");
		}

		public byte ReadByte() {
			return _reader.ReadByte();
		}

		public byte[] ReadBytes(int count) {
			return _reader.ReadBytes(count);
		}
		
		public byte[] ReadBytes(long count) {
			var result = new byte[count];
			for (var i = 0L; i < count; i++) {
				result[i] = _reader.ReadByte();
			}
			return result;
		}
		
		public byte[] ReadBytes(ulong count) {
			var result = new byte[count];
			for (var i = 0UL; i < count; i++) {
				result[i] = _reader.ReadByte();
			}
			return result;
		}

		public bool ReadBoolean() {
			return _reader.ReadBoolean();
		}

		public Int16 ReadInt16() {
			var val = _reader.ReadInt16();
			unsafe {
				Util.SwapBytes((byte*) &val, 2);
			}

			return val;
		}

		public Int32 ReadInt32() {
			var val = _reader.ReadInt32();
			unsafe {
				Util.SwapBytes((byte*) &val, 4);
			}

			return val;
		}

		public Int64 ReadInt64() {
			var val = _reader.ReadInt64();
			unsafe {
				Util.SwapBytes((byte*) &val, 8);
			}

			return val;
		}

		public UInt16 ReadUInt16() {
			var val = _reader.ReadUInt16();
			unsafe {
				Util.SwapBytes((byte*) &val, 2);
			}

			return val;
		}

		public UInt32 ReadUInt32() {
			var val = _reader.ReadUInt32();
			unsafe {
				Util.SwapBytes((byte*) &val, 4);
			}

			return val;
		}

		public UInt64 ReadUInt64() {
			var val = _reader.ReadUInt64();
			unsafe {
				Util.SwapBytes((byte*) &val, 8);
			}

			return val;
		}
		
		public double ReadDouble() {
			var val = _reader.ReadDouble();
			unsafe {
				Util.SwapBytes((byte*) &val, 8);
			}
			return val;
		}

		//////////////////////////////////////////////////////////////////

		/// <summary>
		/// Read padding to get to the byte multiple for the block.
		/// </summary>
		/// <param name="startPosition">Starting position of the padded block.</param>
		/// <param name="padMultiple">Byte multiple that the block is padded to.</param>
		public void ReadPadding(long startPosition, int padMultiple) {
			// Pad to specified byte multiple
			var totalLength = _reader.BaseStream.Position - startPosition;
			var padBytes = Util.GetPadding((int) totalLength, padMultiple);
			ReadBytes(padBytes);
		}

		public Rectangle ReadRectangle() {
			var rect = new Rectangle();
			rect.Y = ReadInt32();
			rect.X = ReadInt32();
			rect.Height = ReadInt32() - rect.Y;
			rect.Width = ReadInt32() - rect.X;
			return rect;
		}
		
		/// <summary>
		/// Read a fixed-length ASCII string.
		/// </summary>
		public string ReadKey() {
			var keyLength = ReadInt32();
			var key = ReadAsciiChars(keyLength == 0 ? 4 : keyLength);
			return key;
		}

		/// <summary>
		/// Read a fixed-length ASCII string.
		/// </summary>
		public string ReadAsciiChars(int count) {
			var bytes = _reader.ReadBytes(count);
			;
			var s = Encoding.ASCII.GetString(bytes);
			return s;
		}

		/// <summary>
		/// Read a Pascal string using the specified encoding.
		/// </summary>
		/// <param name="padMultiple">Byte multiple that the Pascal string is padded to.</param>
		public string ReadPascalString(int padMultiple) {
			var startPosition = _reader.BaseStream.Position;

			byte stringLength = ReadByte();
			var bytes = ReadBytes(stringLength);
			ReadPadding(startPosition, padMultiple);

			// Default decoder uses best-fit fallback, so it will not throw any
			// exceptions if unknown characters are encountered.
			var str = _context.Encoding.GetString(bytes);
			return str;
		}

		public string ReadUnicodeString() {
			var numChars = ReadInt32();
			var length = 2 * numChars;
			var data = ReadBytes(length);
			var str = Encoding.BigEndianUnicode.GetString(data, 0, length);

			return str;
		}
		
		public void SkipBytes(int i) {
			_reader.BaseStream.Position += i;
		}

		//////////////////////////////////////////////////////////////////

		# region IDisposable

		private bool disposed = false;

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			// Check to see if Dispose has already been called. 
			if (disposed) {
				return;
			}

			if (disposing) {
				if (_reader != null) {
					// BinaryReader.Dispose() is protected.
					_reader.Close();
					_reader = null;
				}
			}

			disposed = true;
		}

		#endregion
	}
}