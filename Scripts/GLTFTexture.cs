using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#texture
	public class GLTFTexture {

#region Serialized fields
		public int? sampler;
		public int? source;
		public string name;
#endregion

#region Non-serialized fields
		[JsonIgnore] public GLTFImage Source;
#endregion

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

		protected override bool OnLoad() {
			if (source.HasValue) {
				Source = glTFObject.images[source.Value];
				return true;
			}
			return false;
		}
	}
}