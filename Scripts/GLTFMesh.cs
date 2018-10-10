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

        public Mesh GetMesh(GLTFObject gLTFObject) {
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
    }
}