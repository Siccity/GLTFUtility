using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, UNSIGNED_INT = 5125, FLOAT = 5126 }

    [Serializable]
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
        public bool loaded { get; private set; }
        public string directoryRoot { get; private set; }
        public string mainFile { get; private set; }
#endregion

        /// <summary> Constructor </summary>
        public GLTFObject() { }

        /// <summary> Constructor </summary>
        /// <param name="filepath">Full path to a .gltf or .glb file</param>
        public GLTFObject(string filepath) {
            Load(filepath);
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

        public void Load(string filepath) {
            string extension = Path.GetExtension(filepath).ToLower();
            if (extension == ".glb") LoadGLB(filepath);
            else if (extension == ".gltf") LoadGLTF(filepath);
            else {
                Debug.Log("Extension '" + extension + "' not recognized in " + filepath);
                loaded = false;
                return;
            }
            loaded = true;
        }

        public void LoadGLB(string filepath) {
            byte[] bytes = File.ReadAllBytes(filepath);

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
            JsonUtility.FromJsonOverwrite(json, this);

            LoadInternal(filepath);
        }

        public void LoadGLTF(string filepath) {
            string json = File.ReadAllText(filepath);

            JsonUtility.FromJsonOverwrite(json, this);

            LoadInternal(filepath);
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
        }
    }
}