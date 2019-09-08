using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {

    public class GLTFObject {

#region Serialized fields
        public int? scene;
        [JsonProperty(Required = Required.Always)] public GLTFAsset asset;
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
        [JsonIgnore] public bool loaded { get; set; }
        [JsonIgnore] public string directoryRoot { get; set; }
        [JsonIgnore] public string mainFile { get; set; }
#endregion

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
    }
}