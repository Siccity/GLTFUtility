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
#endregion

#region Non-serialized fields
		private Material cache = null;
		public Color EmissiveFactor { get; private set; }
#endregion

		public override void Load() {
			if (pbrMetallicRoughness != null) {
				pbrMetallicRoughness.glTFObject = glTFObject;
				pbrMetallicRoughness.Load();
				cache = pbrMetallicRoughness.Material;
			}
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
		}

		[Serializable]
		public class PbrMetalRoughness : GLTFProperty {

#region Serialized fields
			[SerializeField] private float[] baseColorFactor;
			public float metallicFactor;
			public float roughnessFactor = 1f;
			public TextureReference baseColorTexture;
			public TextureReference metallicRoughnessTexture;
#endregion

#region Non-serialized fields
			public Color BaseColor { get; private set; }
			public Material Material { get; private set; }
#endregion

			public override void Load() {
				// Base Color
				if (baseColorFactor != null && baseColorFactor.Length == 3) BaseColor = new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]);
				if (baseColorFactor != null && baseColorFactor.Length == 4) BaseColor = new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2], baseColorFactor[3]);
				else BaseColor = Color.white;

				// Material
				Material = new Material(Shader.Find("Standard"));
				Material.color = BaseColor;
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
			}
		}

		[Serializable]
		public class TextureReference {
			public int index = -1;
		}

		public void Initialize(List<GLTFImage> images) {

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