using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFSkin : GLTFProperty {
        public int[] bindShapeMatrix = new int[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        public int inverseBindMatrices;
        public int[] joints;
        public int skeleton = -1;

        public override void Load() {

        }

        public Matrix4x4 GetBindShapeMatrix() {
            return new Matrix4x4(
                new Vector4(bindShapeMatrix[0], bindShapeMatrix[1], bindShapeMatrix[2], bindShapeMatrix[3]),
                new Vector4(bindShapeMatrix[4], bindShapeMatrix[5], bindShapeMatrix[6], bindShapeMatrix[7]),
                new Vector4(bindShapeMatrix[8], bindShapeMatrix[9], bindShapeMatrix[10], bindShapeMatrix[11]),
                new Vector4(bindShapeMatrix[12], bindShapeMatrix[13], bindShapeMatrix[14], bindShapeMatrix[15])
            );
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
            if (inverseBindMatrices != -1) {
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