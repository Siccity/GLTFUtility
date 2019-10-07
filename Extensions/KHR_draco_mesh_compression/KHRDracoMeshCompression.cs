using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility.Extensions {
	[Extension("KHR_draco_mesh_compression")]
	public class KHRDracoMeshCompression : IExtension {
		public int bufferView;
		public GLTFPrimitive.GLTFAttributes attributes; 

		public void ReadJson(JsonReader reader, JsonSerializer serializer) {
			Debug.Log("reader");
		}

		public void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			throw new System.NotImplementedException();
		}
	}
}