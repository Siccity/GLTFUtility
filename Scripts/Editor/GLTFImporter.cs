using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : GLTFImporterBase {

        public override void OnImportAsset(AssetImportContext ctx) {
            // Load file and get directory
            GLTFObject gltfObject = JsonUtility.FromJson<GLTFObject>(File.ReadAllText(ctx.assetPath));
            string directoryRoot = Directory.GetParent(ctx.assetPath).ToString() + "/";
            string mainFile = Path.GetFileName(ctx.assetPath);

            // Create gameobject structure
            GameObject[] roots = gltfObject.Create(directoryRoot, mainFile);

            SaveToAsset(ctx, roots);
            AddMeshes(ctx, gltfObject);
            AddMaterials(ctx, gltfObject);
            AddTextures(ctx, gltfObject);
            AddAnimactionClips(ctx, gltfObject);
        }
    }
}