using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            // Load asset
            GameObject root = Importer.ImportGLTF(ctx.assetPath);
            // Save asset
            GLTFAssetUtility.SaveToAsset(root, ctx);
        }
    }
}