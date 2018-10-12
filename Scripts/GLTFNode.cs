using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFNode {
        public string name;
        /// <summary> Indices of child nodes </summary>
        public List<int> children;

        /// <summary> Local TRS </summary>
        public float[] matrix;

        /// <summary> Local position </summary>
        public float[] translation;
        /// <summary> Local rotation </summary>
        public float[] rotation;
        /// <summary> Local scale </summary>
        public float[] scale;

        public Vector3 Position { get { return new Vector3(translation[0], translation[1], translation[2]); } }
        public Vector3 Scale { get { return new Vector3(scale[0], scale[1], scale[2]); } }
        public Quaternion Rotation { get { return new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]); } }

        public int mesh = -1;
        public int skin = -1;
        public int camera = -1;

        private Transform transform;

        /// <summary> Recursively set up this node's transform in the scene, followed by its children </summary>
        public Transform CreateTransform(GLTFObject gLTFObject, Transform parent) {
            if (transform == null) transform = new GameObject().transform;
            transform.parent = parent;
            transform.gameObject.name = name;
            if (matrix != null) Debug.LogWarning("MatrixTRS not supported.");
            if (translation != null) transform.localPosition = new Vector3(translation[0], translation[1], translation[2]);
            if (rotation != null) transform.localRotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            if (scale != null) transform.localScale = new Vector3(scale[0], scale[1], scale[2]);

            for (int i = 0; i < children.Count; i++) {
                gLTFObject.nodes[children[i]].CreateTransform(gLTFObject, transform);
            }

            return transform;
        }

        /// <summary> Set up various components defined in the node. Call after all transforms have been set up </summary>
        public void SetupComponents(GLTFObject gLTFObject) {
            if (transform == null) {
                Debug.LogWarning("Transform is null. Call CreateTransform before calling SetupComponents");
                return;
            }
            if (this.mesh != -1) {
                GLTFMesh gLTFMesh = gLTFObject.meshes[this.mesh];
                Mesh mesh = gLTFMesh.GetMesh(gLTFObject);
                Renderer renderer;
                if (skin != -1) {
                    SkinnedMeshRenderer smr = transform.gameObject.AddComponent<SkinnedMeshRenderer>();
                    GLTFSkin gLTFSkin = gLTFObject.skins[skin];
                    Transform[] bones = new Transform[gLTFSkin.joints.Length];
                    for (int i = 0; i < bones.Length; i++) {
                        int jointNodeIndex = gLTFSkin.joints[i];
                        GLTFNode jointNode = gLTFObject.nodes[jointNodeIndex];
                        bones[i] = jointNode.transform;
                    }
                    smr.bones = bones;
                    smr.rootBone = bones[0];
                    renderer = smr;

                    // Bindposes
                    if (gLTFSkin.inverseBindMatrices != -1) {
                        Matrix4x4 m = gLTFObject.nodes[0].transform.localToWorldMatrix;
                        Matrix4x4[] bindPoses = new Matrix4x4[gLTFSkin.joints.Length];
                        for (int i = 0; i < gLTFSkin.joints.Length; i++) {
                            bindPoses[i] = gLTFObject.nodes[gLTFSkin.joints[i]].transform.worldToLocalMatrix * m;
                        }
                        mesh.bindposes = bindPoses;
                    }
                    smr.sharedMesh = mesh;
                } else {
                    MeshRenderer mr = transform.gameObject.AddComponent<MeshRenderer>();
                    MeshFilter mf = transform.gameObject.AddComponent<MeshFilter>();
                    renderer = mr;
                }

#if UNITY_EDITOR
                renderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
#endif
            }
        }
    }
}