using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility.Converters {
	/// <summary>
	/// Converts from float array to Quaternion during deserialization, and back.
	/// Compensates for differing coordinate systems as well.
	/// </summary>
	[Preserve] public class QuaternionConverter : JsonConverter {
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			Quaternion q = (Quaternion) value;
			// TODO: We need to apply a left-handed to right-handed axis
			//       flip here during export in the same way as we handle
			//       it during import.
			writer.WriteStartArray();
			writer.WriteValue(q.x);
			writer.WriteValue(q.y);
			writer.WriteValue(-q.z);
			writer.WriteValue(-q.w);
			writer.WriteEndArray();
		}

		// We need to convert the system from a right-handed system that GLTF
		// supports into a left-handed system that Unity supports.  These
		// 'flip' values will enable an X-axis switch.
		static float xFlip =  1.0f;
		static float yFlip = -1.0f;
		static float zFlip = -1.0f;
		static float wFlip =  1.0f;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			float[] floatArray = serializer.Deserialize<float[]>(reader);
			return new Quaternion(	xFlip * floatArray[0],
						yFlip * floatArray[1],
						zFlip * floatArray[2],
						wFlip * floatArray[3]);
		}

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(Quaternion);
		}
	}
}
