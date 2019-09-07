using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFTexture : GLTFProperty {

#region Serialized fields
		[JsonProperty(Required = Required.Always)] public int sampler;
		[JsonProperty(Required = Required.Always)] public int source;
#endregion

#region Non-serialized fields
		[JsonIgnore] public GLTFImage Source;
#endregion

		protected override bool OnLoad() {
			Source = glTFObject.images[source];
			return true;
		}
	}
}