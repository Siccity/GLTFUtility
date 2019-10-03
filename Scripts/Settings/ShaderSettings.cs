using System;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> Defines which shaders to use in the gltf import process </summary>
	[Serializable]
	public class ShaderSettings {
		[SerializeField] private Shader metallic;
		public Shader Metallic { get { return metallic != null ? metallic : Shader.Find("GLTFUtility/Standard (Metallic)"); } }

		[SerializeField] private Shader metallicBlend;
		public Shader MetallicBlend { get { return metallicBlend != null ? metallicBlend : Shader.Find("GLTFUtility/Standard Transparent (Metallic)"); } }

		[SerializeField] private Shader specular;
		public Shader Specular { get { return specular != null ? specular : Shader.Find("GLTFUtility/Standard (Specular)"); } }

		[SerializeField] private Shader specularBlend;
		public Shader SpecularBlend { get { return specularBlend != null ? specularBlend : Shader.Find("GLTFUtility/Standard Transparent (Specular)"); } }

		/// <summary> Caches default shaders so that async import won't try to search for them while on a separate thread </summary>
		public void CacheDefaultShaders() {
			metallic = Metallic;
			metallicBlend = MetallicBlend;
			specular = Specular;
			specularBlend = SpecularBlend;
		}
	}
}