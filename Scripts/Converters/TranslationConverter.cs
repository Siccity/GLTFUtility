using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility.Converters {
	/// <summary>
	/// Converts from float array to Vector3 during deserialization, and back.
	/// Compensates for differing coordinate systems as well.
	/// </summary>
	[Preserve] public class TranslationConverter : JsonConverter {
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			Vector3 pos = (Vector3) value;
			// TODO: We need to apply the same kind of left-handed to right-handed
			//       axis flip here during export in the same way as we handle
			//       it during import.
			writer.WriteStartArray();
			writer.WriteValue(pos.x);
			writer.WriteValue(pos.y);
			writer.WriteValue(-pos.z);
			writer.WriteEndArray();
		}

		// We need to convert the system from a right-handed system that GLTF 
		// supports into a left-handed system that Unity supports.  These 'flip'
		// values will enable an X-axis switch.
		static float xFlip = -1.0f;
		static float yFlip =  1.0f;
		static float zFlip =  1.0f;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			float[] floatArray = serializer.Deserialize<float[]>(reader);
			return new Vector3(xFlip * floatArray[0],
					   yFlip * floatArray[1],
					   zFlip * floatArray[2]);
		}

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(Vector3);
		}
	}
}
