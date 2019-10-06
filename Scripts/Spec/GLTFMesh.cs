using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#mesh
	public class GLTFMesh {

#region Serialization
		[JsonProperty(Required = Required.Always)] public List<GLTFPrimitive> primitives;
		/// <summary> Morph target weights </summary>
		public List<float> weights;
		public string name;
#endregion

#region Import
		public class ImportResult {
			public Material[] materials;
			public Mesh mesh;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			private class MeshData {
				string name;
				List<Vector3> normals = new List<Vector3>();
				List<List<int>> submeshTris = new List<List<int>>();
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

				public MeshData(GLTFMesh gltfMesh, GLTFAccessor.ImportResult[] accessors) {
					name = gltfMesh.name;
					if (gltfMesh.primitives.Count == 0) {
						Debug.LogWarning("0 primitives in mesh");
					} else {
						for (int i = 0; i < gltfMesh.primitives.Count; i++) {
							GLTFPrimitive primitive = gltfMesh.primitives[i];

							int vertStartIndex = verts.Count;

							// Verts - (Z points backwards in GLTF)
							if (primitive.attributes.POSITION.HasValue) {
								IEnumerable<Vector3> newVerts = accessors[primitive.attributes.POSITION.Value].ReadVec3().Select(v => { v.z = -v.z; return v; });
								verts.AddRange(newVerts);
							}

							int vertCount = verts.Count;

							// Tris - (Invert all triangles. Instead of flipping each triangle, just flip the entire array. Much easier)
							if (primitive.indices.HasValue) {
								submeshTris.Add(new List<int>(accessors[primitive.indices.Value].ReadInt().Reverse().Select(x => x + vertStartIndex)));
							}

							/// Normals - (Z points backwards in GLTF)
							if (primitive.attributes.NORMAL.HasValue) {
								normals.AddRange(accessors[primitive.attributes.NORMAL.Value].ReadVec3().Select(v => { v.z = -v.z; return v; }));
							}

							// Tangents - (Z points backwards in GLTF)
							if (primitive.attributes.TANGENT.HasValue) {
								tangents.AddRange(accessors[primitive.attributes.TANGENT.Value].ReadVec4().Select(v => { v.z = -v.z; v.w = -v.w; return v; }));
							}

							// Vertex colors
							if (primitive.attributes.COLOR_0.HasValue) {
								colors.AddRange(accessors[primitive.attributes.COLOR_0.Value].ReadColor());
							}

							// Weights
							if (primitive.attributes.WEIGHTS_0.HasValue && primitive.attributes.JOINTS_0.HasValue) {
								Vector4[] weights0 = accessors[primitive.attributes.WEIGHTS_0.Value].ReadVec4();
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
				}

				public Mesh ToMesh() {
					Mesh mesh = new Mesh();;
					mesh.vertices = verts.ToArray();
					mesh.subMeshCount = submeshTris.Count;
					for (int i = 0; i < submeshTris.Count; i++) {
						mesh.SetTriangles(submeshTris[i].ToArray(), i);
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

					if (normals.Count == 0) mesh.RecalculateNormals();
					else mesh.normals = normals.ToArray();

					if (tangents.Count == 0) mesh.RecalculateTangents();
					else mesh.tangents = tangents.ToArray();

					mesh.name = name;
					return mesh;
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
					Vector2[] _uvs = accessors[texcoord.Value].ReadVec2();
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

			private MeshData[] meshData;
			private List<GLTFMesh> meshes;
			private GLTFMaterial.ImportTask materialTask;

			public ImportTask(List<GLTFMesh> meshes, GLTFAccessor.ImportTask accessorTask, GLTFMaterial.ImportTask materialTask, ImportSettings importSettings) : base(accessorTask, materialTask) {
				this.meshes = meshes;
				this.materialTask = materialTask;

				task = new Task(() => {
					if (meshes == null) return;

					meshData = new MeshData[meshes.Count];
					for (int i = 0; i < meshData.Length; i++) {
						meshData[i] = new MeshData(meshes[i], accessorTask.Result);
					}
				});
			}

			protected override void OnMainThreadFinalize() {
				Result = new ImportResult[meshData.Length];
				for (int i = 0; i < meshData.Length; i++) {
					if (meshData[i] == null) {
						Debug.LogWarning("Draco mesh not supported");
						continue;
					}

					Result[i] = new ImportResult();
					Result[i].mesh = meshData[i].ToMesh();
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
				}
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