using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "gltf")]
    public class GLTFImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            GLTFObject gltfObject = JsonUtility.FromJson<GLTFObject>(File.ReadAllText(ctx.assetPath));
            string directoryRoot = Directory.GetParent(ctx.assetPath).ToString() + "/";

            // Read buffers
            for (int i = 0; i < gltfObject.buffers.Count; i++) {
                gltfObject.buffers[i].Read(directoryRoot);
            }

            // Get root node indices from scenes
            int[] rootNodes = gltfObject.scenes.SelectMany(x => x.nodes).ToArray();

            if (rootNodes.Length != 1) {
                Debug.LogError("Only one root node is currently supported");
                return;
            }

            // Add meshes
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                ctx.AddSubAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetMesh(gltfObject));
            }

            // Parse root node
            GameObject root = gltfObject.nodes[0].Create(gltfObject, null);
            ctx.SetMainAsset("main obj", root);
        }
    }
}