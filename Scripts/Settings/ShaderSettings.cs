using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siccity.GLTFUtility
{
	public enum PiplineType
	{
		Default,
		URP,
		HDRP
	}
	/// <summary> Defines which shaders to use in the gltf import process </summary>
	[Serializable]
	public class ShaderSettings
	{
		public static bool bHDRPShader
		{
			get
			{
				return GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.name.Contains("HDR");
			}
		}
		[SerializeField] private Shader metallic;
		public Shader Metallic { get { return metallic != null ? metallic : GetDefaultMetallic(); } }

		[SerializeField] private Shader metallicTwoSide;
		public Shader MetallicTwoSide { get { return metallicTwoSide != null ? metallicTwoSide : GetDefaultMetallicTwoSide(); } }

		[SerializeField] private Shader metallicBlend;
		public Shader MetallicBlend { get { return metallicBlend != null ? metallicBlend : GetDefaultMetallicBlend(); } }

		[SerializeField] private Shader metallicBlendTwoSide;
		public Shader MetallicBlendTwoSide { get { return metallicBlendTwoSide != null ? metallicBlendTwoSide : GetDefaultMetallicBlendTwoSide(); } }

		[SerializeField] private Shader specular;
		public Shader Specular { get { return specular != null ? specular : GetDefaultSpecular(); } }

		[SerializeField] private Shader specularTwoSide;
		public Shader SpecularTwoSide { get { return specularTwoSide != null ? specularTwoSide : GetDefaultSpecularTwoSide(); } }

		[SerializeField] private Shader specularBlend;
		public Shader SpecularBlend { get { return specularBlend != null ? specularBlend : GetDefaultSpecularBlend(); } }

		[SerializeField] private Shader specularBlendTwoSide;
		public Shader SpecularBlendTwoSide { get { return specularBlendTwoSide != null ? specularBlendTwoSide : GetDefaultSpecularBlendTwoSide(); } }

		/// <summary> Caches default shaders so that async import won't try to search for them while on a separate thread </summary>
		public void CacheDefaultShaders()
		{
			metallic = Metallic;
			metallicTwoSide = MetallicTwoSide;
			metallicBlend = MetallicBlend;
			metallicBlendTwoSide = MetallicBlendTwoSide;
			specular = Specular;
			specularTwoSide = SpecularTwoSide;
			specularBlend = SpecularBlend;
			specularBlendTwoSide = SpecularBlendTwoSide;
		}

		public Shader GetDefaultMetallic()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard (Metallic)");
			}
			else if (GraphicsSettings.renderPipelineAsset != null)
			{
				return Shader.Find("GLTFUtility/URP/Standard (Metallic)");
			}
			else
#endif
				return Shader.Find("GLTFUtility/Standard (Metallic)");
		}

		public Shader GetDefaultMetallicTwoSide()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard (Metallic) TwoSide");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard (Metallic) TwoSide");
			else
#endif
				return Shader.Find("GLTFUtility/Standard (Metallic) TwoSide");
		}

		public Shader GetDefaultMetallicBlend()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard Transparent (Metallic)");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard Transparent (Metallic)");
			else
#endif
				return Shader.Find("GLTFUtility/Standard Transparent (Metallic)");
		}
		public Shader GetDefaultMetallicBlendTwoSide()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard Transparent (Metallic) TwoSide");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard Transparent (Metallic) TwoSide");
			else
#endif
				return Shader.Find("GLTFUtility/Standard Transparent (Metallic) TwoSide");
		}

		public Shader GetDefaultSpecular()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard (Specular)");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard (Specular)");
			else
#endif
				return Shader.Find("GLTFUtility/Standard (Specular)");
		}

		public Shader GetDefaultSpecularTwoSide()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard (Specular) TwoSide");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard (Specular) TwoSide");
			else
#endif
				return Shader.Find("GLTFUtility/Standard (Specular) TwoSide");
		}

		public Shader GetDefaultSpecularBlend()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard Transparent (Specular)");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard Transparent (Specular)");
			else
#endif
				return Shader.Find("GLTFUtility/Standard Transparent (Specular)");
		}

		public Shader GetDefaultSpecularBlendTwoSide()
		{
#if UNITY_2019_1_OR_NEWER
			if (bHDRPShader)
			{
				return Shader.Find("Shader Graphs/HDRP_Standard Transparent (Specular) TwoSide");
			}
			else
			if (GraphicsSettings.renderPipelineAsset) return Shader.Find("GLTFUtility/URP/Standard Transparent (Specular) TwoSide");
			else
#endif
				return Shader.Find("GLTFUtility/Standard Transparent (Specular) TwoSide");
		}
	}
}