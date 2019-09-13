using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[ScriptedImporter(1, "glb")]
	public class GLBImporter : GLTFImporter {

		public override void OnImportAsset(AssetImportContext ctx) {
			// Load asset
			GLTFAnimation.ImportResult[] animations;
			GameObject root = Importer.ImportGLB(ctx.assetPath, shaderSettings, out animations);
			// Save asset
			GLTFAssetUtility.SaveToAsset(root, animations, ctx);
		}
	}
}