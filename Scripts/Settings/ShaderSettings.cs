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

#if UNITY_2019_1_OR_NEWER
		[SerializeField] private bool useDefaultShader = true;
#endif
		public Shader Metallic { get { return metallic; } }
		public Shader MetallicBlend { get { return metallicBlend; } }
		public Shader Specular { get { return specular; } }
		public Shader SpecularBlend { get { return specularBlend; } }

#if UNITY_2019_1_OR_NEWER
		public bool UseDefaultShader
		{
			get { return this.useDefaultShader; }
		}
#endif
	}
}