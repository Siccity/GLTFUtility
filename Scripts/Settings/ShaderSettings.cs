using System;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> Defines which shaders to use in the gltf import process </summary>
	[Serializable]
	public class ShaderSettings {
		public static ShaderSettings Default {
			get {
				return new ShaderSettings() {
					metallic = Shader.Find("GLTFUtility/Standard (Metallic)"),
						metallicBlend = Shader.Find("GLTFUtility/Standard Transparent (Metallic)"),
						specular = Shader.Find("GLTFUtility/Standard (Specular)"),
						specularBlend = Shader.Find("GLTFUtility/Standard Transparent (Specular)")
				};
			}
		}
		private static ShaderSettings _default;

		[SerializeField] private Shader metallic;
		public Shader Metallic { get { return metallic != null ? metallic : Default.metallic; } }

		[SerializeField] private Shader metallicBlend;
		public Shader MetallicBlend { get { return metallicBlend != null ? metallicBlend : Default.metallicBlend; } }

		[SerializeField] private Shader specular;
		public Shader Specular { get { return specular != null ? specular : Default.specular; } }

		[SerializeField] private Shader specularBlend;
		public Shader SpecularBlend { get { return specularBlend != null ? specularBlend : Default.specularBlend; } }
	}
}