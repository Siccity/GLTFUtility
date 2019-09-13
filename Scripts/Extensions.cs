using System;
using UnityEngine;

namespace Siccity.GLTFUtility {
	public static class Extensions {

		public static T[] SubArray<T>(this T[] data, int index, int length) {
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		public static void UnpackTRS(this Matrix4x4 trs, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale) {
			position = trs.GetColumn(3);
			position.z = -position.z;
			rotation = trs.rotation;
			rotation = new Quaternion(rotation.x, rotation.y, -rotation.z, -rotation.w);
			scale = trs.lossyScale;
		}
	}
}