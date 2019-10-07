using System.Collections.Generic;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using static Siccity.GLTFUtility.GLTFMaterial;

namespace Siccity.GLTFUtility {
	[Extension("KHR_materials_pbrSpecularGlossiness")]
	public class KHRSpecularGlossiness : IExtension {
		private List<GLTFMaterial> materials;
		private GLTFTexture.ImportTask textureTask;
		private ImportSettings importSettings;

		public void TaskedWork(params object[] parms) {
			materials = parms[0] as List<GLTFMaterial>;
			textureTask = parms[1] as GLTFTexture.ImportTask;
			importSettings = parms[2] as ImportSettings;
		}

		public void MainThreadWork<T>(ref T Result) where T : class {
			GLTFMaterial.ImportResult result = Result as GLTFMaterial.ImportResult;
			result.material = materials[i].CreateMaterial(textureTask.Result, importSettings.shaders);
				if (Result[i].material.name == null) Result[i].material.name = "material" + i;
				foreach (var kvp in materials[i].extensions) {
					kvp.Value.MainThreadWork(Result);
			}
		}

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

				// Assign textures
				if (textures != null) {
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
							mat.SetTexture("_SpecGlossMap", textures[specularGlossinessTexture.index].image.texture);
							mat.EnableKeyword("_SPECGLOSSMAP");
						}
					}
				}
				return mat;
			}
		}
	}
}