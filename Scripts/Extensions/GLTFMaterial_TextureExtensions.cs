using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	public interface IGLTFMaterial_TextureExtension {
		void Apply(GLTFMaterial.TextureInfo texInfo, Material material, string textureSamplerName);
	}

	[Preserve] public class GLTFMaterial_TextureExtensions {
		public KHR_texture_transform KHR_texture_transform;

		public void Apply(GLTFMaterial.TextureInfo texInfo, Material material, string textureSamplerName) {
			// TODO: check if GLTFObject has extensionUsed/extensionRequired for these extensions

			if (KHR_texture_transform != null)
				KHR_texture_transform.Apply(texInfo, material, textureSamplerName);
		}
	}
}