/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2017 Tao Yue
//
// See LICENSE for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles {
	public class RleRowLengths {
		public int[] Values { get; private set; }

		public long Total => Values.Sum(x => (long) x);

		public int this[int i] {
			get => Values[i];
			set => Values[i] = value;
		}

		public RleRowLengths(int rowCount) {
			Values = new int[rowCount];
		}

		public RleRowLengths(PsdBinaryReader reader, int rowCount, bool isLargeDocument)
			: this(rowCount) {
			for (int i = 0; i < rowCount; i++) {
				Values[i] = isLargeDocument
					? reader.ReadInt32()
					: reader.ReadUInt16();
			}
		}

		public void Write(PsdBinaryWriter writer, bool isLargeDocument) {
			for (int i = 0; i < Values.Length; i++) {
				if (isLargeDocument) {
					writer.Write(Values[i]);
				} else {
					writer.Write((UInt16) Values[i]);
				}
			}
		}
	}
}