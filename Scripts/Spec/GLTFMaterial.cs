using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#material
	public class GLTFMaterial {
#if UNITY_EDITOR
		public static Material defaultMaterial { get { return _defaultMaterial != null ? _defaultMaterial : _defaultMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"); } }
		private static Material _defaultMaterial;
#else
		public static Material defaultMaterial { get { return null; } }
#endif

		public string name;
		public PbrMetalRoughness pbrMetallicRoughness;
		public TextureInfo normalTexture;
		public TextureInfo occlusionTexture;
		public TextureInfo emissiveTexture;
		[JsonConverter(typeof(ColorRGBConverter))] public Color emissiveFactor = Color.black;
		[JsonConverter(typeof(EnumConverter))] public AlphaMode alphaMode = AlphaMode.OPAQUE;
		public float alphaCutoff = 0.5f;
		public bool doubleSided = false;
		public MaterialExtensions extensions;

		public class ImportResult {
			public Material[] materials;
		}

		public Material CreateMaterial(GLTFTexture.ImportResult[] textures, ShaderSettings shaderSettings) {
			Material mat;

			// Load metallic-roughness materials
			if (pbrMetallicRoughness != null) {
				mat = pbrMetallicRoughness.CreateMaterial(textures, alphaMode, shaderSettings);
			}
			// Load specular-glossiness materials
			else if (extensions != null && extensions.KHR_materials_pbrSpecularGlossiness != null) {
				mat = extensions.KHR_materials_pbrSpecularGlossiness.CreateMaterial(textures, alphaMode, shaderSettings);
			}
			// Load fallback material
			else mat = new Material(Shader.Find("Standard"));

			Texture2D tex;
			if (TryGetTexture(textures, normalTexture, out tex, x => x.GetNormalMap())) {
				mat.SetTexture("_BumpMap", tex);
				mat.EnableKeyword("_NORMALMAP");
			}
			if (TryGetTexture(textures, occlusionTexture, out tex)) {
				mat.SetTexture("_OcclusionMap", tex);
			}
			if (emissiveFactor != Color.black) {
				mat.SetColor("_EmissionColor", emissiveFactor);
				mat.EnableKeyword("_EMISSION");
			}
			if (TryGetTexture(textures, emissiveTexture, out tex)) {
				mat.SetTexture("_EmissionMap", tex);
				mat.EnableKeyword("_EMISSION");
			}
			if (alphaMode == AlphaMode.MASK) {
				mat.SetFloat("_AlphaCutoff", alphaCutoff);
			}
			mat.name = name;
			return mat;
		}

		public static bool TryGetTexture(GLTFTexture.ImportResult[] textures, TextureInfo texture, out Texture2D tex, Func<GLTFImage.ImportResult, Texture2D> getter = null) {
			tex = null;
			if (texture == null || texture.index < 0) {
				return false;
			}
			if (textures.Length <= texture.index) {
				Debug.LogWarning("Attempted to get texture index " + texture.index + " when only " + textures.Length + " exist");
				return false;
			}
			if (getter == null) tex = textures[texture.index].image.texture;
			else tex = getter(textures[texture.index].image);
			return true;
		}

		public class MaterialExtensions {
			public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness = null;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#pbrmetallicroughness
		public class PbrMetalRoughness {

			[JsonConverter(typeof(ColorRGBAConverter))] public Color baseColorFactor = Color.white;
			public TextureInfo baseColorTexture;
			public float metallicFactor = 1f;
			public float roughnessFactor = 1f;
			public TextureInfo metallicRoughnessTexture;

			public Material CreateMaterial(GLTFTexture.ImportResult[] textures, AlphaMode alphaMode, ShaderSettings shaderSettings) {
				// Shader
				Shader sh = null;
#if UNITY_2019_1_OR_NEWER
				// LWRP support
				if (GraphicsSettings.renderPipelineAsset && shaderSettings.UseDefaultShader)
					sh = GraphicsSettings.renderPipelineAsset.defaultShader;
#endif
				if (sh == null) {
					if (alphaMode == AlphaMode.BLEND) sh = shaderSettings.MetallicBlend;
					else sh = shaderSettings.Metallic;
				}

				// Material
				Material mat = new Material(sh);
				mat.color = baseColorFactor;
				mat.SetFloat("_Metallic", metallicFactor);
				mat.SetFloat("_Glossiness", 1 - roughnessFactor);
				if (baseColorTexture != null && baseColorTexture.index >= 0) {
					if (textures.Length <= baseColorTexture.index) {
						Debug.LogWarning("Attempted to get basecolor texture index " + baseColorTexture.index + " when only " + textures.Length + " exist");
					} else {
						mat.SetTexture("_MainTex", textures[baseColorTexture.index].image.texture);
					}
				}
				if (metallicRoughnessTexture != null && metallicRoughnessTexture.index >= 0) {
					if (textures.Length <= metallicRoughnessTexture.index) {
						Debug.LogWarning("Attempted to get metallicRoughness texture index " + metallicRoughnessTexture.index + " when only " + textures.Length + " exist");
					} else {
						mat.SetTexture("_MetallicGlossMap", textures[metallicRoughnessTexture.index].image.GetFixedMetallicRoughness());
						mat.EnableKeyword("_METALLICGLOSSMAP");
					}
				}

				// After the texture and color is extracted from the glTFObject
				if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mat.mainTexture);
				if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColorFactor);
				return mat;
			}
		}

		[Serializable]
		public class PbrSpecularGlossiness {

			/// <summary> The reflected diffuse factor of the material </summary>
			[JsonConverter(typeof(ColorRGBAConverter))] public Color diffuseFactor = Color.white;
			/// <summary> The diffuse texture </summary>
			public TextureInfo diffuseTexture;
			/// <summary> The reflected diffuse factor of the material </summary>
			[JsonConverter(typeof(ColorRGBConverter))] public Color specularFactor = Color.white;
			/// <summary> The glossiness or smoothness of the material </summary>
			public float glossinessFactor = 1f;
			/// <summary> The specular-glossiness texture </summary>
			public TextureInfo specularGlossinessTexture;

			public Material CreateMaterial(GLTFTexture.ImportResult[] textures, AlphaMode alphaMode, ShaderSettings shaderSettings) {
				// Shader
				Shader sh = null;
#if UNITY_2019_1_OR_NEWER
				// LWRP support
				if (GraphicsSettings.renderPipelineAsset) sh = GraphicsSettings.renderPipelineAsset.defaultShader;
#endif
				if (sh == null) {
					if (alphaMode == AlphaMode.BLEND) sh = shaderSettings.SpecularBlend;
					else sh = shaderSettings.Specular;
				}

				// Material
				Material mat = new Material(sh);
				mat.color = diffuseFactor;
				mat.SetColor("_SpecColor", specularFactor);
				mat.SetFloat("_Glossiness", glossinessFactor);

				// Diffuse texture
				if (diffuseTexture != null) {
					if (textures.Length <= diffuseTexture.index) {
						Debug.LogWarning("Attempted to get diffuseTexture texture index " + diffuseTexture.index + " when only " + textures.Length + " exist");
					} else {
						mat.SetTexture("_MainTex", textures[diffuseTexture.index].image.texture);
					}
				}
				// Specular texture
				if (specularGlossinessTexture != null) {
					if (textures.Length <= specularGlossinessTexture.index) {
						Debug.LogWarning("Attempted to get specularGlossinessTexture texture index " + specularGlossinessTexture.index + " when only " + textures.Length + " exist");
					} else {
						mat.SetTexture("_SpecGlossMap", textures[specularGlossinessTexture.index].image.GetFixedMetallicRoughness());
						mat.EnableKeyword("_SPECGLOSSMAP");
					}
				}
				return mat;
			}
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#normaltextureinfo
		public class TextureInfo {
			[JsonProperty(Required = Required.Always)] public int index;
			public int texCoord = 0;
			public float scale = 1;
		}
	}

	public static class GLTFMaterialExtensions {
		public static GLTFMaterial.ImportResult Import(this List<GLTFMaterial> materials, GLTFTexture.ImportResult[] textures, ImportSettings importSettings) {
			if (!importSettings.materials) return null;

			GLTFMaterial.ImportResult result = new GLTFMaterial.ImportResult();
			result.materials = new Material[materials.Count];
			for (int i = 0; i < materials.Count; i++) {
				result.materials[i] = materials[i].CreateMaterial(textures, importSettings.shaders);
				if (materials[i].name == null) materials[i].name = "material" + i;
			}
			return result;
		}
	}
}