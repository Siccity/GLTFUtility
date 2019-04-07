using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFSkin : GLTFProperty {

#region Serialized fields
        /// <summary> Index of accessor containing inverse bind shape matrices </summary>
        public int inverseBindMatrices = -1;
        public int[] joints;
        public int skeleton = -1;
#endregion

#region Non-serialized fields
        public Matrix4x4[] InverseBindMatrices { get; private set; }

#endregion

        protected override bool OnLoad() {
            // Inverse bind matrices
            if (inverseBindMatrices != -1) {
                InverseBindMatrices = glTFObject.accessors[inverseBindMatrices].ReadMatrix4x4();
                for (int i = 0; i < InverseBindMatrices.Length; i++) {
                    // Flip the matrix from GLTF to Unity format. This was done through trial and error, i can't explain it.
                    Matrix4x4 m = InverseBindMatrices[i];
                    Vector4 row0 = m.GetRow(0);
                    row0.z = -row0.z;
                    Vector4 row1 = m.GetRow(1);
                    row1.z = -row1.z;
                    Vector4 row2 = m.GetRow(2);
                    row2.x = -row2.x;
                    row2.y = -row2.y;
                    Vector4 row3 = m.GetRow(3);
                    row3.z = -row3.z;
                    m.SetColumn(0, row0);
                    m.SetColumn(1, row1);
                    m.SetColumn(2, row2);
                    m.SetColumn(3, row3);
                    InverseBindMatrices[i] = m;
                }
            }
            return true;
        }

        public SkinnedMeshRenderer SetupSkinnedRenderer(GameObject go, Mesh mesh) {
            SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
            Transform[] bones = new Transform[joints.Length];
            for (int i = 0; i < bones.Length; i++) {
                int jointNodeIndex = joints[i];
                GLTFNode jointNode = glTFObject.nodes[jointNodeIndex];
                bones[i] = jointNode.Transform;
            }
            smr.bones = bones;
            smr.rootBone = bones[0];

            // Bindposes
            if (InverseBindMatrices != null) {
                if (InverseBindMatrices.Length != joints.Length) Debug.LogWarning("InverseBindMatrices count and joints count not the same");
                Matrix4x4 m = glTFObject.nodes[0].Transform.localToWorldMatrix;
                Matrix4x4[] bindPoses = new Matrix4x4[joints.Length];
                for (int i = 0; i < joints.Length; i++) {
                    bindPoses[i] = InverseBindMatrices[i];
                }
                mesh.bindposes = bindPoses;
            } else {
                Matrix4x4 m = glTFObject.nodes[0].Transform.localToWorldMatrix;
                Matrix4x4[] bindPoses = new Matrix4x4[joints.Length];
                for (int i = 0; i < joints.Length; i++) {
                    bindPoses[i] = glTFObject.nodes[joints[i]].Transform.worldToLocalMatrix * m;
                }
                mesh.bindposes = bindPoses;
            }
            smr.sharedMesh = mesh;
            return smr;
        }
    }
}