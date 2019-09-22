using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[ScriptedImporter(1, "gltf")]
	public class GLTFImporter : ScriptedImporter {

		public ImportSettings importSettings;

		public override void OnImportAsset(AssetImportContext ctx) {
			// Load asset
			GLTFAnimation.ImportResult[] animations;
			GameObject root = Importer.LoadFromFile(ctx.assetPath, importSettings, out animations, Format.GLTF);
			// Save asset
			GLTFAssetUtility.SaveToAsset(root, animations, ctx);
		}
	}
}