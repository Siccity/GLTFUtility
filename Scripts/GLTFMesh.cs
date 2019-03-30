using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFMesh : GLTFProperty {

#region Serialized fields
        public string name;
        public List<GLTFPrimitive> primitives;
        /// <summary> Morph target weights </summary>
        public List<float> weights;
#endregion

#region Non-serialized fields
        private Mesh cache;
#endregion

        public override void Load() {
            if (primitives.Count == 0) {
                Debug.LogWarning("0 primitives in mesh");
            } else if (primitives.Count >= 1) {
                if (primitives.Count > 1) Debug.LogWarning("Multiple primitives per mesh not supported");

                Mesh mesh;
                mesh = new Mesh();

                // Name
                if (string.IsNullOrEmpty(name)) mesh.name = "mesh" + glTFObject.meshes.IndexOf(this);
                else mesh.name = name;

                // Verts - (Z points backwards in GLTF)
                if (primitives[0].attributes.POSITION != -1) {
                    mesh.vertices = glTFObject.accessors[primitives[0].attributes.POSITION].ReadVec3().Select(v => { v.z = -v.z; return v; }).ToArray();
                }

                // Tris - (Instead of flipping each triangle, just flip the entire array. Much easier)
                if (primitives[0].indices != -1) {
                    mesh.triangles = glTFObject.accessors[primitives[0].indices].ReadInt().Reverse().ToArray();
                }

                // Normals - (Z points backwards in GLTF)
                if (primitives[0].attributes.NORMAL != -1) {
                    mesh.normals = glTFObject.accessors[primitives[0].attributes.NORMAL].ReadVec3().Select(v => { v.z = -v.z; return v; }).ToArray();
                } else mesh.RecalculateNormals();

                // Tangents - (Z points backwards in GLTF)
                if (primitives[0].attributes.TANGENT != -1) {
                    mesh.tangents = glTFObject.accessors[primitives[0].attributes.TANGENT].ReadVec4().Select(v => { v.z = -v.z; return v; }).ToArray();
                } else mesh.RecalculateTangents();

                // Vertex colors
                if (primitives[0].attributes.COLOR_0 != -1) {
                    mesh.colors = glTFObject.accessors[primitives[0].attributes.COLOR_0].ReadColor();
                }

                // Weights
                if (primitives[0].attributes.WEIGHTS_0 != -1 && primitives[0].attributes.JOINTS_0 != -1) {
                    Vector4[] weights0 = glTFObject.accessors[primitives[0].attributes.WEIGHTS_0].ReadVec4();
                    Vector4[] joints0 = glTFObject.accessors[primitives[0].attributes.JOINTS_0].ReadVec4();
                    if (joints0.Length == weights0.Length) {
                        BoneWeight[] boneWeights = new BoneWeight[weights0.Length];
                        for (int i = 0; i < boneWeights.Length; i++) {
                            NormalizeWeights(ref weights0[i]);
                            boneWeights[i].weight0 = weights0[i].x;
                            boneWeights[i].weight1 = weights0[i].y;
                            boneWeights[i].weight2 = weights0[i].z;
                            boneWeights[i].weight3 = weights0[i].w;
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
                    Vector2[] uvs = glTFObject.accessors[primitives[0].attributes.TEXCOORD_0].ReadVec2();
                    FlipY(ref uvs);
                    mesh.uv = uvs;
                }
                if (primitives[0].attributes.TEXCOORD_1 != -1) { // UV 2
                    Vector2[] uvs = glTFObject.accessors[primitives[0].attributes.TEXCOORD_1].ReadVec2();
                    FlipY(ref uvs);
                    mesh.uv2 = uvs;
                }
                if (primitives[0].attributes.TEXCOORD_2 != -1) { // UV 3
                    Vector2[] uvs = glTFObject.accessors[primitives[0].attributes.TEXCOORD_2].ReadVec2();
                    FlipY(ref uvs);
                    mesh.uv3 = uvs;
                }
                if (primitives[0].attributes.TEXCOORD_3 != -1) { // UV 4
                    Vector2[] uvs = glTFObject.accessors[primitives[0].attributes.TEXCOORD_3].ReadVec2();
                    FlipY(ref uvs);
                    mesh.uv4 = uvs;
                }

                mesh.RecalculateBounds();
                cache = mesh;
            }
        }

        public Mesh GetMesh() {
            return cache;
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