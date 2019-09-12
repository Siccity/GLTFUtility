using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#node
    public class GLTFNode {

        public string name;
        /// <summary> Indices of child nodes </summary>
        public List<int> children;
        /// <summary> Local TRS </summary>
        [JsonProperty, JsonConverter(typeof(Matrix4x4Converter))] private Matrix4x4 matrix { set { value.UnpackTRS(ref translation, ref rotation, ref scale); } }
        /// <summary> Local position </summary>
        [JsonConverter(typeof(TranslationConverter))] public Vector3 translation = Vector3.zero;
        /// <summary> Local rotation </summary>
        [JsonConverter(typeof(QuaternionConverter))] public Quaternion rotation = Quaternion.identity;
        /// <summary> Local scale </summary>
        [JsonConverter(typeof(Vector3Converter))] public Vector3 scale = Vector3.one;
        public int? mesh;
        public int? skin;
        public int? camera;
        public int? weights;

        public bool ShouldSerializetranslation() { return translation != Vector3.zero; }
        public bool ShouldSerializerotation() { return rotation != Quaternion.identity; }
        public bool ShouldSerializescale() { return scale != Vector3.one; }

        public class ImportResult {
            public int? parent;
            public int[] children;
            public Transform transform;

            public bool IsRoot { get { return !parent.HasValue; } }
        }

        /// <summary> Set local position, rotation and scale </summary>
        public void ApplyTRS(Transform transform) {
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        public GLTFNode(Transform transform) {
            name = transform.name;
            translation = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
        }

#region Export
        public static List<GLTFNode> CreateNodeList(Transform root) {
            List<GLTFNode> nodes = new List<GLTFNode>();
            CreateNodeListRecursive(root, nodes);
            return nodes;
        }

        private static void CreateNodeListRecursive(Transform transform, List<GLTFNode> nodes) {
            GLTFNode node = new GLTFNode(transform);
            nodes.Add(node);
            if (transform.childCount > 0) {
                node.children = new List<int>();
                foreach (Transform child in transform) {
                    node.children.Add(nodes.Count);
                    CreateNodeListRecursive(child, nodes);
                }
            }
        }
#endregion

        /// <summary> Set up various components defined in the node. Call after all transforms have been set up </summary>
        /* public void SetupComponents() {
            if (Transform == null) {
                Debug.LogWarning("Transform is null. Call CreateTransform before calling SetupComponents");
                return;
            }
            if (this.mesh.HasValue) {
                GLTFMesh glTFMesh = glTFObject.meshes[this.mesh.Value];
                Mesh mesh = glTFMesh.GetMesh();
                Renderer renderer;
                if (Skin != null) {
                    renderer = Skin.SetupSkinnedRenderer(Transform.gameObject, mesh);
                } else {
                    MeshRenderer mr = Transform.gameObject.AddComponent<MeshRenderer>();
                    MeshFilter mf = Transform.gameObject.AddComponent<MeshFilter>();
                    renderer = mr;
                    mf.sharedMesh = mesh;
                }

                //Materials
                Material[] materials = new Material[glTFMesh.primitives.Count];
                for (int i = 0; i < glTFMesh.primitives.Count; i++) {
                    GLTFPrimitive primitive = glTFMesh.primitives[i];
                    // Create material if id is positive or 0
                    if (primitive.material.HasValue) materials[i] = glTFObject.materials[primitive.material.Value].GetMaterial();
                }
                renderer.materials = materials;
            }
        } */
    }

    public static class GLTFNodeExtensions {
#region Import
        public static GLTFNode.ImportResult[] Import(this List<GLTFNode> nodes) {
            GLTFNode.ImportResult[] results = new GLTFNode.ImportResult[nodes.Count];
            // Initialize transforms
            for (int i = 0; i < results.Length; i++) {
                results[i].transform = new GameObject().transform;
                results[i].transform.name = nodes[i].name;
            }
            // Set up hierarchy
            for (int i = 0; i < results.Length; i++) {
                results[i].children = nodes[i].children.ToArray();
                for (int k = 0; k < nodes[i].children.Count; k++) {
                    results[k].parent = i;
                    results[k].transform.parent = results[i].transform;
                }
            }
            // Apply TRS
            for (int i = 0; i < results.Length; i++) {
                nodes[i].ApplyTRS(results[i].transform);
            }
            return results;
        }

        /// <summary> Returns the root if there is one, otherwise creates a new empty root </summary>
        public static GameObject GetRoot(this GLTFNode.ImportResult[] nodes) {
            GLTFNode.ImportResult[] roots = nodes.Where(x => x.IsRoot).ToArray();
            if (roots.Length == 1) return roots[0].transform.gameObject;
            else {
                GameObject root = new GameObject("Root");
                for (int i = 0; i < roots.Length; i++) {
                    roots[i].transform.parent = root.transform;
                }
                return root;
            }
        }
#endregion
    }
}