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
		protected override bool OnLoad() {
			Source = glTFObject.images[source];
			return true;
		}
	}
}