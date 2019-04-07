using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFMaterial : GLTFProperty {

#region Serialized fields
		[SerializeField] private string name;
		public TextureReference occlusionTexture = null;
		public TextureReference normalTexture = null;
		public PbrMetalRoughness pbrMetallicRoughness = null;
		public TextureReference emissiveTexture = null;
		public float[] emissiveFactor = null;
		public MaterialExtensions extensions = null;
#endregion

#region Non-serialized fields
		private Material cache = null;
		public Color EmissiveFactor { get; private set; }
#endregion

		protected override bool OnLoad() {
			// Load metallic-roughness materials
			if (pbrMetallicRoughness != null && pbrMetallicRoughness.Load(glTFObject)) {
				cache = pbrMetallicRoughness.Material;
			}
			// Load specular-glossiness materials
			else if (extensions != null && extensions.KHR_materials_pbrSpecularGlossiness != null && extensions.KHR_materials_pbrSpecularGlossiness.Load(glTFObject)) {
				cache = extensions.KHR_materials_pbrSpecularGlossiness.Material;
			}
			// Load fallback material
			else cache = new Material(Shader.Find("Standard"));

			// EmissiveFactor
			if (emissiveFactor != null && emissiveFactor.Length == 3) EmissiveFactor = new Color(emissiveFactor[0], emissiveFactor[1], emissiveFactor[2]);

			if (normalTexture != null && normalTexture.index >= 0) {
				if (glTFObject.textures.Count <= normalTexture.index) {
					Debug.LogWarning("Attempted to get normal texture index " + normalTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[normalTexture.index].Source.GetNormalMap();
					cache.SetTexture("_BumpMap", tex);
					cache.EnableKeyword("_NORMALMAP");
				}
			}
			if (occlusionTexture != null && occlusionTexture.index >= 0) {
				if (glTFObject.textures.Count <= occlusionTexture.index) {
					Debug.LogWarning("Attempted to get occlusion texture index " + occlusionTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[occlusionTexture.index].Source.GetTexture();
					cache.SetTexture("_OcclusionMap", tex);
				}
			}
			if (emissiveFactor != null && emissiveFactor.Length == 3) {
				cache.SetColor("_EmissionColor", EmissiveFactor);
				cache.EnableKeyword("_EMISSION");
			}
			if (emissiveTexture != null && emissiveTexture.index >= 0) {
				if (glTFObject.textures.Count <= emissiveTexture.index) {
					Debug.LogWarning("Attempted to get emissive texture index " + emissiveTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[emissiveTexture.index].Source.GetTexture();
					cache.SetTexture("_EmissionMap", tex);
					cache.EnableKeyword("_EMISSION");
				}
			}
			// Name
			if (string.IsNullOrEmpty(name)) cache.name = "material" + glTFObject.materials.IndexOf(this);
			else cache.name = name;
			return true;
		}

		[Serializable]
		public class MaterialExtensions {
			public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness = null;
		}

		[Serializable]
		public class PbrMetalRoughness : GLTFProperty {

#region Serialized fields
			public float[] baseColorFactor;
			public float metallicFactor = 1f;
			public float roughnessFactor = 1f;
			public TextureReference baseColorTexture;
			public TextureReference metallicRoughnessTexture;
#endregion

#region Non-serialized fields
			public Material Material { get; private set; }
#endregion

			protected override bool OnLoad() {
				if (!IsValid()) return false;

				GLTFProperty.Load(glTFObject, baseColorTexture, metallicRoughnessTexture);

				// Base Color
				Color baseColor;
				if (baseColorFactor != null && baseColorFactor.Length == 3) baseColor = new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]);
				if (baseColorFactor != null && baseColorFactor.Length == 4) baseColor = new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2], baseColorFactor[3]);
				else baseColor = Color.white;

				// Material
				Material = new Material(Shader.Find("GLTFUtility/Standard (Metallic)"));
				Material.color = baseColor;
				Material.SetFloat("_Metallic", metallicFactor);
				Material.SetFloat("_Glossiness", 1 - roughnessFactor);
				if (baseColorTexture != null && baseColorTexture.index >= 0) {
					if (glTFObject.textures.Count <= baseColorTexture.index) {
						Debug.LogWarning("Attempted to get basecolor texture index " + baseColorTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						Material.SetTexture("_MainTex", glTFObject.textures[baseColorTexture.index].Source.GetTexture());
					}
				}
				if (metallicRoughnessTexture != null && metallicRoughnessTexture.index >= 0) {
					if (glTFObject.textures.Count <= metallicRoughnessTexture.index) {
						Debug.LogWarning("Attempted to get metallicRoughness texture index " + metallicRoughnessTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						Material.SetTexture("_MetallicGlossMap", glTFObject.textures[metallicRoughnessTexture.index].Source.GetFixedMetallicRoughness());
						Material.EnableKeyword("_METALLICGLOSSMAP");
					}
				}
				return true;
			}

			/// <summary> JSONUtility sometimes sets nulls to new empty classes instead of null. Check if any values are set </summary>
			public bool IsValid() {
				if (baseColorFactor != null && baseColorFactor.Length != 0) return true;
				else if (metallicFactor != 1f) return true;
				else if (roughnessFactor != 1f) return true;
				else if (baseColorTexture != null) return true;
				else if (metallicRoughnessTexture != null) return true;
				else return false;
			}
		}

		[Serializable]
		public class PbrSpecularGlossiness : GLTFProperty {

#region Serialized fields
			/// <summary> The reflected diffuse factor of the material </summary>
			public float[] diffuseFactor;
			/// <summary> The diffuse texture </summary>
			public TextureReference diffuseTexture;
			/// <summary> The reflected diffuse factor of the material </summary>
			public float[] specularFactor;
			/// <summary> The glossiness or smoothness of the material </summary>
			public float glossinessFactor = 1f;
			/// <summary> The specular-glossiness texture </summary>
			public TextureReference specularGlossinessTexture;
#endregion

#region Non-serialized fields
			public Material Material { get; private set; }
#endregion

			protected override bool OnLoad() {
				if (!IsValid()) return false;

				GLTFProperty.Load(glTFObject, diffuseTexture, specularGlossinessTexture);

				// Base color
				Color baseColor;
				if (diffuseFactor != null && diffuseFactor.Length == 3) baseColor = new Color(diffuseFactor[0], diffuseFactor[1], diffuseFactor[2]);
				if (diffuseFactor != null && diffuseFactor.Length == 4) baseColor = new Color(diffuseFactor[0], diffuseFactor[1], diffuseFactor[2], diffuseFactor[3]);
				else baseColor = Color.white;

				// Specular color
				Color specularColor;
				if (specularFactor != null && specularFactor.Length == 3) specularColor = new Color(specularFactor[0], specularFactor[1], specularFactor[2]);
				else specularColor = Color.white;

				// Material base values
				Material = new Material(Shader.Find("GLTFUtility/Standard (Specular)"));
				Material.color = baseColor;
				Material.SetColor("_SpecColor", specularColor);
				Material.SetFloat("_Glossiness", glossinessFactor);

				// Diffuse texture
				if (diffuseTexture != null && diffuseTexture.index >= 0) {
					if (glTFObject.textures.Count <= diffuseTexture.index) {
						Debug.LogWarning("Attempted to get diffuseTexture texture index " + diffuseTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						Material.SetTexture("_MainTex", glTFObject.textures[diffuseTexture.index].Source.GetTexture());
					}
				}
				// Specular texture
				if (specularGlossinessTexture != null && specularGlossinessTexture.index >= 0) {
					if (glTFObject.textures.Count <= specularGlossinessTexture.index) {
						Debug.LogWarning("Attempted to get specularGlossinessTexture texture index " + specularGlossinessTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						Material.SetTexture("_SpecGlossMap", glTFObject.textures[specularGlossinessTexture.index].Source.GetFixedMetallicRoughness());
						Material.EnableKeyword("_SPECGLOSSMAP");
					}
				}
				return true;
			}

			/// <summary> JSONUtility sometimes sets nulls to new empty classes instead of null. Check if any values are set </summary>
			public bool IsValid() {
				if (diffuseFactor != null && diffuseFactor.Length != 0) return true;
				else if (diffuseTexture != null) return true;
				else if (specularFactor != null && specularFactor.Length != 0) return true;
				else if (glossinessFactor != 1f) return true;
				else if (specularGlossinessTexture != null) return true;
				else return false;
			}
		}

		[Serializable]
		public class TextureReference : GLTFProperty {

#region Serialized fields
			public int index = -1;
#endregion

#region Non-serialized fields
			public GLTFTexture Reference { get; private set; }
#endregion

			protected override bool OnLoad() {
				if (index >= 0) {
					if (glTFObject.textures.Count <= index) {
						Debug.LogWarning("Attempted to get texture index " + index + " when only " + glTFObject.textures.Count + " exist");
						return false;
					} else {
						Reference = glTFObject.textures[index];
					}
				}
				return true;
			}
		}

		public Material GetMaterial() {
			if (cache != null) return cache;
			else {
				Debug.LogWarning("No material cached. Please initialize first");
				return null;
			}
		}
	}
}