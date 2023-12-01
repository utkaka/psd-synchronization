/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2016 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Text;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles {
	/// <summary>
	/// Contains settings and callbacks that affect the loading of a PSD file.
	/// </summary>
	public class Context {
		public string PsdPath { get; private set; }
		public Encoding Encoding { get; private set; }
		public ILogger Logger { get; private set; }

		public Context(string psdPath) {
			PsdPath = psdPath;
			Encoding = Encoding.Default;
			Logger = new DummyLogger();
		}
		
		public Context(string psdPath, ILogger logger) {
			PsdPath = psdPath;
			Encoding = Encoding.Default;
			Logger = logger;
		}
	}
}