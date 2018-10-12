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

        public int mesh = -1;
        public int skin = -1;
        public int camera = -1;

        private GameObject cache;

        public GameObject Create(GLTFObject gLTFObject, Transform parent) {
            if (!cache) cache = new GameObject();
            cache.name = name;
            cache.transform.parent = parent;
            if (matrix != null) Debug.LogWarning("MatrixTRS not supported.");
            if (translation != null) cache.transform.localPosition = new Vector3(translation[0], translation[1], translation[2]);
            if (rotation != null) cache.transform.localRotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            if (scale != null) cache.transform.localScale = new Vector3(scale[0], scale[1], scale[2]);

            if (mesh != -1) {
                Mesh mesh = gLTFObject.meshes[0].GetMesh(gLTFObject, gLTFObject.skins[skin]);
                Renderer renderer;
                if (skin != -1) {
                    SkinnedMeshRenderer smr = cache.AddComponent<SkinnedMeshRenderer>();
                    smr.sharedMesh = mesh;
                    GLTFSkin skinObj = gLTFObject.skins[skin];
                    Transform[] bones = new Transform[skinObj.joints.Length];
                    for (int i = 0; i < bones.Length; i++) {
                        int jointNodeIndex = skinObj.joints[i];
                        GLTFNode jointNode = gLTFObject.nodes[jointNodeIndex];
                        bones[i] = jointNode.GetCached().transform;
                    }
                    smr.bones = bones;
                    smr.rootBone = bones[0];
                    renderer = smr;
                } else {
                    MeshRenderer mr = cache.AddComponent<MeshRenderer>();
                    MeshFilter mf = cache.AddComponent<MeshFilter>();
                    renderer = mr;
                }

#if UNITY_EDITOR
                renderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
#endif
            }

            for (int i = 0; i < children.Count; i++) {
                gLTFObject.nodes[children[i]].Create(gLTFObject, cache.transform);
            }

            return cache;
        }

        /// <summary> Return the last GameObject created by this Node </summary>
        public GameObject GetCached() {
            if (!cache) cache = new GameObject();
            return cache;
        }
    }
}