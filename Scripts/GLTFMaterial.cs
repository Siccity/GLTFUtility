using System;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#material
	public class GLTFMaterial : GLTFProperty {

#region Serialized fields
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
#endregion

#region Non-serialized fields
		[JsonIgnore] private Material cache = null;
#endregion

		public Material CreateMaterial() {
			Material mat;
			// Load metallic-roughness materials
			if (pbrMetallicRoughness != null) {
				mat = pbrMetallicRoughness.CreateMaterial(glTFObject);
			}
			// Load specular-glossiness materials
			else if (extensions != null && extensions.KHR_materials_pbrSpecularGlossiness != null) {
				mat = extensions.KHR_materials_pbrSpecularGlossiness.CreateMaterial(glTFObject);
			}
			// Load fallback material
			else mat = new Material(Shader.Find("Standard"));

			if (normalTexture != null) {
				if (glTFObject.textures.Count <= normalTexture.index) {
					Debug.LogWarning("Attempted to get normal texture index " + normalTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[normalTexture.index].Source.GetNormalMap();
					mat.SetTexture("_BumpMap", tex);
					mat.EnableKeyword("_NORMALMAP");
				}
			}
			if (occlusionTexture != null) {
				if (glTFObject.textures.Count <= occlusionTexture.index) {
					Debug.LogWarning("Attempted to get occlusion texture index " + occlusionTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[occlusionTexture.index].Source.GetTexture();
					mat.SetTexture("_OcclusionMap", tex);
				}
			}
			if (emissiveFactor != Color.black) {
				mat.SetColor("_EmissionColor", emissiveFactor);
				mat.EnableKeyword("_EMISSION");
			}
			if (emissiveTexture != null && emissiveTexture.index >= 0) {
				if (glTFObject.textures.Count <= emissiveTexture.index) {
					Debug.LogWarning("Attempted to get emissive texture index " + emissiveTexture.index + " when only " + glTFObject.textures.Count + " exist");
				} else {
					Texture2D tex = glTFObject.textures[emissiveTexture.index].Source.GetTexture();
					mat.SetTexture("_EmissionMap", tex);
					mat.EnableKeyword("_EMISSION");
				}
			}
			// Name
			if (string.IsNullOrEmpty(name)) mat.name = "material" + glTFObject.materials.IndexOf(this);
			mat.name = name;

			return mat;
		}

		protected override bool OnLoad() {
			cache = CreateMaterial();
			return true;
		}

		public class MaterialExtensions {
			public PbrSpecularGlossiness KHR_materials_pbrSpecularGlossiness = null;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#pbrmetallicroughness
		public class PbrMetalRoughness {

#region Serialized fields
			[JsonConverter(typeof(ColorRGBAConverter))] public Color baseColorFactor = Color.white;
			public TextureInfo baseColorTexture;
			public float metallicFactor = 1f;
			public float roughnessFactor = 1f;
			public TextureInfo metallicRoughnessTexture;
#endregion

			public Material CreateMaterial(GLTFObject glTFObject) {
				GLTFProperty.Load(glTFObject, baseColorTexture, metallicRoughnessTexture);

				Material mat;
				// Material
				Shader sh = null;
#if UNITY_2019_1_OR_NEWER
				// LWRP support
				if (GraphicsSettings.renderPipelineAsset) sh = GraphicsSettings.renderPipelineAsset.defaultShader;
#endif
				if (sh == null) sh = Shader.Find("GLTFUtility/Standard (Metallic)");

				mat = new Material(sh);
				mat.color = baseColorFactor;
				mat.SetFloat("_Metallic", metallicFactor);
				mat.SetFloat("_Glossiness", 1 - roughnessFactor);
				if (baseColorTexture != null && baseColorTexture.index >= 0) {
					if (glTFObject.textures.Count <= baseColorTexture.index) {
						Debug.LogWarning("Attempted to get basecolor texture index " + baseColorTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						mat.SetTexture("_MainTex", glTFObject.textures[baseColorTexture.index].Source.GetTexture());
					}
				}
				if (metallicRoughnessTexture != null && metallicRoughnessTexture.index >= 0) {
					if (glTFObject.textures.Count <= metallicRoughnessTexture.index) {
						Debug.LogWarning("Attempted to get metallicRoughness texture index " + metallicRoughnessTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						mat.SetTexture("_MetallicGlossMap", glTFObject.textures[metallicRoughnessTexture.index].Source.GetFixedMetallicRoughness());
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

#region Serialized fields
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
#endregion

			public Material CreateMaterial(GLTFObject glTFObject) {
				GLTFProperty.Load(glTFObject, diffuseTexture, specularGlossinessTexture);

				Material mat;
				// Material base values
				mat = new Material(Shader.Find("GLTFUtility/Standard (Specular)"));
				mat.color = diffuseFactor;
				mat.SetColor("_SpecColor", specularFactor);
				mat.SetFloat("_Glossiness", glossinessFactor);

				// Diffuse texture
				if (diffuseTexture != null && diffuseTexture.index >= 0) {
					if (glTFObject.textures.Count <= diffuseTexture.index) {
						Debug.LogWarning("Attempted to get diffuseTexture texture index " + diffuseTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						mat.SetTexture("_MainTex", glTFObject.textures[diffuseTexture.index].Source.GetTexture());
					}
				}
				// Specular texture
				if (specularGlossinessTexture != null && specularGlossinessTexture.index >= 0) {
					if (glTFObject.textures.Count <= specularGlossinessTexture.index) {
						Debug.LogWarning("Attempted to get specularGlossinessTexture texture index " + specularGlossinessTexture.index + " when only " + glTFObject.textures.Count + " exist");
					} else {
						mat.SetTexture("_SpecGlossMap", glTFObject.textures[specularGlossinessTexture.index].Source.GetFixedMetallicRoughness());
						mat.EnableKeyword("_SPECGLOSSMAP");
					}
				}
				return mat;
			}
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#normaltextureinfo
		public class TextureInfo : GLTFProperty {

#region Serialized fields
			[JsonProperty(Required = Required.Always)] public int index;
			public int texCoord = 0;
			public float scale = 1;
#endregion

#region Non-serialized fields
			[JsonIgnore] public GLTFTexture Reference { get; private set; }
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