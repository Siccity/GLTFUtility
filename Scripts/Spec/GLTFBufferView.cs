using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#bufferview
	/// <summary> Defines sections within the Buffer </summary>
	public class GLTFBufferView {

		[JsonProperty(Required = Required.Always)] public int buffer;
		[JsonProperty(Required = Required.Always)] public int byteLength;
		public int byteOffset = 0;
		public int? byteStride;
		/// <summary> OpenGL buffer target </summary>
		public int? target;
		public string name;

		public class ImportResult {
			public byte[] bytes;

			public byte[] GetBytes(int byteOffset = 0) {
				if (byteOffset != 0) return bytes.SubArray(byteOffset, bytes.Length - byteOffset);
				else return bytes;
			}
		}
	}

	public static class GLTFBufferViewExtensions {
#region Import
		public static Task<GLTFBufferView.ImportResult[]> ImportTask(this List<GLTFBufferView> bufferViews, GLTFBuffer.ImportResult[] buffers) {
			return new Task<GLTFBufferView.ImportResult[]>(() => {
				GLTFBufferView.ImportResult[] results = new GLTFBufferView.ImportResult[bufferViews.Count];
				for (int i = 0; i < results.Length; i++) {
					int byteOffset = bufferViews[i].byteOffset;
					int byteLength = bufferViews[i].byteLength;
					GLTFBuffer.ImportResult buffer = buffers[bufferViews[i].buffer];
					GLTFBufferView.ImportResult result = new GLTFBufferView.ImportResult();
					result.bytes = buffer.bytes.SubArray(byteOffset, byteLength);
					results[i] = result;
				}
				return results;
			});
		}
#endregion
	}
}