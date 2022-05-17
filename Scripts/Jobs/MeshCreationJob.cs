using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Magnopus.GLTFUtility.Jobs
{
	[BurstCompile]
	public struct MeshCreationJob : IJob
	{
        private struct TangentPair
        {
            public float3 Tangent;
            public float3 BiNormal;
        }

        public Mesh.MeshData meshData;
        public bool generateBounds;
        public bool generateNormals;
        public bool generateTangents;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<VertexAttributeDescriptor> descriptors;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> vertices;
        [DeallocateOnJobCompletion] public NativeArray<float3> normals;
        [DeallocateOnJobCompletion] public NativeArray<float4> tangents;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Color> colors;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> indices;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> indicesStartIndex;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> indicesSubMeshLength;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<MeshTopology> meshTopologies;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv1;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv2;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv3;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv4;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv5;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv6;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv7;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float2> uv8;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<BoneWeight> weights;

        public NativeArray<float3x2> outputBounds;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<float> floatColor32;

        public void Execute()
		{
            int vertexCount = vertices.Length;
            IndexFormat format = vertexCount >= ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            meshData.SetVertexBufferParams(vertexCount, descriptors);
            meshData.SetIndexBufferParams(indices.Length, format);

            int streamIndex = 0;
            NativeArray<float> vb = meshData.GetVertexData<float>();

            bool containsNormalAttr = meshData.HasVertexAttribute(VertexAttribute.Normal);
            bool normalsEmpty = normals.Length == 0;
            if (generateNormals)
            {
                // Generate Normals
                for (int subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++)
                {
                    int indexStart = indicesStartIndex[subMeshIndex];
                    int endIndex = indicesSubMeshLength[subMeshIndex] + indexStart;
                    for (int triIndex = indexStart; triIndex < endIndex; triIndex += 3)
                    {
                        int idx = indices[triIndex];
                        float3 a = vertices[idx];
                        float3 b = vertices[idx + 1];
                        float3 c = vertices[idx + 2];

                        float3 ab = a - b;
                        float3 bc = c - b;
                        float3 cross = math.cross(ab, bc);

                        normals[idx] += cross;
                        normals[idx + 1] += cross;
                        normals[idx + 2] += cross;
                    }
                }

                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = math.normalize(normals[i]);
                }
            }
            bool containsTangentAttr = meshData.HasVertexAttribute(VertexAttribute.Tangent);
            bool tangentsEmpty = tangents.Length == 0;
            if (generateTangents)
            {
                // Generate Tangents
                CalculateTangents();
            }

            int stride = 3;
            stride += containsNormalAttr ? 3 : 0;
            stride += containsTangentAttr ? 4 : 0;

            int strideIndex;
            for (int i = 0; i < vertexCount; i++)
            {
                strideIndex = i * stride;
                vb[strideIndex++] = vertices[i].x;
                vb[strideIndex++] = vertices[i].y;
                vb[strideIndex++] = vertices[i].z;
                if (containsNormalAttr)
                {
                    if (!normalsEmpty)
                    {
                        vb[strideIndex++] = normals[i].x;
                        vb[strideIndex++] = normals[i].y;
                        vb[strideIndex++] = normals[i].z;
                    }
                    else
                    {
                        strideIndex += 3;
                    }
                }
                if (containsTangentAttr)
                {
                    if (!tangentsEmpty)
                    {
                        vb[strideIndex++] = tangents[i].x;
                        vb[strideIndex++] = tangents[i].y;
                        vb[strideIndex++] = tangents[i].z;
                        vb[strideIndex++] = tangents[i].w;
                    }
                }
            }

            bool containsColor = meshData.HasVertexAttribute(VertexAttribute.Color); 
            bool containsUV1 = meshData.HasVertexAttribute(VertexAttribute.TexCoord0); 
            bool containsUV2 = meshData.HasVertexAttribute(VertexAttribute.TexCoord1); 
            bool containsUV3 = meshData.HasVertexAttribute(VertexAttribute.TexCoord2); 
            bool containsUV4 = meshData.HasVertexAttribute(VertexAttribute.TexCoord3); 
            bool containsUV5 = meshData.HasVertexAttribute(VertexAttribute.TexCoord4); 
            bool containsUV6 = meshData.HasVertexAttribute(VertexAttribute.TexCoord5); 
            bool containsUV7 = meshData.HasVertexAttribute(VertexAttribute.TexCoord6); 
            bool containsUV8 = meshData.HasVertexAttribute(VertexAttribute.TexCoord7);

            if (containsColor)
            {
                // C# doesn't have a reinterpret_cast equivalent. Have to use Unity's method and convert a whole array.
                NativeArray<int> intColor32 = new NativeArray<int>(colors.Length, Allocator.Temp);
                for (int i = 0; i < intColor32.Length; i++)
                {
                    Color32 bColor = colors[i];
                    int color = 0;
                    color |= (bColor.a & 255) << 24;
                    color |= (bColor.b & 255) << 16;
                    color |= (bColor.g & 255) << 8;
                    color |= (bColor.r & 255);
                    intColor32[i] = color;
                }
                floatColor32 = intColor32.Reinterpret<float>();
            }

            if (containsColor ||
                containsUV1 ||
                containsUV2 ||
                containsUV3 ||
                containsUV4 ||
                containsUV5 ||
                containsUV6 ||
                containsUV7 ||
                containsUV8)
            {
                // Contains 2nd VB Stream
                stride = 0;
                stride += containsColor ? 1 : 0;
                stride += containsUV1 ? 2 : 0;
                stride += containsUV2 ? 2 : 0;
                stride += containsUV3 ? 2 : 0;
                stride += containsUV4 ? 2 : 0;
                stride += containsUV5 ? 2 : 0;
                stride += containsUV6 ? 2 : 0;
                stride += containsUV7 ? 2 : 0;
                stride += containsUV8 ? 2 : 0;

                streamIndex++;
                vb = meshData.GetVertexData<float>(streamIndex);
                for (int i = 0; i < vertexCount; i++)
                {
                    strideIndex = i * stride;
                    if (containsColor)
                    {
                        vb[i] = floatColor32[i];
                    }
                    if (containsUV1)
                    {
                        vb[strideIndex++] = uv1[i].x;
                        vb[strideIndex++] = uv1[i].y;
                    }
                    if (containsUV2)
                    {
                        vb[strideIndex++] = uv2[i].x;
                        vb[strideIndex++] = uv2[i].y;
                    }
                    if (containsUV3)
                    {
                        vb[strideIndex++] = uv3[i].x;
                        vb[strideIndex++] = uv3[i].y;
                    }
                    if (containsUV4)
                    {
                        vb[strideIndex++] = uv4[i].x;
                        vb[strideIndex++] = uv4[i].y;
                    }
                    if (containsUV5)
                    {
                        vb[strideIndex++] = uv5[i].x;
                        vb[strideIndex++] = uv5[i].y;
                    }
                    if (containsUV6)
                    {
                        vb[strideIndex++] = uv6[i].x;
                        vb[strideIndex++] = uv6[i].y;
                    }
                    if (containsUV7)
                    {
                        vb[strideIndex++] = uv7[i].x;
                        vb[strideIndex++] = uv7[i].y;
                    }
                    if (containsUV8)
                    {
                        vb[strideIndex++] = uv8[i].x;
                        vb[strideIndex++] = uv8[i].y;
                    }
                }
            }
            if (floatColor32.IsCreated)
            {
                floatColor32.Dispose();
            }

            if (meshData.HasVertexAttribute(VertexAttribute.BlendWeight) && meshData.HasVertexAttribute(VertexAttribute.BlendIndices))
            {
                streamIndex++;
                NativeArray<BoneWeightVB> blendVB = meshData.GetVertexData<BoneWeightVB>(streamIndex);
                for (int i = 0; i < weights.Length; i++)
                {
                    var weight = weights[i];
                    blendVB[i] = new BoneWeightVB()
                    {
                        weights = new float4(weight.weight0, weight.weight1, weight.weight2, weight.weight3),
                        indices = new int4(weight.boneIndex0, weight.boneIndex1, weight.boneIndex2, weight.boneIndex3)
                    };
                }
            }

            if (meshData.indexFormat == IndexFormat.UInt16)
            {
                NativeArray<ushort> indexBuffer = meshData.GetIndexData<ushort>();
                for (int i = 0; i < indexBuffer.Length; i++)
                {
                    indexBuffer[i] = (ushort)indices[i];
                }
            }
            else
            {
                NativeArray<uint> indexBuffer = meshData.GetIndexData<uint>();
                indexBuffer.CopyFrom(indices.Reinterpret<uint>());
            }

            float3x2 meshBounds = new float3x2(new float3(math.INFINITY), new float3(float.NegativeInfinity));
            for (int subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++)
            {
                int indexStart = indicesStartIndex[subMeshIndex];
                int length = indicesSubMeshLength[subMeshIndex];
                var topo = meshTopologies[subMeshIndex];
                var sm = new SubMeshDescriptor(indexStart, length, topo);
                if (generateBounds)
                {
                    float3x2 bounds = new float3x2(new float3(math.INFINITY), new float3(float.NegativeInfinity));
                    int endIndex = length + indexStart;
                    for (int triIndex = streamIndex; triIndex < endIndex; triIndex++)
                    {
                        int idx = indices[triIndex];
                        float3 vert = vertices[idx];

                        bounds.c0 = math.min(bounds.c0, vert);
                        bounds.c1 = math.max(bounds.c1, vert);
                        meshBounds.c0 = math.min(meshBounds.c0, vert);
                        meshBounds.c1 = math.max(meshBounds.c1, vert);
                    }
                    sm.bounds = new Bounds((bounds.c0 + bounds.c1) * 0.5f, bounds.c1 - bounds.c0);
                }
                meshData.SetSubMesh(subMeshIndex, sm, MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds);
            }
            if (generateBounds)
            {
                // Weird case where no submeshes exist.
                if (meshData.subMeshCount == 0)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        float3 vert = vertices[i];
                        meshBounds.c0 = math.min(meshBounds.c0, vert);
                        meshBounds.c1 = math.max(meshBounds.c1, vert);
                    }
                }
                outputBounds[0] = meshBounds;
            }
        }

        // Code is taken from both http://foundationsofgameenginedev.com/FGED2-sample.pdf and Unity's own source code.
        private void CalculateTangents()
        {
            int vertexCount = vertices.Length;

            NativeArray<TangentPair> tempTangents = new NativeArray<TangentPair>(vertexCount, Allocator.Temp);

            for (int i = 0; i < indices.Length; i += 3)
            {
                int i0 = indices[i];
                int i1 = indices[i + 1];
                int i2 = indices[i + 2];
                float3 p0 = vertices[i0];
                float3 p1 = vertices[i1];
                float3 p2 = vertices[i2];
                float2 w0 = uv1[i0];
                float2 w1 = uv1[i1];
                float2 w2 = uv1[i2];

                float3 e1 = p1 - p0;
                float3 e2 = p2 - p0;
                float x1 = w1.x - w0.x; 
                float x2 = w2.x - w0.x;
                float y1 = w1.y - w0.y;
                float y2 = w2.y - w0.y;
                float r = 1.0f / (x1 * y2 - x2 * y1);
                float3 t = ((e1 * y2) - (e2 * y1)) * r;
                float3 b = ((e2 * x1) - (e1 * x2)) * r;

                TangentPair tp0 = tempTangents[i0];
                TangentPair tp1 = tempTangents[i1];
                TangentPair tp2 = tempTangents[i2];
                tp0.Tangent += t;
                tp1.Tangent += t;
                tp2.Tangent += t;
                tp0.BiNormal += b;
                tp1.BiNormal += b;
                tp2.BiNormal += b;
                tempTangents[i0] = tp0;
                tempTangents[i1] = tp1;
                tempTangents[i2] = tp2;
            }

            // Orthogonalize Tangents
            for (int i = 0; i < vertexCount; i++)
            {
                float3 normal = normals[i];
                float3 tangent = tempTangents[i].Tangent;
                float3 binormal = tempTangents[i].BiNormal;

                // Try Gram-Schmidt orthonormalize.
                // This might fail in degenerate cases which we all handle separately.

                float nDotT = math.dot(normal, tangent);
                float3 newTangent = new float3(tangent.x - nDotT * normal.x, tangent.y - nDotT * normal.y, tangent.z - nDotT * normal.z);

                float magT = math.length(newTangent);
                newTangent /= magT;

                float nDotB = math.dot(normal, binormal);
                float tDotB = math.dot(newTangent, binormal) * magT;

                float3 newBinormal = new float3(binormal.x - nDotB * normal.x - tDotB * newTangent.x, 
                    binormal.y - nDotB * normal.y - tDotB * newTangent.y, 
                    binormal.z - nDotB * normal.z - tDotB * newTangent.z);

                float magB = math.length(newBinormal);
                newBinormal /= magB;

                tangent = newTangent;
                binormal = newBinormal;

                const double kNormalizeEpsilon = 1e-6;
                if (magT <= kNormalizeEpsilon || magB <= kNormalizeEpsilon)
                {
                    // Create a tangent from scratch
                    float3 axis1, axis2;

                    float dpXN = math.abs(math.dot(math.right(), normal));
                    float dpYN = math.abs(math.dot(math.up(), normal));
                    float dpZN = math.abs(math.dot(math.forward(), normal));

                    if (dpXN <= dpYN && dpXN <= dpZN)
                    {
                        axis1 = math.right();
                        if (dpYN <= dpZN)
                            axis2 = math.up();
                        else
                            axis2 = math.forward();
                    }
                    else if (dpYN <= dpXN && dpYN <= dpZN)
                    {
                        axis1 = math.up();
                        if (dpXN <= dpZN)
                            axis2 = math.right();
                        else
                            axis2 = math.forward();
                    }
                    else
                    {
                        axis1 = math.forward();
                        if (dpXN <= dpYN)
                            axis2 = math.right();
                        else
                            axis2 = math.up();
                    }

                    tangent = axis1 - math.dot(normal, axis1) * normal;
                    binormal = axis2 - math.dot(normal, axis2) * normal - math.dot(tangent, axis2) * math.normalizesafe(tangent);

                    tangent = math.normalizesafe(tangent);
                    binormal = math.normalizesafe(binormal);
                }

                float dp = math.dot(math.cross(normal, tangent), binormal);
                float w = dp > 0.0f ? 1.0f : -1.0f;
                tangents[i] = new float4(tangent, w);

            }

            tempTangents.Dispose();
        }
    }

    public struct BoneWeightVB
    {
        public float4 weights;
        public int4 indices;
    }
}
