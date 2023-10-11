/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace com.utkaka.Psd.PsdFiles.EngineData {
	public static class EngineDataWriter {
		public static byte[] Write(EngineDataObject root) {
			var stream = new List<byte> {
				Constants.LineFeed
			};
			WriteObject(stream, root);
			return stream.ToArray();
		}

		private static void WriteNewLine(List<byte> stream, int indent) {
			stream.Add(Constants.LineFeed);
			for (var i = 0; i < indent; i++) {
				stream.Add(Constants.Tab);
			}
		}

		private static void WriteObject(List<byte> stream, EngineDataObject target, int indent = 1) {
			if (target.IsDictionary()) {
				var dictionary = target.AsDictionary();
				WriteNewLine(stream, indent - 1);
				stream.Add(Constants.DictionaryStart);
				stream.Add(Constants.DictionaryStart);
				foreach (var key in dictionary.Keys) {
					WriteNewLine(stream, indent);
					stream.Add(Constants.PropertyStart);
					stream.AddRange(Encoding.ASCII.GetBytes(key));
					WriteObject(stream, dictionary[key], indent + 1);
					//WriteNewLine(stream, indent);
				}
				WriteNewLine(stream, indent - 1);
				stream.Add(Constants.DictionaryEnd);
				stream.Add(Constants.DictionaryEnd);
			} else if (target.IsList()) {
				var list = target.AsList();
				stream.Add(Constants.Space);
				stream.Add(Constants.ArrayStart);
				foreach (var item in list) {
					WriteObject(stream, item, indent);
				}

				if (list.Count == 0 || list[0].IsBool() || list[0].IsFloat() || list[0].IsInt()) {
					stream.Add(Constants.Space);	
				} else {
					WriteNewLine(stream, indent);
				}
				stream.Add(Constants.ArrayEnd);
			} else if (target.IsString()) {
				var stringValue = target.GetStringValue();
				stream.Add(Constants.Space);
				stream.Add(Constants.TextStart);
				stream.Add(Constants.TextSecond);
				stream.Add(Constants.TextThird);
				var stringBytes = Encoding.Unicode.GetBytes(stringValue);
				stream.AddRange(stringBytes);
				stream.Add(Constants.TextEnd);
			} else if (target.IsBool()) {
				var boolValue = target.GetBoolValue();
				stream.Add(Constants.Space);
				stream.AddRange(Encoding.ASCII.GetBytes(boolValue ? "true" : "false"));
			} else if (target.IsFloat()) {
				stream.Add(Constants.Space);
				var floatValue = target.GetFloatValue();
				var floatString = floatValue.ToString("F8", CultureInfo.InvariantCulture);
				stream.AddRange(Encoding.ASCII.GetBytes(floatString));
			} else if (target.IsInt()) {
				var intValue = target.GetIntValue();
				stream.Add(Constants.Space);
				stream.AddRange(Encoding.ASCII.GetBytes(intValue.ToString(CultureInfo.InvariantCulture)));
			}
		}
	}
}