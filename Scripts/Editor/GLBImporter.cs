using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [ScriptedImporter(1, "glb")]
    public class GLBImporter : GLTFImporterBase {

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
            string chunkType = Encoding.ASCII.GetString(bytes.SubArray(16, 4));
            string json = Encoding.ASCII.GetString(bytes.SubArray(20, (int) chunkLength));

            // Load file and get directory
            GLTFObject gltfObject = JsonUtility.FromJson<GLTFObject>(json);
            string directoryRoot = Directory.GetParent(ctx.assetPath).ToString() + "/";
            string mainFile = Path.GetFileName(ctx.assetPath);
            gltfObject.Load(directoryRoot, mainFile);

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