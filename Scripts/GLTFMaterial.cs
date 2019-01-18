using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFMaterial {
		public string name;
		public TextureReference normalTexture;
		public PbrMetalRoughness pbrMetallicRoughness;

		private Material cache;

		[Serializable]
		public class PbrMetalRoughness {
			public float[] baseColorFactor;
			public float metallicFactor;
			public float roughnessFactor;
			public TextureReference baseColorTexture;
			public TextureReference metallicRoughnessTexture;

			public Color BaseColor { get { return (baseColorFactor != null && baseColorFactor.Length == 3) ? new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]) : Color.white; } }

			public Material CreateMaterial(List<GLTFImage> images) {
				Material mat = new Material(Shader.Find("Standard"));
				mat.color = BaseColor;
				mat.SetFloat("_Metallic", metallicFactor);
				mat.SetFloat("_Glossiness", 1 - roughnessFactor);
				if (baseColorTexture != null) mat.SetTexture("_MainTex", images[baseColorTexture.index].GetTexture());
				if (metallicRoughnessTexture != null) mat.SetTexture("_MetallicGlossMap", images[metallicRoughnessTexture.index].GetTexture());
				return mat;
			}
		}

		[Serializable]
		public class TextureReference {
			public int index;
		}

		public void Initialize(List<GLTFImage> images) {
			if (pbrMetallicRoughness != null) cache = pbrMetallicRoughness.CreateMaterial(images);
			else cache = new Material(Shader.Find("Standard"));
			if (normalTexture != null) {
				Texture2D tex = images[normalTexture.index].GetTexture();
				cache.SetTexture("_BumpMap", tex);
			}
			cache.name = name;
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