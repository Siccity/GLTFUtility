using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#texture
	public class GLTFTexture {

		public int? sampler;
		public int? source;
		public string name;

		public class ImportResult {
			public GLTFImage.ImportResult image;
		}

		public ImportResult Import(GLTFImage.ImportResult[] images) {
			if (source.HasValue) {
				ImportResult result = new ImportResult();
				result.image = images[source.Value];
				return result;
			}
			return null;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFTexture> textures, GLTFImage.ImportTask imageTask) : base(imageTask) {
				task = new Task(() => {
					if (textures == null) return;

					Result = new ImportResult[textures.Count];
					for (int i = 0; i < Result.Length; i++) {
						Result[i] = textures[i].Import(imageTask.Result);
					}
				});
			}
		}
	}
}