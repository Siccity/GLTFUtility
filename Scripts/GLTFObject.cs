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
        public List<GLTFAnimation> animations;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFSkin> skins;
        public List<GLTFImage> images;
        public List<GLTFMaterial> materials;

        public GameObject[] Create(string directoryRoot, string mainFile) {
            // Read buffers
            for (int i = 0; i < buffers.Count; i++) {
                buffers[i].Read(directoryRoot, mainFile);
            }

            // Load textures
            for (int i = 0; i < images.Count; i++) {
                images[i].Initialize(this, directoryRoot);
            }

            // Load materials
            for (int i = 0; i < materials.Count; i++) {
                materials[i].Initialize(images);
            }

            // Get root node indices from scenes
            int[] rootNodes = scenes.SelectMany(x => x.nodes).ToArray();

            GameObject[] roots = new GameObject[rootNodes.Length];
            for (int i = 0; i < rootNodes.Length; i++) {
                // Recursively construct transform hierarchy
                int nodeIndex = rootNodes[i];
                roots[i] = nodes[nodeIndex].CreateTransform(this, null).gameObject;
            }

            // Flip the entire node tree on the global Z axis
            var worldTransforms = nodes.Where(x => x.transform != null).Select(x => new {
                transform = x.transform, worldPos = x.transform.position, worldRot = x.transform.rotation,
            }).ToArray();
            for (int i = 0; i < worldTransforms.Length; i++) {
                var x = worldTransforms[i];

                // Reverse Z
                x.transform.position = new Vector3(x.worldPos.x, x.worldPos.y, -x.worldPos.z);
                x.transform.rotation = new Quaternion(-x.worldRot.x, -x.worldRot.y, x.worldRot.z, x.worldRot.w);
            }

            // Setup mesh renderers and such
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].SetupComponents(this);
            }
            return roots;
        }
    }
}