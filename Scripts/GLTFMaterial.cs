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
			// EmissiveFactor
			if (emissiveFactor != null && emissiveFactor.Length == 3) EmissiveFactor = new Color(emissiveFactor[0], emissiveFactor[1], emissiveFactor[2]);

			if (pbrMetallicRoughness != null) cache = pbrMetallicRoughness.CreateMaterial(glTFObject.images);
			else cache = new Material(Shader.Find("Standard"));
			if (normalTexture != null && normalTexture.index >= 0) {
				Texture2D tex = glTFObject.images[normalTexture.index].GetNormalMap();
				cache.SetTexture("_BumpMap", tex);
				cache.EnableKeyword("_NORMALMAP");
			}
			if (occlusionTexture != null && occlusionTexture.index >= 0) {
				Texture2D tex = glTFObject.images[occlusionTexture.index].GetTexture();
				cache.SetTexture("_OcclusionMap", tex);
			}
			if (emissiveFactor != null && emissiveFactor.Length == 3) {
				cache.SetColor("_EmissionColor", EmissiveFactor);
				cache.EnableKeyword("_EMISSION");
			}
			if (emissiveTexture != null && emissiveTexture.index >= 0) {
				Texture2D tex = glTFObject.images[emissiveTexture.index].GetTexture();
				cache.SetTexture("_EmissionMap", tex);
				cache.EnableKeyword("_EMISSION");
			}
			// Name
			if (string.IsNullOrEmpty(name)) cache.name = "material" + glTFObject.materials.IndexOf(this);
			else cache.name = name;
		}

		[Serializable]
		public class PbrMetalRoughness {
			public float[] baseColorFactor;
			public float metallicFactor;
			public float roughnessFactor = 1f;
			public TextureReference baseColorTexture;
			public TextureReference metallicRoughnessTexture;

			public Color BaseColor { get { return (baseColorFactor != null && baseColorFactor.Length == 3) ? new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]) : Color.white; } }

			public Material CreateMaterial(List<GLTFImage> images) {
				Material mat = new Material(Shader.Find("Standard"));
				mat.color = BaseColor;
				mat.SetFloat("_Metallic", metallicFactor);
				mat.SetFloat("_Glossiness", 1 - roughnessFactor);
				if (baseColorTexture != null && baseColorTexture.index >= 0) {
					mat.SetTexture("_MainTex", images[baseColorTexture.index].GetTexture());
				}
				if (metallicRoughnessTexture != null && metallicRoughnessTexture.index >= 0) {
					mat.SetTexture("_MetallicGlossMap", images[metallicRoughnessTexture.index].GetFixedMetallicRoughness());
					mat.EnableKeyword("_METALLICGLOSSMAP");
				}
				return mat;
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