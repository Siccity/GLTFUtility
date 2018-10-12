using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, FLOAT = 5126 }

    [Serializable]
    public class GLTFObject {

        /// <summary> Default scene </summary>
        int scene = -1;
        public List<GLTFScene> scenes;
        public List<GLTFNode> nodes;
        public List<GLTFMesh> meshes;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFSkin> skins;

        public GameObject Create(string directoryRoot) {
            // Read buffers
            for (int i = 0; i < buffers.Count; i++) {
                buffers[i].Read(directoryRoot);
            }

            // Get root node indices from scenes
            int[] rootNodes = scenes.SelectMany(x => x.nodes).ToArray();

            if (rootNodes.Length != 1) {
                Debug.LogError("Only one root node is currently supported");
                return null;
            }

            // Recursively construct transform hierarchy
            Transform root = nodes[0].CreateTransform(this, null);

            // Setup mesh renderers and such
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].SetupComponents(this);
            }
            return root.gameObject;
        }
    }
}