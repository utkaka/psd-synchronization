/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2017 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.ImageResources {
	/// <summary>
	/// Summary description for ResolutionInfo.
	/// </summary>
	[Serializable]
	public class ResolutionInfo : ImageResource {
		/// <summary>
		/// 1 = pixels per inch, 2 = pixels per centimeter
		/// </summary>
		public enum ResUnit {
			PxPerInch = 1,
			PxPerCm = 2
		}
		
		/// <summary>
		/// Physical units.
		/// </summary>
		public enum Unit {
			Inches = 1,
			Centimeters = 2,
			Points = 3,
			Picas = 4,
			Columns = 5
		}
		
		[SerializeField]
		private UFixed1616 _hDpi;
		[SerializeField]
		private UFixed1616 _vDpi;
		[SerializeField]
		private ResUnit _hResDisplayUnit;
		[SerializeField]
		private ResUnit _vResDisplayUnit;
		[SerializeField]
		private Unit _widthDisplayUnit;
		[SerializeField]
		private Unit _heightDisplayUnit;
		
		public override ResourceID ID => ResourceID.ResolutionInfo;

		/// <summary>
		/// Horizontal DPI.
		/// </summary>
		public UFixed1616 HDpi => _hDpi;

		/// <summary>
		/// Vertical DPI.
		/// </summary>
		public UFixed1616 VDpi => _vDpi;

		/// <summary>
		/// Display units for horizontal resolution.  This only affects the
		/// user interface; the resolution is still stored in the PSD file
		/// as pixels/inch.
		/// </summary>
		public ResUnit HResDisplayUnit => _hResDisplayUnit;

		/// <summary>
		/// Display units for vertical resolution.
		/// </summary>
		public ResUnit VResDisplayUnit => _vResDisplayUnit;
		public Unit WidthDisplayUnit => _widthDisplayUnit;
		public Unit HeightDisplayUnit => _heightDisplayUnit;

		public ResolutionInfo(PsdBinaryReader reader, string name)
			: base(name) {
			_hDpi = new UFixed1616(reader.ReadUInt32());
			_hResDisplayUnit = (ResUnit) reader.ReadInt16();
			_widthDisplayUnit = (Unit) reader.ReadInt16();
			_vDpi = new UFixed1616(reader.ReadUInt32());
			_vResDisplayUnit = (ResUnit) reader.ReadInt16();
			_heightDisplayUnit = (Unit) reader.ReadInt16();
		}

		protected override void WriteData(PsdBinaryWriter writer) {
			writer.Write(HDpi.Integer);
			writer.Write(HDpi.Fraction);
			writer.Write((Int16) HResDisplayUnit);
			writer.Write((Int16) WidthDisplayUnit);

			writer.Write(VDpi.Integer);
			writer.Write(VDpi.Fraction);
			writer.Write((Int16) VResDisplayUnit);
			writer.Write((Int16) HeightDisplayUnit);
		}
	}
}