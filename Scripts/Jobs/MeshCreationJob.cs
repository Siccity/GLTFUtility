using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace Siccity.GLTFUtility.Jobs
{
	[BurstCompile]
	public struct MeshCreationJob : IJob
	{
        public Mesh.MeshData meshData;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> vertices;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> normals;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float4> tangents;
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

		public void Execute()
		{
            int streamIndex = 0;
            NativeArray<float3> verts = meshData.GetVertexData<float3>(streamIndex++);
            verts.CopyFrom(vertices);

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

            for (int subMeshIndex = 0; subMeshIndex < meshData.subMeshCount; subMeshIndex++)
            {
                int indexStart = indicesStartIndex[subMeshIndex];
                int length = indicesSubMeshLength[subMeshIndex];
                var topo = meshTopologies[subMeshIndex];
                meshData.SetSubMesh(subMeshIndex, new SubMeshDescriptor(indexStart, length, topo));
            }

            if (meshData.HasVertexAttribute(VertexAttribute.Normal))
            {
                NativeArray<float3> normals = meshData.GetVertexData<float3>(streamIndex++);
                normals.CopyFrom(this.normals);
            }

            if (meshData.HasVertexAttribute(VertexAttribute.Tangent))
            {
                NativeArray<float4> tangents = meshData.GetVertexData<float4>(streamIndex++);
                tangents.CopyFrom(this.tangents);
            }

            if (meshData.HasVertexAttribute(VertexAttribute.Color))
            {
                NativeArray<Color> colors = meshData.GetVertexData<Color>(streamIndex++);
                colors.CopyFrom(this.colors);
            }

            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord0))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                int dimension = meshData.GetVertexAttributeDimension(VertexAttribute.TexCoord0);
                var length = meshData.GetVertexAttributeFormat(VertexAttribute.TexCoord0);
                uvs.CopyFrom(uv1);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord1))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv2);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord2))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv3);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord3))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv4);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord4))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv5);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord5))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv6);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord6))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv7);
            }
            if (meshData.HasVertexAttribute(VertexAttribute.TexCoord7))
            {
                NativeArray<float2> uvs = meshData.GetVertexData<float2>(streamIndex++);
                uvs.CopyFrom(uv8);
            }

            if (meshData.HasVertexAttribute(VertexAttribute.BlendWeight) && meshData.HasVertexAttribute(VertexAttribute.BlendIndices))
            {
                Debug.Log("Blends");
                NativeArray<float4> blendWeights = meshData.GetVertexData<float4>(streamIndex++);
                NativeArray<int4> blendIndices = meshData.GetVertexData<int4>(streamIndex++);
                for (int i = 0; i < weights.Length; i++)
                {
                    var weight = weights[i];
                    blendWeights[i] = new float4(weight.weight0, weight.weight1, weight.weight2, weight.weight3);
                    blendIndices[i] = new int4(weight.boneIndex0, weight.boneIndex1, weight.boneIndex2, weight.boneIndex3);
                }
            }
        }
	}
}
