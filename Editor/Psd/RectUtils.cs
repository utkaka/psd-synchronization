using System.Drawing;
using UnityEngine;

namespace com.utkaka.Psd {
	public static class RectUtils {

		public static Rect RectFromBounds(double left, double top, double right, double bottom) {
			var result = new Rect();
			result.x = (float)left;
			result.y = (float)top;
			result.width = (float)(right - left);
			result.height = (float)(bottom - top);
			return result;
		}

		public static Rect ConvertToUnitySpace(this Rect rectangle, int documentWidth, int documentHeight) {
			return new Rect(rectangle.x - documentWidth / 2.0f, documentHeight / 2.0f - rectangle.y - rectangle.height,
				rectangle.width, rectangle.height);
		}

		public static Rect ConvertToPsdSpace(this Rect rectangle, int documentWidth, int documentHeight) {
			return new Rect(rectangle.x + documentWidth / 2.0f, documentHeight / 2.0f - rectangle.y - rectangle.height,
				rectangle.width, rectangle.height);
		}

		public static Rect ToRect(this Rectangle rectangle) {
			return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		public static Rectangle ToRectangle(this Rect rect) {
			return new Rectangle(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width),
				Mathf.RoundToInt(rect.height));
		}
	}
}