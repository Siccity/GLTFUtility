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
                    int addedVertices = 0;

                    // Verts - (Z points backwards in GLTF)
                    if (primitive.attributes.POSITION != -1) {
                        IEnumerable<Vector3> newVerts = glTFObject.accessors[primitive.attributes.POSITION].ReadVec3().Select(v => { v.z = -v.z; return v; });
                        verts.AddRange(newVerts);
                        addedVertices = newVerts.Count();
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
                    ReadUVs(ref uv1, primitive.attributes.TEXCOORD_0, addedVertices);
                    ReadUVs(ref uv2, primitive.attributes.TEXCOORD_1, addedVertices);
                    ReadUVs(ref uv3, primitive.attributes.TEXCOORD_2, addedVertices);
                    ReadUVs(ref uv4, primitive.attributes.TEXCOORD_3, addedVertices);
                    ReadUVs(ref uv5, primitive.attributes.TEXCOORD_4, addedVertices);
                    ReadUVs(ref uv6, primitive.attributes.TEXCOORD_5, addedVertices);
                    ReadUVs(ref uv7, primitive.attributes.TEXCOORD_6, addedVertices);
                    ReadUVs(ref uv8, primitive.attributes.TEXCOORD_7, addedVertices);
                }
                mesh.vertices = verts.ToArray();
                mesh.subMeshCount = submeshTris.Count;
                for (int i = 0; i < submeshTris.Count; i++) {
                    mesh.SetTriangles(submeshTris[i].ToArray(), i);
                }
                mesh.normals = normals.ToArray();
                mesh.tangents = tangents.ToArray();
                mesh.colors = colors.ToArray();
                if (uv1.Count != 0) mesh.uv = uv1.ToArray();
                if (uv2.Count != 0) mesh.uv2 = uv2.ToArray();
                if (uv3.Count != 0) mesh.uv3 = uv3.ToArray();
                if (uv4.Count != 0) mesh.uv4 = uv4.ToArray();
                if (uv5.Count != 0) mesh.uv5 = uv5.ToArray();
                if (uv6.Count != 0) mesh.uv6 = uv6.ToArray();
                if (uv7.Count != 0) mesh.uv7 = uv7.ToArray();
                if (uv8.Count != 0) mesh.uv8 = uv8.ToArray();

                mesh.RecalculateBounds();
                cache = mesh;
            }
        }

        private void ReadUVs(ref List<Vector2> uvs, int texcoord, int newVerts) {
            if (texcoord == -1) {
                // If there are no valid texcoords, add some empty filler uvs so it still matches the vertex array
                uvs.AddRange(new Vector2[newVerts]);
                return;
            }
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