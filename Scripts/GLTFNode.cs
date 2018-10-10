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

        public GameObject Create(GLTFObject gLTFObject, Transform parent) {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            if (matrix != null) Debug.LogWarning("MatrixTRS not supported.");
            if (translation != null) go.transform.localPosition = new Vector3(translation[0], translation[1], translation[2]);
            if (rotation != null) go.transform.localRotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
            if (scale != null) go.transform.localScale = new Vector3(scale[0], scale[1], scale[2]);

            if (mesh != -1) {
                Mesh mesh = gLTFObject.meshes[0].GetMesh(gLTFObject);
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                MeshFilter mf = go.AddComponent<MeshFilter>();
#if UNITY_EDITOR
                mr.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
#endif
                mf.mesh = mesh;
            }

            for (int i = 0; i < children.Count; i++) {
                gLTFObject.nodes[children[i]].Create(gLTFObject, go.transform);
            }
            return go;
        }
    }
}