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

		public class ImportTask : Importer.ImportTask {
			public override Task Task { get { return task; } }
			public Task<ImportResult[]> task;

			public ImportTask(List<GLTFTexture> textures, GLTFImage.ImportTask imageTask) : base(imageTask) {
				task = new Task<ImportResult[]>(() => {
					if (textures == null) return new ImportResult[0];

					ImportResult[] results = new ImportResult[textures.Count];
					for (int i = 0; i < results.Length; i++) {
						results[i] = textures[i].Import(imageTask.task.Result);
					}
					return results;
				});
			}

			protected override void OnCompleted() { }
		}
	}
}