using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> API used for exporting .gltf and .glb files </summary>
	public static class Exporter {
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Edit/Export Selection/.glb")]
		public static void ExportSelected() {
			ExportGLB(UnityEditor.Selection.activeGameObject);
		}
#endif

		public static void ExportGLB(GameObject root) {
			GLTFObject gltfObject = CreateGLTFObject(root.transform);
			Debug.Log(JsonConvert.SerializeObject(gltfObject, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
		}

		public static void ExportGLTF(GameObject root) {

		}

		public static GLTFObject CreateGLTFObject(Transform root) {
			GLTFObject gltfObject = new GLTFObject();
			gltfObject.scene = 0;
			gltfObject.asset = new GLTFAsset() {
				generator = "GLTFUtility by Siccity https: //github.com/Siccity/GLTFUtility",
					version = "2.0"
			};
			gltfObject.nodes = GLTFNode.CreateNodeList(root);
			return gltfObject;
		}
	}
}