using com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Utils;
using UnityEngine;

namespace com.utkaka.PsdSynchronization.Editor.Psd.Extensions {
	public static class MatrixExtensions {
		public static Quaternion GetRotation(this Matrix3X3 matrix) {
			var forward = new Vector3(0.0f, 0.0f, 1.0f);
			var upwards = new Vector3(matrix.m01, matrix.m11, 0.0f);
			var v2 = new Vector3(matrix.m01, matrix.m11, matrix.m21);
			return Quaternion.Inverse(Quaternion.LookRotation(forward,  upwards * v2.normalized.y));
		}
 
		public static Vector3 GetPosition(this Matrix3X3 matrix) {
			return new Vector3(matrix.m02, matrix.m12, 0.0f);
		}
 
		public static Vector3 GetScale(this Matrix3X3 matrix) {
			var v1 = new Vector3(matrix.m00, matrix.m10, matrix.m20);
			var v2 = new Vector3(matrix.m01, matrix.m11, matrix.m21);
			return new Vector3(
				v1.magnitude * (v1.normalized.x < 0.0f ? -1.0f : 1.0f),
				v2.magnitude * (v2.normalized.y < 0.0f ? -1.0f : 1.0f),
				0.0f);
		}
	}
}