using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFMesh {
        public string name;
        public List<GLTFPrimitive> primitives;
        /// <summary> Morph target weights </summary>
        public List<float> weights;

        private Mesh cache;

        public Mesh GetMesh(GLTFObject gLTFObject) {
            if (cache) return cache;
            else {
                if (primitives.Count == 0) {
                    Debug.LogWarning("0 primitives in mesh");
                    return null;
                } else if (primitives.Count == 1) {
                    Mesh mesh;
                    mesh = new Mesh();

                    // Name
                    mesh.name = name;

                    // Verts - (Z points backwards in GLTF)
                    if (primitives[0].attributes.POSITION != -1) {
                        mesh.vertices = gLTFObject.accessors[primitives[0].attributes.POSITION].ReadVec3(gLTFObject).Select(v => { v.z = -v.z; return v; }).ToArray();
                    }

                    // Tris - (Instead of flipping each triangle, just flip the entire array. Much easier)
                    if (primitives[0].indices != -1) {
                        mesh.triangles = gLTFObject.accessors[primitives[0].indices].ReadInt(gLTFObject).Reverse().ToArray();
                    }

                    // Normals - (Z points backwards in GLTF)
                    if (primitives[0].attributes.NORMAL != -1) {
                        mesh.normals = gLTFObject.accessors[primitives[0].attributes.NORMAL].ReadVec3(gLTFObject).Select(v => { v.z = -v.z; return v; }).ToArray();
                    } else mesh.RecalculateNormals();

                    // Tangents - (Z points backwards in GLTF)
                    if (primitives[0].attributes.TANGENT != -1) {
                        mesh.tangents = gLTFObject.accessors[primitives[0].attributes.TANGENT].ReadVec4(gLTFObject).Select(v => { v.z = -v.z; return v; }).ToArray();
                    } else mesh.RecalculateTangents();

                    // Vertex colors
                    if (primitives[0].attributes.COLOR_0 != -1) {
                        mesh.colors = gLTFObject.accessors[primitives[0].attributes.COLOR_0].ReadColor(gLTFObject);
                    }

                    // Weights
                    if (primitives[0].attributes.WEIGHTS_0 != -1 && primitives[0].attributes.JOINTS_0 != -1) {
                        Vector4[] weights0 = gLTFObject.accessors[primitives[0].attributes.WEIGHTS_0].ReadVec4(gLTFObject);
                        Vector4[] joints0 = gLTFObject.accessors[primitives[0].attributes.JOINTS_0].ReadVec4(gLTFObject);
                        if (joints0.Length == weights0.Length) {
                            BoneWeight[] boneWeights = new BoneWeight[weights0.Length];
                            for (int i = 0; i < boneWeights.Length; i++) {
                                NormalizeWeights(ref weights0[i]);
                                boneWeights[i].weight0 = weights0[i].x;
                                boneWeights[i].weight1 = weights0[i].y;
                                boneWeights[i].weight2 = weights0[i].z;
                                boneWeights[i].weight0 = weights0[i].x;
                                boneWeights[i].boneIndex0 = Mathf.RoundToInt(joints0[i].x);
                                boneWeights[i].boneIndex1 = Mathf.RoundToInt(joints0[i].y);
                                boneWeights[i].boneIndex2 = Mathf.RoundToInt(joints0[i].z);
                                boneWeights[i].boneIndex3 = Mathf.RoundToInt(joints0[i].w);
                            }
                            mesh.boneWeights = boneWeights;
                        } else Debug.LogWarning("WEIGHTS_0 and JOINTS_0 not same length. Skipped");
                    }

                    // UVs
                    if (primitives[0].attributes.TEXCOORD_0 != -1) { // UV 1
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_0].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_1 != -1) { // UV 2
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_1].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv2 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_2 != -1) { // UV 3
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_2].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv3 = uvs;
                    }
                    if (primitives[0].attributes.TEXCOORD_3 != -1) { // UV 4
                        Vector2[] uvs = gLTFObject.accessors[primitives[0].attributes.TEXCOORD_3].ReadVec2(gLTFObject);
                        FlipY(ref uvs);
                        mesh.uv4 = uvs;
                    }

                    mesh.RecalculateBounds();
                    cache = mesh;
                    return mesh;
                } else {
                    Debug.LogError("Multiple primitives per mesh not supported");
                    return null;
                }
            }
        }

        public void NormalizeWeights(ref Vector4 weights) {
            float total = weights.x + weights.y + weights.z + weights.w;
            float mult = 1f / total;
            weights.x *= mult;
            weights.y *= mult;
            weights.z *= mult;
            weights.w *= mult;
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