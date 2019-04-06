using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFSkin : GLTFProperty {
        public int inverseBindMatrices;
        public int[] joints;
        public int skeleton = -1;

        public override void Load() {

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