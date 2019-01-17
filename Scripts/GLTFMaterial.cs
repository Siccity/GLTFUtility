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
			public string metallicFactor;
			public string roughnessFactor;
			public TextureReference baseColorTexture;
			public TextureReference metallicRoughnessTexture;

			public Color BaseColor { get { return new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]); } }
		}

		[Serializable]
		public class TextureReference {
			public int index;
		}

		public Material GetMaterial() {
			return new Material(Shader.Find("Standard"));

		}
	}
}