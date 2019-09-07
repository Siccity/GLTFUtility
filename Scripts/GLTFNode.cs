using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFNode : GLTFProperty {

#region Serialized fields
        public string name;
        /// <summary> Indices of child nodes </summary>
        public List<int> children;
        /// <summary> Local TRS </summary>
        [JsonConverter(typeof(Matrix4x4))] private Matrix4x4 matrix { set { value.UnpackTRS(ref translation, ref rotation, ref scale); } }
        /// <summary> Local position </summary>
        [JsonConverter(typeof(Vector3Converter))] public Vector3 translation = Vector3.zero;
        /// <summary> Local rotation </summary>
        [JsonConverter(typeof(QuaternionConverter))] public Quaternion rotation = Quaternion.identity;
        /// <summary> Local scale </summary>
        [JsonConverter(typeof(Vector3Converter))] public Vector3 scale = Vector3.one;
        public int? mesh;
        public int? skin;
        public int? camera;
#endregion

#region Non-serialized fields
        [JsonIgnore] public Transform Transform { get; private set; }
        [JsonIgnore] public GLTFSkin Skin { get; private set; }
#endregion

        protected override bool OnLoad() {
            return true;
        }

        /// <summary> Recursively set up this node's transform in the scene, followed by its children </summary>
        public Transform CreateTransform(Transform parent) {
            if (Transform == null) Transform = new GameObject().transform;
            Transform.parent = parent;
            Transform.gameObject.name = GetName();
            Transform.localPosition = translation;
            Transform.localRotation = rotation;
            Transform.localScale = scale;

            if (children != null) {
                for (int i = 0; i < children.Count; i++) {
                    glTFObject.nodes[children[i]].CreateTransform(Transform);
                }
            }

            return Transform;
        }

        /// <summary> Returns an automatic name if no name is set </summary>
        public string GetName() {
            if (name == null) {
                if (IsJoint()) return "joint" + glTFObject.nodes.IndexOf(this);
                else return "node" + glTFObject.nodes.IndexOf(this);
            } else return name;
        }

        /// <summary> Set up various components defined in the node. Call after all transforms have been set up </summary>
        public void SetupComponents() {
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
                    if (primitive.material != -1) materials[i] = glTFObject.materials[primitive.material].GetMaterial();
                }
                renderer.materials = materials;
            }
        }

        public GLTFNode GetParentNode() {
            int nodeIndex = glTFObject.nodes.IndexOf(this);
            for (int i = 0; i < glTFObject.nodes.Count; i++) {
                if (glTFObject.nodes[i].children.Contains(nodeIndex)) return glTFObject.nodes[i];
            }
            return null;
        }

        /// <summary>  Returns true if this node is referenced directly in a scene </summary>
        public bool IsRootNode() {
            int nodeIndex = glTFObject.nodes.IndexOf(this);
            for (int i = 0; i < glTFObject.scenes.Count; i++) {
                if (glTFObject.scenes[i].nodes.Contains(nodeIndex)) return true;
            }
            return false;
        }

        /// <summary> 
        /// Same as IsRootNode except returns false if the scene has more root nodes, 
        /// which means that it will create an extra transform object as root
        /// </summary>
        public bool IsRootTransform() {
            int nodeIndex = glTFObject.nodes.IndexOf(this);
            for (int i = 0; i < glTFObject.scenes.Count; i++) {
                if (glTFObject.scenes[i].nodes.Contains(nodeIndex) && glTFObject.scenes[i].nodes.Count == 1) return true;
            }
            return false;
        }

        public bool IsJoint() {
            if (glTFObject.skins == null || glTFObject.skins.Count == 0) return false;
            int nodeIndex = glTFObject.nodes.IndexOf(this);
            for (int i = 0; i < glTFObject.skins.Count; i++) {
                for (int k = 0; k < glTFObject.skins[i].joints.Length; k++) {
                    if (glTFObject.skins[i].joints[k] == nodeIndex) return true;
                }
            }
            return false;
        }
    }
}