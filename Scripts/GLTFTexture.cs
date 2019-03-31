using System;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFTexture : GLTFProperty {

#region Serialized fields
		public int sampler;
		[SerializeField] private int source;
#endregion

#region Non-serialized fields
		[SerializeField] public GLTFImage Source;

#endregion
		public override void Load() {
			Source = glTFObject.images[source];
		}
	}
}