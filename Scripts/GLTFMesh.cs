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
            } else {
                Mesh mesh;
                mesh = new Mesh();
                if (string.IsNullOrEmpty(name)) mesh.name = "mesh" + glTFObject.meshes.IndexOf(this);
                else mesh.name = name;

                List<Vector3> normals = new List<Vector3>();
                List<List<int>> submeshTris = new List<List<int>>();
                List<Vector3> verts = new List<Vector3>();
                List<Vector4> tangents = new List<Vector4>();
                List<Color> colors = new List<Color>();
                List<BoneWeight> weights = new List<BoneWeight>();
                List<Vector2> uv1 = new List<Vector2>();
                List<Vector2> uv2 = new List<Vector2>();
                List<Vector2> uv3 = new List<Vector2>();
                List<Vector2> uv4 = new List<Vector2>();
                List<Vector2> uv5 = new List<Vector2>();
                List<Vector2> uv6 = new List<Vector2>();
                List<Vector2> uv7 = new List<Vector2>();
                List<Vector2> uv8 = new List<Vector2>();

                for (int i = 0; i < primitives.Count; i++) {
                    GLTFPrimitive primitive = primitives[i];

                    int vertStartIndex = verts.Count;

                    // Verts - (Z points backwards in GLTF)
                    if (primitive.attributes.POSITION != -1) {
                        verts.AddRange(glTFObject.accessors[primitive.attributes.POSITION].ReadVec3().Select(v => { v.z = -v.z; return v; }));
                    }

                    // Tris - (Invert all triangles. Instead of flipping each triangle, just flip the entire array. Much easier)
                    if (primitive.indices != -1) {
                        submeshTris.Add(new List<int>(glTFObject.accessors[primitive.indices].ReadInt().Reverse().Select(x => x + vertStartIndex)));
                    }

                    /// Normals - (Z points backwards in GLTF)
                    if (primitive.attributes.NORMAL != -1) {
                        normals.AddRange(glTFObject.accessors[primitive.attributes.NORMAL].ReadVec3().Select(v => { v.z = -v.z; return v; }));
                    } else mesh.RecalculateNormals();

                    // Tangents - (Z points backwards in GLTF)
                    if (primitive.attributes.TANGENT != -1) {
                        tangents.AddRange(glTFObject.accessors[primitive.attributes.TANGENT].ReadVec4().Select(v => { v.z = -v.z; return v; }));
                    } else mesh.RecalculateTangents();

                    // Vertex colors
                    if (primitive.attributes.COLOR_0 != -1) {
                        colors.AddRange(glTFObject.accessors[primitive.attributes.COLOR_0].ReadColor());
                    }

                    // Weights
                    if (primitive.attributes.WEIGHTS_0 != -1 && primitive.attributes.JOINTS_0 != -1) {
                        Vector4[] weights0 = glTFObject.accessors[primitive.attributes.WEIGHTS_0].ReadVec4();
                        Vector4[] joints0 = glTFObject.accessors[primitive.attributes.JOINTS_0].ReadVec4();
                        if (joints0.Length == weights0.Length) {
                            BoneWeight[] boneWeights = new BoneWeight[weights0.Length];
                            for (int k = 0; k < boneWeights.Length; k++) {
                                NormalizeWeights(ref weights0[k]);
                                boneWeights[k].weight0 = weights0[k].x;
                                boneWeights[k].weight1 = weights0[k].y;
                                boneWeights[k].weight2 = weights0[k].z;
                                boneWeights[k].weight3 = weights0[k].w;
                                boneWeights[k].boneIndex0 = Mathf.RoundToInt(joints0[k].x);
                                boneWeights[k].boneIndex1 = Mathf.RoundToInt(joints0[k].y);
                                boneWeights[k].boneIndex2 = Mathf.RoundToInt(joints0[k].z);
                                boneWeights[k].boneIndex3 = Mathf.RoundToInt(joints0[k].w);
                            }
                            weights.AddRange(boneWeights);
                        } else Debug.LogWarning("WEIGHTS_0 and JOINTS_0 not same length. Skipped");
                    }

                    // UVs
                    ReadUVs(ref uv1, primitive.attributes.TEXCOORD_0);
                    ReadUVs(ref uv2, primitive.attributes.TEXCOORD_1);
                    ReadUVs(ref uv3, primitive.attributes.TEXCOORD_2);
                    ReadUVs(ref uv4, primitive.attributes.TEXCOORD_3);
                    ReadUVs(ref uv5, primitive.attributes.TEXCOORD_4);
                    ReadUVs(ref uv6, primitive.attributes.TEXCOORD_5);
                    ReadUVs(ref uv7, primitive.attributes.TEXCOORD_6);
                    ReadUVs(ref uv8, primitive.attributes.TEXCOORD_7);
                }
                mesh.vertices = verts.ToArray();
                mesh.subMeshCount = submeshTris.Count;
                for (int i = 0; i < submeshTris.Count; i++) {
                    mesh.SetTriangles(submeshTris[i].ToArray(), i);
                }
                mesh.normals = normals.ToArray();
                mesh.tangents = tangents.ToArray();
                mesh.colors = colors.ToArray();
                mesh.uv = uv1.ToArray();
                mesh.uv2 = uv2.ToArray();
                mesh.uv3 = uv3.ToArray();
                mesh.uv4 = uv4.ToArray();
                mesh.uv5 = uv5.ToArray();
                mesh.uv6 = uv6.ToArray();
                mesh.uv7 = uv7.ToArray();
                mesh.uv8 = uv8.ToArray();

                mesh.RecalculateBounds();
                cache = mesh;
            }
        }

        private void ReadUVs(ref List<Vector2> uvs, int texcoord) {
            if (texcoord == -1) return;
            Vector2[] _uvs = glTFObject.accessors[texcoord].ReadVec2();
            FlipY(ref _uvs);
            uvs.AddRange(_uvs);
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