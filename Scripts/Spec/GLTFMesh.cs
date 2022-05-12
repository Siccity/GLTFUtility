using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#mesh
	[Preserve] public class GLTFMesh {
#region Serialization
		[JsonProperty(Required = Required.Always)] public List<GLTFPrimitive> primitives;
		/// <summary> Morph target weights </summary>
		public List<float> weights;
		public string name;
		public Extras extras;

		public class Extras {
			/// <summary>
			/// Morph target names. Not part of the official spec, but pretty much a standard.
			/// Discussed here https://github.com/KhronosGroup/glTF/issues/1036
			/// </summary>
			public string[] targetNames;
		}
#endregion

#region Import
		public class ImportResult {
			public Material[] materials;
			public Mesh mesh;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			private class TaskMeshData {

				string name;
				List<Vector3> normals = new List<Vector3>();
				List<List<int>> submeshTris = new List<List<int>>();
				List<RenderingMode> submeshTrisMode = new List<RenderingMode>();
				List<Vector3> verts = new List<Vector3>();
				List<Vector4> tangents = new List<Vector4>();
				List<Color> colors = new List<Color>();
				List<BoneWeight> weights = null;
				List<Vector2> uv1 = null;
				List<Vector2> uv2 = null;
				List<Vector2> uv3 = null;
				List<Vector2> uv4 = null;
				List<Vector2> uv5 = null;
				List<Vector2> uv6 = null;
				List<Vector2> uv7 = null;
				List<Vector2> uv8 = null;
				List<BlendShape> blendShapes = new List<BlendShape>();
				List<int> submeshVertexStart = new List<int>();

				private class BlendShape {
					public string name;
					public Vector3[] pos, norm, tan;
				}

				public TaskMeshData(GLTFMesh gltfMesh, GLTFAccessor.ImportResult[] accessors, GLTFBufferView.ImportResult[] bufferViews) {
					name = gltfMesh.name;
					if (gltfMesh.primitives.Count == 0) {
						Debug.LogWarning("0 primitives in mesh");
					} else {
						for (int i = 0; i < gltfMesh.primitives.Count; i++) {
							GLTFPrimitive primitive = gltfMesh.primitives[i];
							// Load draco mesh
							if (primitive.extensions != null && primitive.extensions.KHR_draco_mesh_compression != null) {
								GLTFPrimitive.DracoMeshCompression draco = primitive.extensions.KHR_draco_mesh_compression;
								GLTFBufferView.ImportResult bufferView = bufferViews[draco.bufferView];
								GLTFUtilityDracoLoader loader = new GLTFUtilityDracoLoader();
								byte[] buffer = new byte[bufferView.byteLength];
								bufferView.stream.Seek(bufferView.byteOffset, System.IO.SeekOrigin.Begin);

								bufferView.stream.Read(buffer, 0, bufferView.byteLength);

								GLTFUtilityDracoLoader.MeshAttributes attribs = new GLTFUtilityDracoLoader.MeshAttributes(
									primitive.extensions.KHR_draco_mesh_compression.attributes.POSITION ?? -1,
									primitive.extensions.KHR_draco_mesh_compression.attributes.NORMAL ?? -1,
									primitive.extensions.KHR_draco_mesh_compression.attributes.TEXCOORD_0 ?? -1,
									primitive.extensions.KHR_draco_mesh_compression.attributes.JOINTS_0 ?? -1,
									primitive.extensions.KHR_draco_mesh_compression.attributes.WEIGHTS_0 ?? -1,
									primitive.extensions.KHR_draco_mesh_compression.attributes.COLOR_0 ?? -1
								);

								//Mesh mesh = loader.LoadMesh(buffer, attribs);

								GLTFUtilityDracoLoader.AsyncMesh asyncMesh = loader.LoadMesh(buffer, attribs);
								if (asyncMesh == null) Debug.LogWarning("Draco mesh couldn't be loaded");

								submeshTrisMode.Add(primitive.mode);

								// Tris
								int vertCount = verts.Count();
								submeshTris.Add(asyncMesh.tris.Reverse().Select(x => x + vertCount).ToList());

								verts.AddRange(asyncMesh.verts.Select(x => new Vector3(-x.x, x.y, x.z)));

								if (asyncMesh.norms != null) {
									normals.AddRange(asyncMesh.norms.Select(v => { v.x = -v.x; return v; }));
								}
								//tangents.AddRange(asyncMesh.tangents.Select(v => { v.y = -v.y; v.z = -v.z; return v; }));

								// Weights
								if (asyncMesh.boneWeights != null) {
									if (weights == null) weights = new List<BoneWeight>();
									weights.AddRange(asyncMesh.boneWeights);
								}

								// BlendShapes not supported yet
								/* for (int k = 0; k < mesh.blendShapeCount; k++) {
									int frameCount = mesh.GetBlendShapeFrameCount(k);
									BlendShape blendShape = new BlendShape();
									blendShape.pos = new Vector3[frameCount];
									blendShape.norm = new Vector3[frameCount];
									blendShape.tan = new Vector3[frameCount];
									for (int o = 0; o < frameCount; o++) {
										mesh.GetBlendShapeFrameVertices(k, o, blendShape.pos, blendShape.norm, blendShape.tan);
									}
									blendShapes.Add(blendShape);
								} */

								// UVs
								if (asyncMesh.uv != null) {
									if (uv1 == null) uv1 = new List<Vector2>();
									uv1.AddRange(asyncMesh.uv.Select(x => new Vector2(x.x, -x.y)));
								}
							}
							// Load normal mesh
							else {
								int vertStartIndex = verts.Count;
								submeshVertexStart.Add(vertStartIndex);

								// Verts - (X points left in GLTF)
								if (primitive.attributes.POSITION.HasValue) {
									IEnumerable<Vector3> newVerts = accessors[primitive.attributes.POSITION.Value].ReadVec3(true).Select(v => { v.x = -v.x; return v; });
									verts.AddRange(newVerts);
								}

								int vertCount = verts.Count;

								// Tris - (Invert all triangles. Instead of flipping each triangle, just flip the entire array. Much easier)
								if (primitive.indices.HasValue) {
									submeshTris.Add(new List<int>(accessors[primitive.indices.Value].ReadInt().Reverse().Select(x => x + vertStartIndex)));
									submeshTrisMode.Add(primitive.mode);
								}

								/// Normals - (X points left in GLTF)
								if (primitive.attributes.NORMAL.HasValue) {
									normals.AddRange(accessors[primitive.attributes.NORMAL.Value].ReadVec3(true).Select(v => { v.x = -v.x; return v; }));
								}

								// Tangents - (X points left in GLTF)
								if (primitive.attributes.TANGENT.HasValue) {
									tangents.AddRange(accessors[primitive.attributes.TANGENT.Value].ReadVec4(true).Select(v => { v.y = -v.y; v.z = -v.z; return v; }));
								}

								// Vertex colors
								if (primitive.attributes.COLOR_0.HasValue) {
									colors.AddRange(accessors[primitive.attributes.COLOR_0.Value].ReadColor());
								}

								// Weights
								if (primitive.attributes.WEIGHTS_0.HasValue && primitive.attributes.JOINTS_0.HasValue) {
									Vector4[] weights0 = accessors[primitive.attributes.WEIGHTS_0.Value].ReadVec4(true);
									Vector4[] joints0 = accessors[primitive.attributes.JOINTS_0.Value].ReadVec4();
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
										if (weights == null) weights = new List<BoneWeight>(new BoneWeight[vertCount - boneWeights.Length]);
										weights.AddRange(boneWeights);
									} else Debug.LogWarning("WEIGHTS_0 and JOINTS_0 not same length. Skipped");
								} else {
									if (weights != null) weights.AddRange(new BoneWeight[vertCount - weights.Count]);
								}

								// UVs
								ReadUVs(ref uv1, accessors, primitive.attributes.TEXCOORD_0, vertCount);
								ReadUVs(ref uv2, accessors, primitive.attributes.TEXCOORD_1, vertCount);
								ReadUVs(ref uv3, accessors, primitive.attributes.TEXCOORD_2, vertCount);
								ReadUVs(ref uv4, accessors, primitive.attributes.TEXCOORD_3, vertCount);
								ReadUVs(ref uv5, accessors, primitive.attributes.TEXCOORD_4, vertCount);
								ReadUVs(ref uv6, accessors, primitive.attributes.TEXCOORD_5, vertCount);
								ReadUVs(ref uv7, accessors, primitive.attributes.TEXCOORD_6, vertCount);
								ReadUVs(ref uv8, accessors, primitive.attributes.TEXCOORD_7, vertCount);
							}
						}

						bool hasTargetNames = gltfMesh.extras != null && gltfMesh.extras.targetNames != null;
						if (hasTargetNames) {
							if (gltfMesh.primitives.All(x => x.targets.Count != gltfMesh.extras.targetNames.Length)) {
								Debug.LogWarning("Morph target names found in mesh " + name + " but array length does not match primitive morph target array length");
								hasTargetNames = false;
							}
						}
						// Read blend shapes after knowing final vertex count
						int finalVertCount = verts.Count;

						for (int i = 0; i < gltfMesh.primitives.Count; i++) {
							GLTFPrimitive primitive = gltfMesh.primitives[i];
							if (primitive.targets != null) {
								for (int k = 0; k < primitive.targets.Count; k++) {
									BlendShape blendShape = new BlendShape();
									blendShape.pos = GetMorphWeights(primitive.targets[k].POSITION, submeshVertexStart[i], finalVertCount, accessors);
									blendShape.norm = GetMorphWeights(primitive.targets[k].NORMAL, submeshVertexStart[i], finalVertCount, accessors);
									blendShape.tan = GetMorphWeights(primitive.targets[k].TANGENT, submeshVertexStart[i], finalVertCount, accessors);
									if (hasTargetNames) blendShape.name = gltfMesh.extras.targetNames[k];
									else blendShape.name = "morph-" + blendShapes.Count;
									blendShapes.Add(blendShape);
								}
							}
						}
					}
				}

				private Vector3[] GetMorphWeights(int? accessor, int vertStartIndex, int vertCount, GLTFAccessor.ImportResult[] accessors) {
					if (accessor.HasValue) {
						if (accessors[accessor.Value] == null) {
							Debug.LogWarning("Accessor is null");
							return new Vector3[vertCount];
						}
						Vector3[] accessorData = accessors[accessor.Value].ReadVec3(true).Select(v => { v.x = -v.x; return v; }).ToArray();
						if (accessorData.Length != vertCount) {
							Vector3[] resized = new Vector3[vertCount];
							Array.Copy(accessorData, 0, resized, vertStartIndex, accessorData.Length);
							return resized;
						} else return accessorData;
					} else return new Vector3[vertCount];
				}

				public Mesh ToMesh() {
					Mesh mesh = new Mesh();
					if (verts.Count >= ushort.MaxValue) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
					mesh.vertices = verts.ToArray();
					mesh.subMeshCount = submeshTris.Count;
					var onlyTriangles = true;
					for (int i = 0; i < submeshTris.Count; i++) {
						switch (submeshTrisMode[i]) {
							case RenderingMode.POINTS:
								mesh.SetIndices(submeshTris[i].ToArray(), MeshTopology.Points, i);
								onlyTriangles = false;
								break;
							case RenderingMode.LINES:
								mesh.SetIndices(submeshTris[i].ToArray(), MeshTopology.Lines, i);
								onlyTriangles = false;
								break;
							case RenderingMode.LINE_STRIP:
								mesh.SetIndices(submeshTris[i].ToArray(), MeshTopology.LineStrip, i);
								onlyTriangles = false;
								break;
							case RenderingMode.TRIANGLES:
								mesh.SetTriangles(submeshTris[i].ToArray(), i);
								break;
							default:
								Debug.LogWarning("GLTF rendering mode " + submeshTrisMode[i] + " not supported.");
								return null;
						}
					}

					mesh.colors = colors.ToArray();
					if (uv1 != null) mesh.uv = uv1.ToArray();
					if (uv2 != null) mesh.uv2 = uv2.ToArray();
					if (uv3 != null) mesh.uv3 = uv3.ToArray();
					if (uv4 != null) mesh.uv4 = uv4.ToArray();
					if (uv5 != null) mesh.uv5 = uv5.ToArray();
					if (uv6 != null) mesh.uv6 = uv6.ToArray();
					if (uv7 != null) mesh.uv7 = uv7.ToArray();
					if (uv8 != null) mesh.uv8 = uv8.ToArray();
					if (weights != null) mesh.boneWeights = weights.ToArray();

					mesh.RecalculateBounds();

					// Blend shapes
					for (int i = 0; i < blendShapes.Count; i++) {
						mesh.AddBlendShapeFrame(blendShapes[i].name, 1f, blendShapes[i].pos, blendShapes[i].norm, blendShapes[i].tan);
					}

					if (normals.Count == 0 && onlyTriangles)
						mesh.RecalculateNormals();
					else
						mesh.normals = normals.ToArray();

					if (tangents.Count == 0 && onlyTriangles)
						mesh.RecalculateTangents();
					else
						mesh.tangents = tangents.ToArray();

					mesh.name = name;
					return mesh;
				}

                public Jobs.MeshCreationJob CreateJob(Mesh.MeshData meshData, out Mesh mesh)
                {
                    Jobs.MeshCreationJob job = new Jobs.MeshCreationJob()
                    {
                        meshData = meshData
                    };
                    int streamIndex = 0;
                    NativeList<VertexAttributeDescriptor> descriptor = new NativeList<VertexAttributeDescriptor>(Allocator.TempJob);

                    NativeArray<Vector3> managedVerts = new NativeArray<Vector3>(verts.ToArray(), Allocator.TempJob);
                    job.vertices = managedVerts.Reinterpret<float3>();
                    descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.Position, stream: streamIndex++));

                    int subMeshCount = submeshTris.Count;
                    NativeArray<int> indicesStartIndex = new NativeArray<int>(subMeshCount, Allocator.TempJob);
                    NativeArray<int> indicesSubMeshLength = new NativeArray<int>(subMeshCount, Allocator.TempJob);
                    NativeArray<MeshTopology> meshTopologies = new NativeArray<MeshTopology>(subMeshCount, Allocator.TempJob);
                    meshData.subMeshCount = subMeshCount;
                    int subMeshIndex = 0;
                    List<int> managedIndices = new List<int>();
					for (int i = 0; i < submeshTris.Count; i++) {
						switch (submeshTrisMode[i]) {
							case RenderingMode.POINTS:
                                meshTopologies[i] = MeshTopology.Points;
								break;
							case RenderingMode.LINES:
                                meshTopologies[i] = MeshTopology.Lines;
								break;
							case RenderingMode.LINE_STRIP:
                                meshTopologies[i] = MeshTopology.LineStrip;
								break;
							case RenderingMode.TRIANGLES:
                                meshTopologies[i] = MeshTopology.Triangles;
								break;
						}
                        managedIndices.AddRange(submeshTris[i]);
                        indicesStartIndex[i] = subMeshIndex;
                        indicesSubMeshLength[i] = submeshTris[i].Count;
                        subMeshIndex += submeshTris[i].Count;
					}

                    job.indices = new NativeArray<int>(managedIndices.ToArray(), Allocator.TempJob);
                    job.indicesStartIndex = indicesStartIndex;
                    job.indicesSubMeshLength = indicesSubMeshLength;
                    job.meshTopologies = meshTopologies;

                    if (normals.Count > 0)
                    {
                        NativeArray<Vector3> managedNormals = new NativeArray<Vector3>(normals.ToArray(), Allocator.TempJob);
                        job.normals = managedNormals.Reinterpret<float3>(); 
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.Normal, stream: streamIndex++));
                    }
                    else
                    {
                        job.normals = new NativeArray<float3>(0, Allocator.TempJob);
                    }

                    if (tangents.Count > 0)
                    {
                        NativeArray<Vector4> managedTangents = new NativeArray<Vector4>(tangents.ToArray(), Allocator.TempJob);
                        job.tangents = managedTangents.Reinterpret<float4>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4, stream: streamIndex++));
                    }
                    else
                    {
                        job.tangents = new NativeArray<float4>(0, Allocator.TempJob);
                    }

                    if(colors.Count > 0)
                    {
                        job.colors = new NativeArray<Color>(colors.ToArray(), Allocator.TempJob);
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.Color, stream: streamIndex++));
                    }
                    else
                    {
                        job.colors = new NativeArray<Color>(0, Allocator.TempJob);
                    }

                    if (uv1 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv1.ToArray(), Allocator.TempJob);
                        job.uv1 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv1 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv2 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv2.ToArray(), Allocator.TempJob);
                        job.uv2 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord1, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv2 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv3 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv3.ToArray(), Allocator.TempJob);
                        job.uv3 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord2, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv3 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv4 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv4.ToArray(), Allocator.TempJob);
                        job.uv4 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord3, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv4 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv5 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv5.ToArray(), Allocator.TempJob);
                        job.uv5 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord4, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv5 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv6 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv6.ToArray(), Allocator.TempJob);
                        job.uv6 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord5, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv6 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv7 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv7.ToArray(), Allocator.TempJob);
                        job.uv7 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord6, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv7 = new NativeArray<float2>(0, Allocator.TempJob);
                    }
                    if (uv8 != null)
                    {
                        NativeArray<Vector2> managedUVs = new NativeArray<Vector2>(uv8.ToArray(), Allocator.TempJob);
                        job.uv8 = managedUVs.Reinterpret<float2>();
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord7, dimension: 2, stream: streamIndex++));
                    }
                    else
                    {
                        job.uv8 = new NativeArray<float2>(0, Allocator.TempJob);
                    }

                    if (weights != null)
                    {
                        job.weights = new NativeArray<BoneWeight>(weights.ToArray(), Allocator.TempJob);
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.BlendWeight, dimension: 4, stream: streamIndex++));
                        descriptor.Add(new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.SInt32, 4, streamIndex++));
                    }
                    else
                    {
                        job.weights = new NativeArray<BoneWeight>(0, Allocator.TempJob);
                    }

                    int vertexCount = job.vertices.Length;
                    IndexFormat format = vertexCount >= ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
                    meshData.SetVertexBufferParams(vertexCount, descriptor.ToArray());
                    meshData.SetIndexBufferParams(subMeshIndex, format);
                    descriptor.Dispose();

                    mesh = new Mesh();
                    mesh.name = name;

                    return job;
                }

                public void JobPostProcessing(Mesh mesh)
                {
					bool onlyTriangles = true;
					for (int i = 0; i < submeshTris.Count; i++) {
						switch (submeshTrisMode[i]) {
							case RenderingMode.POINTS:
							case RenderingMode.LINES:
							case RenderingMode.LINE_STRIP:
								onlyTriangles = false;
								break;
						}
					}

					mesh.RecalculateBounds();

					// Blend shapes
					for (int i = 0; i < blendShapes.Count; i++) {
						mesh.AddBlendShapeFrame(blendShapes[i].name, 1f, blendShapes[i].pos, blendShapes[i].norm, blendShapes[i].tan);
					}

					if (normals.Count == 0 && onlyTriangles)
						mesh.RecalculateNormals();

					if (tangents.Count == 0 && onlyTriangles)
						mesh.RecalculateTangents();
                }

                public void NormalizeWeights(ref Vector4 weights) {
					float total = weights.x + weights.y + weights.z + weights.w;
					float mult = 1f / total;
					weights.x *= mult;
					weights.y *= mult;
					weights.z *= mult;
					weights.w *= mult;
				}

				private void ReadUVs(ref List<Vector2> uvs, GLTFAccessor.ImportResult[] accessors, int? texcoord, int vertCount) {
					// If there are no valid texcoords
					if (!texcoord.HasValue) {
						// If there are already uvs, add some empty filler uvs so it still matches the vertex array
						if (uvs != null) uvs.AddRange(new Vector2[vertCount - uvs.Count]);
						return;
					}
					Vector2[] _uvs = accessors[texcoord.Value].ReadVec2(true);
					FlipY(ref _uvs);
					if (uvs == null) uvs = new List<Vector2>(_uvs);
					else uvs.AddRange(_uvs);
				}

				public void FlipY(ref Vector2[] uv) {
					for (int i = 0; i < uv.Length; i++) {
						uv[i].y = 1 - uv[i].y;
					}
				}
			}

			private TaskMeshData[] meshData;
			private List<GLTFMesh> meshes;
			private GLTFMaterial.ImportTask materialTask;
			private bool asyncMeshCreation;

			public ImportTask(List<GLTFMesh> meshes, GLTFAccessor.ImportTask accessorTask, GLTFBufferView.ImportTask bufferViewTask, GLTFMaterial.ImportTask materialTask, ImportSettings importSettings) : base(accessorTask, materialTask) {
				this.meshes = meshes;
				this.materialTask = materialTask;
				this.asyncMeshCreation = importSettings.asyncMeshCreation;

				task = new Task(() => {
					if (meshes == null) return;

					meshData = new TaskMeshData[meshes.Count];
					for (int i = 0; i < meshData.Length; i++) {
						meshData[i] = new TaskMeshData(meshes[i], accessorTask.Result, bufferViewTask.Result);
					}
				});
			}

			public override IEnumerator OnCoroutine(Action<float> onProgress = null) {
				// No mesh
				if (meshData == null) {
					if (onProgress != null) onProgress.Invoke(1f);
					IsCompleted = true;
					yield break;
				}

				Result = new ImportResult[meshData.Length];
				if (asyncMeshCreation)
				{
					int meshCount = 0;
					for (int i = 0; i < meshData.Length; i++)
					{
						if(meshData[i] == null)
						{
							continue;
						}

						meshCount++;
					}

                    Mesh[] meshes = new Mesh[meshCount];
                    NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(meshCount, Allocator.Persistent);
					Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(meshCount);
                    meshCount = 0;
					for (int i = 0; i < meshData.Length; i++)
					{
						if(meshData[i] == null)
						{
							continue;
						}

                        Jobs.MeshCreationJob job = meshData[i].CreateJob(meshDataArray[i], out Mesh mesh);
                        meshes[i] = mesh;
                        jobHandles[meshCount] = job.Schedule();
						meshCount++;
					}

                    bool jobComplete = false;
                    while(!jobComplete)
                    {
                        yield return null;
                        jobComplete = true;
                        for (int i = 0; i < jobHandles.Length; i++)
                        {
                            if(!jobHandles[i].IsCompleted)
                            {
                                jobComplete = false;
                            }
                        }
                    }
                    JobHandle.CompleteAll(jobHandles);
                    jobHandles.Dispose();

                    Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes);

                    meshCount = 0;
					for (int i = 0; i < meshData.Length; i++)
					{
						if(meshData[i] == null)
						{
							continue;
						}

                        meshData[i].JobPostProcessing(meshes[meshCount]);
                        Result[i] = new ImportResult();
                        Result[i].mesh = meshes[meshCount];
						meshCount++;
					}
				}

				for (int i = 0; i < meshData.Length; i++) {
					if (meshData[i] == null) {
						Debug.LogWarning("Mesh " + i + " import error");
						continue;
					}

					if (!asyncMeshCreation)
					{
                        Result[i] = new ImportResult();
                        Result[i].mesh = meshData[i].ToMesh();
					}
					Result[i].materials = new Material[meshes[i].primitives.Count];
					for (int k = 0; k < meshes[i].primitives.Count; k++) {
						int? matIndex = meshes[i].primitives[k].material;
						if (matIndex.HasValue && materialTask.Result != null && materialTask.Result.Count() > matIndex.Value) {
							GLTFMaterial.ImportResult matImport = materialTask.Result[matIndex.Value];
							if (matImport != null) Result[i].materials[k] = matImport.material;
							else {
								Debug.LogWarning("Mesh[" + i + "].matIndex points to null material (index " + matIndex.Value + ")");
								Result[i].materials[k] = GLTFMaterial.defaultMaterial;
							}
						} else {
							Result[i].materials[k] = GLTFMaterial.defaultMaterial;
						}
					}
					if (string.IsNullOrEmpty(Result[i].mesh.name)) Result[i].mesh.name = "mesh" + i;
					if (onProgress != null) onProgress.Invoke((float) (i + 1) / (float) meshData.Length);
					yield return null;
				}
				IsCompleted = true;
			}
		}
#endregion

#region Export
		public class ExportResult : GLTFMesh {
			[JsonIgnore] public Mesh mesh;
		}

		public static List<ExportResult> Export(List<GLTFNode.ExportResult> nodes) {
			List<ExportResult> results = new List<ExportResult>();
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i].filter) {
					Mesh mesh = nodes[i].filter.sharedMesh;
					if (mesh) {
						nodes[i].mesh = results.Count;
						results.Add(Export(mesh));
					}
				}
			}
			return results;
		}

		public static ExportResult Export(Mesh mesh) {
			ExportResult result = new ExportResult();
			result.name = mesh.name;
			result.primitives = new List<GLTFPrimitive>();
			for (int i = 0; i < mesh.subMeshCount; i++) {
				GLTFPrimitive primitive = new GLTFPrimitive();
				result.primitives.Add(primitive);
			}
			return result;
		}
#endregion
	}
}