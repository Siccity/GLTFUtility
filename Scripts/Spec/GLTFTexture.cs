using System;
using System.Collections.Generic;
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
	}

	public static class GLTFTextureExtensions {
		public static GLTFTexture.ImportResult[] Import(this List<GLTFTexture> textures, GLTFImage.ImportResult[] images) {
			if (textures == null) return null;

			GLTFTexture.ImportResult[] results = new GLTFTexture.ImportResult[textures.Count];
			for (int i = 0; i < textures.Count; i++) {
				results[i] = textures[i].Import(images);
			}
			return results;
		}
	}
}