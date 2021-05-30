using System;
using UnityEngine;

namespace Siccity.GLTFUtility
{
	/// <summary>
	/// Copied from BaseShaderGUI.cs in URP package.
	/// Changes:
	///		Added AlphaMode -> Surface+Blend translation
	///		Simplified emission setup. 
	/// </summary>
	public class URPHelper : MonoBehaviour
	{
		private enum WorkflowMode
		{
			Specular = 0,
			Metallic
		}

		private enum SmoothnessMapChannel
		{
			SpecularMetallicAlpha,
			AlbedoAlpha,
		}

		private enum SurfaceType
		{
			Opaque,
			Transparent
		}

		private enum BlendMode
		{
			Alpha,       // Old school alpha-blending mode, fresnel does not affect amount of transparency
			Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
			Additive,
			Multiply
		}

		public static void SetMaterialKeywords(Material material, AlphaMode alphaMode, bool doubleSided)
		{
			// Clear all keywords for fresh start
			material.shaderKeywords = null;

			// Translate GLTF alpha mode to Surface and Blend modes
			switch (alphaMode)
			{
				case AlphaMode.OPAQUE:
					material.SetFloat("_AlphaClip", 0.0f);
					material.SetFloat("_Surface", 0.0f);
					break;
				case AlphaMode.MASK:
					material.SetFloat("_AlphaClip", 1.0f);
					material.SetFloat("_Surface", 0.0f);
					break;
				case AlphaMode.BLEND:
					material.SetFloat("_AlphaClip", 0.0f);
					material.SetFloat("_Surface", 1.0f);
					material.SetFloat("_Blend", 0.0f);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(alphaMode), alphaMode, null);
			}

			// Set front (2) or double sided rendering (0)
			material.SetFloat("_Cull", doubleSided ? 0.0f : 2.0f);

			// Setup blending - consistent across all Universal RP shaders
			SetupMaterialBlendMode(material);

			// Receive Shadows
			if (material.HasProperty("_ReceiveShadows"))
				SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);

			// Emission
			var shouldEmissionBeEnabled =
				material.HasProperty("_EmissionColor") && material.GetColor("_EmissionColor").maxColorComponent > 0.0f
				|| material.HasProperty("_EmissionMap") && material.GetTexture("_EmissionMap") != null;
			SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

			// Normal Map
			if (material.HasProperty("_BumpMap"))
				SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
			// Shader specific keyword functions
			SetURPMaterialKeywords(material);
		}

		private static void SetURPMaterialKeywords(Material material)
		{
			// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
			// (MaterialProperty value might come from renderer material property block)
			var hasGlossMap = false;
			var isSpecularWorkFlow = false;
			var opaque = ((SurfaceType) material.GetFloat("_Surface") == SurfaceType.Opaque);
			if (material.HasProperty("_WorkflowMode"))
			{
				isSpecularWorkFlow = (WorkflowMode) material.GetFloat("_WorkflowMode") == WorkflowMode.Specular;
				if (isSpecularWorkFlow)
					hasGlossMap = material.GetTexture("_SpecGlossMap") != null;
				else
					hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
			}
			else
			{
				hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
			}

			SetKeyword(material, "_SPECULAR_SETUP", isSpecularWorkFlow);

			SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);

			if (material.HasProperty("_SpecularHighlights"))
				SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
				           material.GetFloat("_SpecularHighlights") == 0.0f);
			if (material.HasProperty("_EnvironmentReflections"))
				SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
				           material.GetFloat("_EnvironmentReflections") == 0.0f);
			if (material.HasProperty("_OcclusionMap"))
				SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));

			if (material.HasProperty("_SmoothnessTextureChannel"))
			{
				SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
				           GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && opaque);
			}
		}

		private static void SetupMaterialBlendMode(Material material)
		{
			if (material == null)
				throw new ArgumentNullException("material");

			bool alphaClip = material.GetFloat("_AlphaClip") == 1;
			if (alphaClip)
			{
				material.EnableKeyword("_ALPHATEST_ON");
			}
			else
			{
				material.DisableKeyword("_ALPHATEST_ON");
			}

			var queueOffset = 0; // queueOffsetRange;
			if (material.HasProperty("_QueueOffset"))
				queueOffset = 50 - (int) material.GetFloat("_QueueOffset");

			SurfaceType surfaceType = (SurfaceType) material.GetFloat("_Surface");
			if (surfaceType == SurfaceType.Opaque)
			{
				if (alphaClip)
				{
					material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
					material.SetOverrideTag("RenderType", "TransparentCutout");
				}
				else
				{
					material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;
					material.SetOverrideTag("RenderType", "Opaque");
				}

				material.renderQueue += queueOffset;
				material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.SetShaderPassEnabled("ShadowCaster", true);
			}
			else
			{
				BlendMode blendMode = (BlendMode) material.GetFloat("_Blend");
				var queue = (int) UnityEngine.Rendering.RenderQueue.Transparent;

				// Specific Transparent Mode Settings
				switch (blendMode)
				{
					case BlendMode.Alpha:
						material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
						material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						break;
					case BlendMode.Premultiply:
						material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
						material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
						break;
					case BlendMode.Additive:
						material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
						material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.One);
						material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						break;
					case BlendMode.Multiply:
						material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.DstColor);
						material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
						material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						material.EnableKeyword("_ALPHAMODULATE_ON");
						break;
				}

				// General Transparent Material Settings
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_ZWrite", 0);
				material.renderQueue = queue + queueOffset;
				material.SetShaderPassEnabled("ShadowCaster", false);
			}
		}

		private static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
		{
			int ch = (int) material.GetFloat("_SmoothnessTextureChannel");
			if (ch == (int) SmoothnessMapChannel.AlbedoAlpha)
				return SmoothnessMapChannel.AlbedoAlpha;

			return SmoothnessMapChannel.SpecularMetallicAlpha;
		}

		private static void SetKeyword(Material material, string keyword, bool state)
		{
			if (state)
				material.EnableKeyword(keyword);
			else
				material.DisableKeyword(keyword);
		}
	}
}