/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//   Copyright (c) 2023 Anton Alexeyev
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils {
	/// <summary>
	/// Fixed-point decimal, with 16-bit integer and 16-bit fraction.
	/// </summary>
	[Serializable]
	public class UFixed1616 {
		[SerializeField]
		private ushort _integer;
		[SerializeField]
		private ushort _fraction;
		public ushort Integer => _integer;
		public ushort Fraction => _fraction;

		public UFixed1616(ushort integer, ushort fraction) {
			_integer = integer;
			_fraction = fraction;
		}

		/// <summary>
		/// Split the high and low words of a 32-bit unsigned integer into a
		/// fixed-point number.
		/// </summary>
		public UFixed1616(uint value) {
			_integer = (ushort) (value >> 16);
			_fraction = (ushort) (value & 0x0000ffff);
		}

		public UFixed1616(double value) {
			if (value >= 65536.0) throw new OverflowException();
			if (value < 0) throw new OverflowException();
			_integer = (ushort) value;
			// Round instead of truncate, because doubles may not represent the
			// fraction exactly.
			_fraction = (ushort) ((value - _integer) * 65536 + 0.5);
		}

		public static implicit operator double(UFixed1616 value) {
			return value._integer + value._fraction / 65536.0;
		}
	}
}