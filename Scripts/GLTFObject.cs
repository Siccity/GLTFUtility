using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, UNSIGNED_INT = 5125, FLOAT = 5126 }

    public class GLTFObject {

#region Serialized fields
        public int scene = -1;
        public List<GLTFScene> scenes;
        public List<GLTFNode> nodes;
        public List<GLTFMesh> meshes;
        public List<GLTFAnimation> animations;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFSkin> skins;
        public List<GLTFTexture> textures;
        public List<GLTFImage> images;
        public List<GLTFMaterial> materials;
#endregion

#region Non-serialized fields
        [JsonIgnore] public bool loaded { get; private set; }
        [JsonIgnore] public string directoryRoot { get; private set; }
        [JsonIgnore] public string mainFile { get; private set; }
#endregion

        public static GLTFObject LoadFromFile(string filepath) {
            string extension = Path.GetExtension(filepath).ToLower();
            if (extension == ".glb") return LoadGLB(filepath);
            else if (extension == ".gltf") return LoadGLTF(filepath);
            else {
                Debug.Log("Extension '" + extension + "' not recognized in " + filepath);
                return null;
            }

        }

        public GameObject[] Create() {

            // Get root node indices from scenes
            int[] rootNodes = scenes.SelectMany(x => x.nodes).ToArray();

            GameObject[] roots = new GameObject[rootNodes.Length];
            for (int i = 0; i < rootNodes.Length; i++) {
                // Recursively construct transform hierarchy
                int nodeIndex = rootNodes[i];
                roots[i] = nodes[nodeIndex].CreateTransform(null).gameObject;
            }

            // Setup mesh renderers and such
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].SetupComponents();
            }
            return roots;
        }

        public static GLTFObject LoadGLB(string filepath) {
            byte[] bytes = File.ReadAllBytes(filepath);

            // 12 byte header
            // 0-4  - magic = "glTF"
            // 4-8  - version = 2
            // 8-12 - length = total length of glb, including Header and all Chunks, in bytes.
            string magic = Encoding.ASCII.GetString(bytes.SubArray(0, 4));
            if (magic != "glTF") {
                Debug.LogWarning("File at " + filepath + " does not look like a .glb file");
                return null;
            }
            uint version = System.BitConverter.ToUInt32(bytes, 4);
            if (version != 2) {
                Debug.LogWarning("Importer does not support gltf version " + version);
                return null;
            }
            uint length = System.BitConverter.ToUInt32(bytes, 8);

            // Chunk 0 (json)
            uint chunkLength = System.BitConverter.ToUInt32(bytes, 12);
            string chunkType = Encoding.ASCII.GetString(bytes.SubArray(16, 4));
            string json = Encoding.ASCII.GetString(bytes.SubArray(20, (int) chunkLength));

            // Parse json
            GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
            gltfObject.LoadInternal(filepath);
            return gltfObject;
        }

        public static GLTFObject LoadGLTF(string filepath) {
            string json = File.ReadAllText(filepath);

            // Parse json
            GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
            gltfObject.LoadInternal(filepath);
            return gltfObject;
        }

        private void LoadInternal(string filepath) {
            this.directoryRoot = Directory.GetParent(filepath).ToString() + "/";
            this.mainFile = Path.GetFileName(filepath);
            GLTFProperty.Load(this, buffers);
            GLTFProperty.Load(this, bufferViews);
            GLTFProperty.Load(this, accessors);
            GLTFProperty.Load(this, images);
            GLTFProperty.Load(this, textures);
            GLTFProperty.Load(this, materials);
            GLTFProperty.Load(this, scenes);
            GLTFProperty.Load(this, nodes);
            GLTFProperty.Load(this, meshes);
            GLTFProperty.Load(this, animations);
            GLTFProperty.Load(this, skins);
            loaded = true;
        }
    }
}