using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#buffer
	/// <summary> Contains raw binary data </summary>
	public class GLTFBuffer {

		[JsonProperty(Required = Required.Always)] public int byteLength;
		public string uri;
		public string name;

		[JsonIgnore] private const string embeddedPrefix = "data:application/octet-stream;base64,";

		public class ImportResult {
			public byte[] bytes;
		}

#region Import
		public ImportResult Import(string filepath) {
			ImportResult result = new ImportResult();

			if (uri == null) {
				// Load entire file
				result.bytes = File.ReadAllBytes(filepath);
			} else if (uri.StartsWith(embeddedPrefix)) {
				// Load embedded
				string b64 = uri.Substring(embeddedPrefix.Length, uri.Length - embeddedPrefix.Length);
				result.bytes = Convert.FromBase64String(b64);
			} else {
				// Load URI
				string directoryRoot = Directory.GetParent(filepath).ToString() + "/";
				result.bytes = File.ReadAllBytes(directoryRoot + uri);
			}

			// Sometimes the buffer is part of a larger file. Since we dont have a byteOffset we have to assume it's at the end of the file.
			// In case you're trying to load a gltf with more than one buffers this might cause issues, but it'll work for now.
			int startIndex = result.bytes.Length - byteLength;
			if (startIndex != 0) result.bytes = result.bytes.SubArray(startIndex, byteLength);
			return result;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFBuffer> buffers, string filepath) : base() {
				task = new Task(() => {
					Result = new ImportResult[buffers.Count];
					for (int i = 0; i < Result.Length; i++) {
						Result[i] = buffers[i].Import(filepath);
					}
				});
			}
		}
#endregion
	}
}