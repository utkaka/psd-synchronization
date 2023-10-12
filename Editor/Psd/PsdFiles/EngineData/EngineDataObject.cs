/////////////////////////////////////////////////////////////////////////////////
//
// This software is provided under the MIT License:
//   Copyright (c) 2022-2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.EngineData {
	public class EngineDataObject {
		private object _value;

		public EngineDataObject(object value) {
			_value = value;
		}

		public EngineDataObject this[string key] {
			get => ((Dictionary<string, EngineDataObject>) _value)[key];
			set => ((Dictionary<string, EngineDataObject>) _value)[key] = value;
		}

		public EngineDataObject this[int index] => ((List<EngineDataObject>) _value)[index];

		public bool IsDictionary() {
			return _value is Dictionary<string, EngineDataObject>;
		}
		
		public Dictionary<string, EngineDataObject> AsDictionary() {
			return _value as Dictionary<string, EngineDataObject>;
		}
		
		public bool IsList() {
			return _value is List<EngineDataObject>;
		}
		
		public List<EngineDataObject> AsList() {
			return _value as List<EngineDataObject>;
		}
		
		public bool IsString() {
			return _value is string;
		}

		public string GetStringValue() {
			return (string)_value;
		}

		public bool IsBool() {
			return _value is bool;
		}
		
		public bool GetBoolValue() {
			return (bool)_value;
		}

		public bool IsFloat() {
			return _value is float;
		}
		
		public float GetFloatValue() {
			return (float)_value;
		}
		
		public bool IsInt() {
			return _value is int;
		}

		public int GetIntValue() {
			return (int)_value;
		}
	}
}