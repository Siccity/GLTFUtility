using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#skin
	[Preserve] public class GLTFSkin {
		/// <summary> Index of accessor containing inverse bind shape matrices </summary>
		public int? inverseBindMatrices;
		public int[] joints;
		public int? skeleton;
		public string name;

		public class ImportResult {
			public Matrix4x4[] inverseBindMatrices;
			public int[] joints;

#region Import
			public SkinnedMeshRenderer SetupSkinnedRenderer(GameObject go, Mesh mesh, GLTFNode.ImportResult[] nodes) {

				SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
				Transform[] bones = new Transform[joints.Length];
				for (int i = 0; i < bones.Length; i++) {
					int jointNodeIndex = joints[i];
					GLTFNode.ImportResult jointNode = nodes[jointNodeIndex];
					bones[i] = jointNode.transform;
					if (string.IsNullOrEmpty(jointNode.transform.name)) jointNode.transform.name = "joint" + i;
				}
				smr.bones = bones;
				smr.rootBone = bones[0];

				// Bindposes
				if (inverseBindMatrices != null) {
					if (inverseBindMatrices.Length != joints.Length) Debug.LogWarning("InverseBindMatrices count and joints count not the same");
					Matrix4x4 m = nodes[0].transform.localToWorldMatrix;
					Matrix4x4[] bindPoses = new Matrix4x4[joints.Length];
					for (int i = 0; i < joints.Length; i++) {
						bindPoses[i] = inverseBindMatrices[i];
					}
					mesh.bindposes = bindPoses;
				} else {
					Matrix4x4 m = nodes[0].transform.localToWorldMatrix;
					Matrix4x4[] bindPoses = new Matrix4x4[joints.Length];
					for (int i = 0; i < joints.Length; i++) {
						bindPoses[i] = nodes[joints[i]].transform.worldToLocalMatrix * m;
					}
					mesh.bindposes = bindPoses;
				}
				smr.sharedMesh = mesh;
				return smr;
			}
		}

		public ImportResult Import(GLTFAccessor.ImportResult[] accessors) {
			ImportResult result = new ImportResult();
			result.joints = joints;

			// Inverse bind matrices
			if (inverseBindMatrices.HasValue) {
				result.inverseBindMatrices = accessors[inverseBindMatrices.Value].ReadMatrix4x4();
				for (int i = 0; i < result.inverseBindMatrices.Length; i++) {
					// Flip the matrix from GLTF to Unity format, which
					// is a left-handed coordinate system (not the right-
					// handed system that GLTF uses).  This basically means
					// that we have to flip everything about the X axis.
					Matrix4x4 m = result.inverseBindMatrices[i];
					Vector3 scale = new Vector3(-1, 1, 1);
					Matrix4x4 convert = Matrix4x4.Scale(scale);
					Matrix4x4 m2 = convert * m * convert;
					result.inverseBindMatrices[i] = m2.transpose;
				}
			}
			return result;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFSkin> skins, GLTFAccessor.ImportTask accessorTask) : base(accessorTask) {
				task = new Task(() => {
					if (skins == null) return;

					Result = new ImportResult[skins.Count];
					for (int i = 0; i < Result.Length; i++) {
						Result[i] = skins[i].Import(accessorTask.Result);
					}
				});
			}
		}
#endregion
	}
}
