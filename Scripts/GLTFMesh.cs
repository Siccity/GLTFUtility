using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFMesh {
        public string name;
        public List<GLTFPrimitive> primitives;
        /// <summary> Morph target weights </summary>
        public List<float> weights;

        private Mesh cache;

        public Mesh GetMesh(GLTFObject gLTFObject, GLTFSkin skin) {
            if (cache == null) {
                if (primitives.Count == 0) {
                    cache = new Mesh() { name = name };
                } else if (primitives.Count == 1) {
                    cache = new Mesh();

                    // Name
                    cache.name = name;

                    // Verts
                    if (primitives[0].attributes.POSITION != -1) {
                        cache.vertices = gLTFObject.accessors[primitives[0].attributes.POSITION].ReadVec3(gLTFObject);
                    }

                    // Tris
                    if (primitives[0].indices != -1) {
                        cache.triangles = gLTFObject.accessors[primitives[0].indices].ReadInt(gLTFObject);
                    }

                    // Normals
                    if (primitives[0].attributes.NORMAL != -1) {
                        cache.normals = gLTFObject.accessors[primitives[0].attributes.NORMAL].ReadVec3(gLTFObject);
                    } else cache.RecalculateNormals();

                    // Tangents
                    if (primitives[0].attributes.TANGENT != -1) {
                        cache.tangents = gLTFObject.accessors[primitives[0].attributes.TANGENT].ReadVec4(gLTFObject);
                    } else cache.RecalculateTangents();

                    // Weights
                    if (primitives[0].attributes.WEIGHTS_0 != -1 && primitives[0].attributes.JOINTS_0 != -1) {
                        Vector4[] weights0 = gLTFObject.accessors[primitives[0].attributes.WEIGHTS_0].ReadVec4(gLTFObject);
                        Vector4[] joints0 = gLTFObject.accessors[primitives[0].attributes.JOINTS_0].ReadVec4(gLTFObject);
                        if (joints0.Length == weights0.Length) {
                            BoneWeight[] boneWeights = new BoneWeight[weights0.Length];
                            for (int i = 0; i < boneWeights.Length; i++) {
                                boneWeights[i].weight0 = weights0[i].x;
                                boneWeights[i].weight1 = weights0[i].y;
                                boneWeights[i].weight2 = weights0[i].z;
                                boneWeights[i].weight0 = weights0[i].x;
                                boneWeights[i].boneIndex0 = Mathf.RoundToInt(joints0[i].x);
                                boneWeights[i].boneIndex1 = Mathf.RoundToInt(joints0[i].y);
                                boneWeights[i].boneIndex2 = Mathf.RoundToInt(joints0[i].z);
                                boneWeights[i].boneIndex3 = Mathf.RoundToInt(joints0[i].w);
                            }
                            cache.boneWeights = boneWeights;
                        } else Debug.LogWarning("WEIGHTS_0 and JOINTS_0 not same length. Skipped");
                    }

                    // Bindposes
                    if (skin.inverseBindMatrices != -1) {
                        Matrix4x4[] bindPoses = gLTFObject.accessors[skin.inverseBindMatrices].ReadMatrix4x4(gLTFObject);
                        cache.bindposes = bindPoses;
                    }

                    // UVs
                    if (primitives[0].attributes.TEXCOORD_0 != -1) { // UV 1
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_0].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        cache.uv = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_1 != -1) { // UV 2
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_1].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        cache.uv2 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_2 != -1) { // UV 3
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_2].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        cache.uv3 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_3 != -1) { // UV 4
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_3].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        cache.uv4 = uvs;
                    }

                    cache.RecalculateBounds();
                } else {
                    Debug.LogError("Multiple primitives per mesh not supported");
                    return new Mesh() { name = name };
                }
            }
            return cache;
        }

        public void FlipY(ref Vector2[] uv) {
            for (int i = 0; i < uv.Length; i++) {
                uv[i].y = 1 - uv[i].y;
            }
        }

        public Mesh GetCachedMesh() {
            if (!cache) Debug.LogWarning("No mesh cached for " + name);
            return cache;
        }
    }
}