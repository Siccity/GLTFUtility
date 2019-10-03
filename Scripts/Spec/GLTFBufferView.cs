using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

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

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFBufferView> bufferViews, GLTFBuffer.ImportTask bufferTask) : base(bufferTask) {
				task = new Task(() => {
					Result = new ImportResult[bufferViews.Count];
					for (int i = 0; i < Result.Length; i++) {
						int byteOffset = bufferViews[i].byteOffset;
						int byteLength = bufferViews[i].byteLength;
						GLTFBuffer.ImportResult buffer = bufferTask.Result[bufferViews[i].buffer];
						ImportResult result = new ImportResult();
						result.bytes = buffer.bytes.SubArray(byteOffset, byteLength);
						Result[i] = result;
					}
				});
			}
		}
	}
}