using System;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> Defines which shaders to use in the gltf import process </summary>
	[CreateAssetMenu]
	public class ShaderSettings : ScriptableObject {
		[SerializeField] private Shader metallic;
		[SerializeField] private Shader metallicBlend;
		[SerializeField] private Shader specular;
		[SerializeField] private Shader specularBlend;

		public Shader Metallic { get { return metallic; } }
		public Shader MetallicBlend { get { return metallicBlend; } }
		public Shader Specular { get { return specular; } }
		public Shader SpecularBlend { get { return specularBlend; } }
	}
}