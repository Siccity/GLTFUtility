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

		public Shader metallic;
		public Shader metallicBlend;
		public Shader specular;
		public Shader specularBlend;
	}
}