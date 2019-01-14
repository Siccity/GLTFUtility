using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "glb")]
    public class GLBImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext ctx) {
            byte[] bytes = File.ReadAllBytes(ctx.assetPath);

            // 12 byte header
            // 0-4  - magic = "glTF"
            // 4-8  - version = 2
            // 8-12 - length = total length of glb, including Header and all Chunks, in bytes.
            string magic = Encoding.ASCII.GetString(bytes.SubArray(0, 4));
            if (magic != "glTF") return;
            uint version = System.BitConverter.ToUInt32(bytes, 4);
            if (version != 2) {
                Debug.LogWarning("Importer does not support gltf version " + version);
                return;
            }
            uint length = System.BitConverter.ToUInt32(bytes, 8);
            
            // Chunk 0 (json)
            uint chunkLength = System.BitConverter.ToUInt32(bytes, 12);
            Debug.Log(chunkLength);
            string chunkType = Encoding.ASCII.GetString(bytes.SubArray(16, 4));
            Debug.Log(chunkType);
            string json = Encoding.ASCII.GetString(bytes.SubArray(20, (int)chunkLength));
            Debug.Log(json);
            return;

            // Load file and get directory
            GLTFObject glbObject = JsonUtility.FromJson<GLTFObject>(File.ReadAllText(ctx.assetPath));
            string directoryRoot = Directory.GetParent(ctx.assetPath).ToString() + "/";

            // Create gameobject structure
            GameObject root = glbObject.Create(directoryRoot);

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
            for (int i = 0; i < glbObject.meshes.Count; i++) {
                ctx.AddSubAsset(glbObject.meshes[i].name, glbObject.meshes[i].GetCachedMesh());
            }
#endif
        }
    }
}