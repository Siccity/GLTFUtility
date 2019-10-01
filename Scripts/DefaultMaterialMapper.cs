using UnityEngine;

namespace Siccity.GLTFUtility
{
	public class DefaultMaterialMapper : IMaterialMapper
	{
		public void Apply(Material material, GLTFMaterial gltfMaterial, GLTFTexture.ImportResult[] textures)
		{
			Texture2D tex;
			if (GLTFMaterial.TryGetTexture(textures, gltfMaterial.normalTexture, out tex, x => x.GetNormalMap()))
			{
				material.SetTexture("_BumpMap", tex);
				material.EnableKeyword("_NORMALMAP");
			}

			if (GLTFMaterial.TryGetTexture(textures, gltfMaterial.occlusionTexture, out tex))
			{
				material.SetTexture("_OcclusionMap", tex);
			}

			if (gltfMaterial.emissiveFactor != Color.black)
			{
				material.SetColor("_EmissionColor", gltfMaterial.emissiveFactor);
				material.EnableKeyword("_EMISSION");
			}

			if (GLTFMaterial.TryGetTexture(textures, gltfMaterial.emissiveTexture, out tex))
			{
				material.SetTexture("_EmissionMap", tex);
				material.EnableKeyword("_EMISSION");
			}

			if (gltfMaterial.alphaMode == AlphaMode.MASK)
			{
				material.SetFloat("_AlphaCutoff", gltfMaterial.alphaCutoff);
			}
		}
	}
}