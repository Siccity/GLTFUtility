using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            // Load file and get directory
            GLTFObject gltfObject = JsonUtility.FromJson<GLTFObject>(File.ReadAllText(ctx.assetPath));
            string directoryRoot = Directory.GetParent(ctx.assetPath).ToString() + "/";

            // Create gameobject structure
            GameObject root = gltfObject.Create(directoryRoot);

            // Save to asset
#if UNITY_2018_2_OR_NEWER
            ctx.AddObjectToAsset("main", root);
            ctx.SetMainObject(root);
#else
            ctx.SetMainAsset("main obj", root);
#endif

            // Add meshes
#if UNITY_2018_2_OR_NEWER
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                ctx.AddObjectToAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetCachedMesh());
            }
#else
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                ctx.AddSubAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetCachedMesh());
            }
#endif
        }
    }
}