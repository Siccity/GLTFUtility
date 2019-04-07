using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFNode : GLTFProperty {

#region Serialized fields
        [SerializeField] private string name;
        /// <summary> Indices of child nodes </summary>
        public List<int> children;
        /// <summary> Local TRS </summary>
        public float[] matrix;
        /// <summary> Local position </summary>
        [SerializeField] private float[] translation;
        /// <summary> Local rotation </summary>
        [SerializeField] private float[] rotation;
        /// <summary> Local scale </summary>
        [SerializeField] private float[] scale;
        public int mesh = -1;
        public int skin = -1;
        public int camera = -1;
#endregion

#region Non-serialized fields
        public Vector3 LocalPosition { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector3 LocalScale { get; private set; }
        public string Name { get; private set; }
        public Transform Transform { get; private set; }
        public GLTFSkin Skin { get; private set; }
#endregion

        protected override bool OnLoad() {
            // Name
            if (string.IsNullOrEmpty(name)) {
                if (IsJoint()) Name = "joint" + glTFObject.nodes.IndexOf(this);
                else Name = "node" + glTFObject.nodes.IndexOf(this);
            } else Name = name;
            // References
            if (skin != -1) Skin = glTFObject.skins[skin];
            // Transform
            if (matrix != null) {
                Matrix4x4 trs = new Matrix4x4(
                    new Vector4(matrix[0], matrix[1], matrix[2], matrix[3]),
                    new Vector4(matrix[4], matrix[5], matrix[6], matrix[7]),
                    new Vector4(matrix[8], matrix[9], matrix[10], matrix[11]),
                    new Vector4(matrix[12], matrix[13], matrix[14], matrix[15])
                );
                Vector3 pos = trs.GetColumn(3);
                pos.z = -pos.z;
                Quaternion rot = trs.rotation;
                rot = new Quaternion(rot.x, rot.y, -rot.z, -rot.w);
                LocalPosition = pos;
                LocalRotation = rot;
                LocalScale = trs.lossyScale;
            } else {
                if (translation != null) LocalPosition = new Vector3(translation[0], translation[1], -translation[2]);
                else LocalPosition = Vector3.zero;
                if (rotation != null) LocalRotation = new Quaternion(rotation[0], rotation[1], -rotation[2], -rotation[3]);
                else LocalRotation = new Quaternion(0, 0, 0, 1);
                if (scale != null) LocalScale = new Vector3(scale[0], scale[1], scale[2]);
                else LocalScale = Vector3.one;
            }
            return true;
        }

        /// <summary> Recursively set up this node's transform in the scene, followed by its children </summary>
        public Transform CreateTransform(Transform parent) {
            if (Transform == null) Transform = new GameObject().transform;
            Transform.parent = parent;
            Transform.gameObject.name = Name;
            Transform.localPosition = LocalPosition;
            Transform.localRotation = LocalRotation;
            Transform.localScale = LocalScale;

            for (int i = 0; i < children.Count; i++) {
                glTFObject.nodes[children[i]].CreateTransform(Transform);
            }

            return Transform;
        }

        /// <summary> Set up various components defined in the node. Call after all transforms have been set up </summary>
        public void SetupComponents() {
            if (Transform == null) {
                Debug.LogWarning("Transform is null. Call CreateTransform before calling SetupComponents");
                return;
            }
            if (this.mesh != -1) {
                GLTFMesh glTFMesh = glTFObject.meshes[this.mesh];
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