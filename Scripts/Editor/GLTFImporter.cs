using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : GLTFImporterBase {

        public override void OnImportAsset(AssetImportContext ctx) {
            // Load asset
            GLTFObject gltfObject = new GLTFObject(ctx.assetPath);

            // Create gameobject structure
            GameObject[] roots = gltfObject.Create();

            ApplyDefaultMaterial(roots);
            SaveToAsset(ctx, roots);
            AddMeshes(ctx, gltfObject);
            AddMaterials(ctx, gltfObject);
            AddTextures(ctx, gltfObject);
            AddAnimations(ctx, gltfObject);
        }
    }
}