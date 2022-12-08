// Copyright 2017 The Draco Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public unsafe class GLTFUtilityDracoLoader
{
	// These values must be exactly the same as the values in draco_types.h.
	// Attribute data type.
	enum DataType
	{
		DT_INVALID = 0,
		DT_INT8,
		DT_UINT8,
		DT_INT16,
		DT_UINT16,
		DT_INT32,
		DT_UINT32,
		DT_INT64,
		DT_UINT64,
		DT_FLOAT32,
		DT_FLOAT64,
		DT_BOOL
	}

	// These values must be exactly the same as the values in
	// geometry_attribute.h.
	// Attribute type.
	enum AttributeType
	{
		INVALID = -1,
		POSITION = 0,
		NORMAL = 1,
		COLOR = 2,
		TEX_COORD = 3,
		GENERIC = 4
	}

	// The order must be consistent with C++ interface.
	[StructLayout(LayoutKind.Sequential)]
	public struct DracoData
	{
		public int dataType;
		public IntPtr data;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DracoAttribute
	{
		public int attributeType;
		public int dataType;
		public int numComponents;
		public int uniqueId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DracoMesh
	{
		public int numFaces;
		public int numVertices;
		public int numAttributes;

		public bool isPointCloud;
		public IndexFormat indexFormat => numVertices >= ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
	}

	public struct MeshAttributes
	{
		public int pos, norms, uv, joints, weights, col;

		public MeshAttributes(int pos, int norms, int uv, int joints, int weights, int col)
		{
			this.pos = pos;
			this.norms = norms;
			this.uv = uv;
			this.joints = joints;
			this.weights = weights;
			this.col = col;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4<T> where T : struct
	{
		public T x;
		public T y;
		public T z;
		public T w;
	}

	public class AsyncMesh
	{
		public int[] tris;
		public Vector3[] verts;
		public Vector2[] uv;
		public Vector3[] norms;
		public BoneWeight[] boneWeights;
		public Color[] colors;
	}

#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_IOS)
        const string DRACODEC_UNITY_LIB = "__Internal";
#elif UNITY_ANDROID || UNITY_STANDALONE || UNITY_WSA || UNITY_EDITOR || PLATFORM_LUMIN
	const string DRACODEC_UNITY_LIB = "dracodec_unity";
#endif

	// Release data associated with DracoMesh.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern void ReleaseDracoMesh(
		DracoMesh** mesh);

	// Release data associated with DracoAttribute.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern void
		ReleaseDracoAttribute(DracoAttribute** attr);

	// Release attribute data.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern void ReleaseDracoData(
		DracoData** data);

// Decodes compressed Draco::Mesh in buffer to mesh. On input, mesh
// must be null. The returned mesh must released with ReleaseDracoMesh.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern int DecodeDracoMeshStep1(
		byte[] buffer, int length, DracoMesh** mesh, void** decoder, void** decoderBuffer);


// Decodes compressed Draco::Mesh in buffer to mesh. On input, mesh
// must be null. The returned mesh must released with ReleaseDracoMesh.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern int DecodeDracoMeshStep2(DracoMesh** mesh, void* decoder, void* decoderBuffer);


	// Returns the DracoAttribute at index in mesh. On input, attribute must be
	// null. The returned attr must be released with ReleaseDracoAttribute.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern bool GetAttribute(
		DracoMesh* mesh, int index, DracoAttribute** attr);

	// Returns the DracoAttribute of type at index in mesh. On input, attribute
	// must be null. E.g. If the mesh has two texture coordinates then
	// GetAttributeByType(mesh, AttributeType.TEX_COORD, 1, &attr); will return
	// the second TEX_COORD attribute. The returned attr must be released with
	// ReleaseDracoAttribute.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern bool GetAttributeByType(
		DracoMesh* mesh, AttributeType type, int index, DracoAttribute** attr);

	// Returns the DracoAttribute with unique_id in mesh. On input, attribute
	// must be null.The returned attr must be released with
	// ReleaseDracoAttribute.
	[DllImport(DRACODEC_UNITY_LIB)]
	private static extern bool
		GetAttributeByUniqueId(DracoMesh* mesh, int unique_id,
			DracoAttribute** attr);


	/// <summary>
	/// Returns an array of indices as well as the type of data in data_type. On
	/// input, indices must be null. The returned indices must be released with
	/// ReleaseDracoData.
	/// </summary>
	/// <param name="mesh">DracoMesh to extract indices from</param>
	/// <param name="dataType">Index data type (int or short) </param>
	/// <param name="indices">Destination index buffer</param>
	/// <param name="indicesCount">Number of indices (equals triangle count * 3)</param>
	/// <param name="flip">If true, triangle vertex order is reverted</param>
	/// <returns>True if extraction succeeded, false otherwise</returns>
	[DllImport(DRACODEC_UNITY_LIB)]
	static extern bool GetMeshIndices(
		DracoMesh* mesh,
		DataType dataType,
		void* indices,
		int indicesCount,
		bool flip
	);

	// Returns an array of attribute data as well as the type of data in
	// data_type. On input, data must be null. The returned data must be
	// released with ReleaseDracoData.
	[DllImport(DRACODEC_UNITY_LIB)]
	unsafe static extern bool GetAttributeData(
		DracoMesh* mesh, DracoAttribute* attr, DracoData** data, bool flip);

	// Decodes a Draco mesh, creates a Unity mesh from the decoded data and
	// adds the Unity mesh to meshes. encodedData is the compressed Draco mesh.
	public unsafe AsyncMesh LoadMesh(byte[] encodedData, MeshAttributes attributes)
	{
		DracoMesh* mesh = null;
		void* decoder;
		void* buffer;
		if (DecodeDracoMeshStep1(encodedData, encodedData.Length, &mesh, &decoder, &buffer) <= 0)
		{
			Debug.Log("Failed: Decoding error.");
			return null;
		}

		var meshPtrPtr = &mesh;
		if (DecodeDracoMeshStep2(meshPtrPtr, decoder, buffer) <= 0)
		{
			Debug.Log("Failed: Decoding error.");
			return null;
		}

		AsyncMesh unityMesh = CreateAsyncMesh(mesh, attributes);

		int numFaces = mesh->numFaces;
		ReleaseDracoMesh(&mesh);
		if (numFaces > 0) return unityMesh;
		else return null;
	}

	// Creates a Unity mesh from the decoded Draco mesh.
	public unsafe AsyncMesh CreateAsyncMesh(DracoMesh* dracoMesh, MeshAttributes attributes)
	{
		int numFaces = dracoMesh->numFaces;

		AsyncMesh mesh = new AsyncMesh();
		mesh.tris = new int[dracoMesh->numFaces * 3];
		mesh.verts = new Vector3[dracoMesh->numVertices];

		var dataType = dracoMesh->indexFormat == IndexFormat.UInt16 ? DataType.DT_UINT16 : DataType.DT_UINT32;
		var indicesPtr = UnsafeUtility.AddressOf(ref mesh.tris[0]);
		DracoData* indicesData;
		GetMeshIndices(dracoMesh, dataType, indicesPtr, mesh.tris.Length, false);
		int elementSize =
			DataTypeSize((DataType) dataType);
		UnsafeUtility.MemCpy(indicesPtr, indicesPtr,
			mesh.tris.Length * elementSize);
		ReleaseDracoData(&indicesData);

		DracoAttribute* attr = null;

		// Copy positions.
		if (GetAttributeByUniqueId(dracoMesh, attributes.pos, &attr))
		{
			DracoData* posData = null;
			GetAttributeData(dracoMesh, attr, &posData, false);
			elementSize = DataTypeSize((DataType) posData->dataType) *
			              attr->numComponents;
			var newVerticesPtr = UnsafeUtility.AddressOf(ref mesh.verts[0]);
			UnsafeUtility.MemCpy(newVerticesPtr, (void*) posData->data,
				dracoMesh->numVertices * elementSize);
			ReleaseDracoData(&posData);
			ReleaseDracoAttribute(&attr);
		}

		// Copy normals.
		if (GetAttributeByUniqueId(dracoMesh, attributes.norms, &attr))
		{
			DracoData* normData = null;
			if (GetAttributeData(dracoMesh, attr, &normData, false))
			{
				elementSize =
					DataTypeSize((DataType) normData->dataType) *
					attr->numComponents;
				mesh.norms = new Vector3[dracoMesh->numVertices];
				var newNormalsPtr = UnsafeUtility.AddressOf(ref mesh.norms[0]);
				UnsafeUtility.MemCpy(newNormalsPtr, (void*) normData->data,
					dracoMesh->numVertices * elementSize);
				ReleaseDracoData(&normData);
				ReleaseDracoAttribute(&attr);
			}
		}

		// Copy texture coordinates.
		if (GetAttributeByUniqueId(dracoMesh, attributes.uv, &attr))
		{
			DracoData* texData = null;
			if (GetAttributeData(dracoMesh, attr, &texData, false))
			{
				elementSize =
					DataTypeSize((DataType) texData->dataType) *
					attr->numComponents;
				mesh.uv = new Vector2[dracoMesh->numVertices];
				var newUVsPtr = UnsafeUtility.AddressOf(ref mesh.uv[0]);
				UnsafeUtility.MemCpy(newUVsPtr, (void*) texData->data,
					dracoMesh->numVertices * elementSize);
				ReleaseDracoData(&texData);
				ReleaseDracoAttribute(&attr);
			}
		}

		// Copy colors.
		if (GetAttributeByUniqueId(dracoMesh, attributes.col, &attr))
		{
			DracoData* colorData = null;
			if (GetAttributeData(dracoMesh, attr, &colorData, false))
			{
				elementSize =
					DataTypeSize((DataType) colorData->dataType) *
					attr->numComponents;
				mesh.colors = new Color[dracoMesh->numVertices];
				var newColorsPtr = UnsafeUtility.AddressOf(ref mesh.colors[0]);
				UnsafeUtility.MemCpy(newColorsPtr, (void*) colorData->data,
					dracoMesh->numVertices * elementSize);
				ReleaseDracoData(&colorData);
				ReleaseDracoAttribute(&attr);
			}
		}

		// Copy weights.
		Vector4[] weights = null;
		if (GetAttributeByUniqueId(dracoMesh, attributes.weights, &attr))
		{
			DracoData* weightData = null;
			if (GetAttributeData(dracoMesh, attr, &weightData, false))
			{
				elementSize =
					DataTypeSize((DataType) weightData->dataType) *
					attr->numComponents;
				if (attr->dataType == 9)
				{
					weights = new Vector4[dracoMesh->numVertices];
					var newWeightsPtr = UnsafeUtility.AddressOf(ref weights[0]);
					UnsafeUtility.MemCpy(newWeightsPtr, (void*) weightData->data,
						dracoMesh->numVertices * elementSize);
				}
				else if (attr->dataType == 4)
				{
					var newWeightsInt = new Vector4<UInt16>[dracoMesh->numVertices];
					var newWeightsPtr = UnsafeUtility.AddressOf(ref newWeightsInt[0]);
					UnsafeUtility.MemCpy(newWeightsPtr, (void*) weightData->data,
						dracoMesh->numVertices * elementSize);
					weights = newWeightsInt.Select(x => new Vector4(x.x, x.y, x.z, x.w)).ToArray();
				}

				ReleaseDracoData(&weightData);
				ReleaseDracoAttribute(&attr);
			}
		}

		// Copy joints.
		Vector4[] joints = null;
		if (GetAttributeByUniqueId(dracoMesh, attributes.joints, &attr))
		{
			DracoData* jointData = null;
			if (GetAttributeData(dracoMesh, attr, &jointData, false))
			{
				elementSize =
					DataTypeSize((DataType) jointData->dataType) *
					attr->numComponents;
				if (attr->dataType == 9)
				{
					joints = new Vector4[dracoMesh->numVertices];
					var newJointsPtr = UnsafeUtility.AddressOf(ref joints[0]);
					UnsafeUtility.MemCpy(newJointsPtr, (void*) jointData->data,
						dracoMesh->numVertices * elementSize);
				}
				else if (attr->dataType == 4)
				{
					var newJointsInt = new Vector4<UInt16>[dracoMesh->numVertices];
					var newJointsPtr = UnsafeUtility.AddressOf(ref newJointsInt[0]);
					UnsafeUtility.MemCpy(newJointsPtr, (void*) jointData->data,
						dracoMesh->numVertices * elementSize);
					joints = newJointsInt.Select(x => new Vector4(x.x, x.y, x.z, x.w)).ToArray();
				}

				ReleaseDracoData(&jointData);
				ReleaseDracoAttribute(&attr);
			}
		}

/* #if UNITY_2017_3_OR_NEWER
		mesh.indexFormat = (newVertices.Length > System.UInt16.MaxValue) ?
			UnityEngine.Rendering.IndexFormat.UInt32 :
			UnityEngine.Rendering.IndexFormat.UInt16;
#else
		if (newVertices.Length > System.UInt16.MaxValue) {
			throw new System.Exception("Draco meshes with more than 65535 vertices are only supported from Unity 2017.3 onwards.");
		}
#endif */

		if (joints != null && weights != null)
		{
			if (joints.Length == weights.Length)
			{
				BoneWeight[] boneWeights = new BoneWeight[weights.Length];
				for (int k = 0; k < boneWeights.Length; k++)
				{
					NormalizeWeights(ref weights[k]);
					boneWeights[k].weight0 = weights[k].x;
					boneWeights[k].weight1 = weights[k].y;
					boneWeights[k].weight2 = weights[k].z;
					boneWeights[k].weight3 = weights[k].w;
					boneWeights[k].boneIndex0 = Mathf.RoundToInt(joints[k].x);
					boneWeights[k].boneIndex1 = Mathf.RoundToInt(joints[k].y);
					boneWeights[k].boneIndex2 = Mathf.RoundToInt(joints[k].z);
					boneWeights[k].boneIndex3 = Mathf.RoundToInt(joints[k].w);
				}

				mesh.boneWeights = boneWeights;
			}
			else Debug.LogWarning("Draco: joints and weights not same length. Skipped");
		}

		return mesh;
	}

	public void NormalizeWeights(ref Vector4 weights)
	{
		float total = weights.x + weights.y + weights.z + weights.w;
		if (total == 0) return;
		float mult = 1f / total;
		weights.x *= mult;
		weights.y *= mult;
		weights.z *= mult;
		weights.w *= mult;
	}

	private int DataTypeSize(DataType dt)
	{
		switch (dt)
		{
			case DataType.DT_INT8:
			case DataType.DT_UINT8:
				return 1;
			case DataType.DT_INT16:
			case DataType.DT_UINT16:
				return 2;
			case DataType.DT_INT32:
			case DataType.DT_UINT32:
				return 4;
			case DataType.DT_INT64:
			case DataType.DT_UINT64:
				return 8;
			case DataType.DT_FLOAT32:
				return 4;
			case DataType.DT_FLOAT64:
				return 8;
			case DataType.DT_BOOL:
				return 1;
			default:
				return -1;
		}
	}
}