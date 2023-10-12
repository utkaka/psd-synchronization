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
using System.Globalization;
using System.Text;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.EngineData {
	public static class EngineDataReader {
		public static EngineDataObject Read(byte[] stream) {
			var reader = new Reader();
			return reader.Read(stream);
		}
	}

	internal class Reader {
		private byte[] _stream;
		private int _index;

		internal EngineDataObject Read(byte[] stream) {
			_stream = stream;
			_index = 0;
			return ReadObject();
		}

		private string ReadString() {
			var stringLength = 0;
			while (_stream[_index + stringLength] is not (Constants.Space or Constants.LineFeed)) {
				stringLength++;
			}
			var result = Encoding.ASCII.GetString(_stream, _index, stringLength);
			_index += stringLength;
			return result;
		}

		private void SkipEmptyData() {
			while (_stream[_index] is Constants.LineFeed or Constants.Tab or Constants.Space) {
				_index++;
			}
		}

		private EngineDataObject ReadObject() {
			SkipEmptyData();
			//Dictionary
			if (_stream[_index] == Constants.DictionaryStart && _stream[_index + 1] == Constants.DictionaryStart) {
				_index += 2;
				var dictionary = new Dictionary<string, EngineDataObject>();
				SkipEmptyData();
				while (!(_stream[_index] == Constants.DictionaryEnd && _stream[_index + 1] == Constants.DictionaryEnd)) {
					if (_stream[_index] != Constants.PropertyStart) throw new Exception("Property token expected");
					_index++;
					var key = ReadString();
					dictionary[key] = ReadObject();
					SkipEmptyData();
				}
				_index += 2;
				return new EngineDataObject(dictionary);
			}
			//Array
			if (_stream[_index] == Constants.ArrayStart) {
				_index++;
				SkipEmptyData();
				var array = new List<EngineDataObject>();
				while (_stream[_index] != Constants.ArrayEnd) {
					array.Add(ReadObject());
					SkipEmptyData();
				}
				_index++;
				return new EngineDataObject(array);
			}
			//UTF-16 String
			if (_stream[_index] == Constants.TextStart && _stream[_index + 1] == Constants.TextSecond && _stream[_index + 2] == Constants.TextThird) {
				_index += 3;
				var length = 0;
				while (!(_stream[_index + length] == Constants.TextEnd && _stream[_index + length + 1] == Constants.LineFeed)) {
					length++;
				}
				var stringValue =  Encoding.Unicode.GetString(_stream, _index, length);
				_index += length + 1;
				return new EngineDataObject(stringValue);
			}
			
			//bool, float, int
			var valueType = ReadString();
			if (valueType is "true" or "false") {
				return new EngineDataObject(valueType == "true");
			}
			if (valueType.Contains('.')) {
				var result = new EngineDataObject(float.Parse(valueType, CultureInfo.InvariantCulture));
				return result;
			}
			return new EngineDataObject(int.Parse(valueType, CultureInfo.InvariantCulture));
		}
	}
}