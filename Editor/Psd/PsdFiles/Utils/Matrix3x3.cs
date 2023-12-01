using System.Numerics;

namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils {
	public struct Matrix3X3 {
		public static Matrix3X3 Identity { get; } = new(
			new Vector3(1f, 0.0f, 0.0f),
			new Vector3(0.0f, 1f, 0.0f), 
			new Vector3(0.0f, 0.0f, 1f));

		public float m00;
		public float m10;
		public float m20;
		public float m01;
		public float m11;
		public float m21;
		public float m02;
		public float m12;
		public float m22;
		
		public Matrix3X3(Vector3 column0, Vector3 column1, Vector3 column2) {
			m00 = column0.X;
			m01 = column1.X;
			m02 = column2.X;
			m10 = column0.Y;
			m11 = column1.Y;
			m12 = column2.Y;
			m20 = column0.Z;
			m21 = column1.Z;
			m22 = column2.Z;
		}

		public Matrix3X3(double[] points) {
			var p1 = new Vector2((float)points[4], (float)points[5]);
			var p2 = new Vector2((float)points[2], (float)points[3]);
			var p3 = new Vector2((float)points[0], (float)points[1]);
			var p4 = new Vector2((float)points[6], (float)points[7]);
			
			var j = p1.X - p2.X - p3.X + p4.X;
			var k = -p1.X - p2.X + p3.X + p4.X;
			var l = -p1.X + p2.X - p3.X + p4.X;
			var m = p1.Y - p2.Y - p3.Y + p4.Y;
			var n = -p1.Y - p2.Y + p3.Y + p4.Y;
			var o = -p1.Y + p2.Y - p3.Y + p4.Y;

			m22 = 1.0f;
			m21 = 0.0f;
			m20 = 0.0f;
			var m21Div = m * k - j * n;
			if (m21Div != 0) m21 = (j * o - m * l) / m21Div;
			if (j != 0) m20 = (k * m21 - l * m22) / j;
			m12 = (p1.Y * (m20 + m21 + m22) + p3.Y * (-m20 - m21 + m22)) / 2.0f;
			m11 = (p1.Y * (m20 + m21 + m22) - p2.Y * (m20 - m21 + m22)) / 2.0f;
			m10 = p1.Y * (m20 + m21 + m22) - m12 - m11;
			m02 = (p1.X * (m20 + m21 + m22) + p3.X * (-m20 - m21 + m22)) / 2.0f;
			m01 = (p1.X * (m20 + m21 + m22) - p2.X * (m20 - m21 + m22)) / 2.0f;
			m00 = p1.X * (m20 + m21 + m22) - m02 - m01;
		}
	}
}