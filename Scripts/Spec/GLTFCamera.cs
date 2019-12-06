using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Shims;
using Siccity.GLTFUtility.Converters;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#camera
	public class GLTFCamera {
		[Preserve] public GLTFCamera() { }

#region Serialization
		public Orthographic orthographic;
		public Perspective perspective;
		[JsonProperty(Required = Required.Always), JsonConverter(typeof(EnumConverter))] public CameraType type;
		public string name;

		public class Orthographic {
			[Preserve] public Orthographic() { }

			[JsonProperty(Required = Required.Always)] public float xmag;
			[JsonProperty(Required = Required.Always)] public float ymag;
			[JsonProperty(Required = Required.Always)] public float zfar;
			[JsonProperty(Required = Required.Always)] public float znear;
		}

		public class Perspective {
			[Preserve] public Perspective() { }

			public float? aspectRatio;
			[JsonProperty(Required = Required.Always)] public float yfov;
			public float? zfar;
			[JsonProperty(Required = Required.Always)] public float znear;
		}
#endregion
	}
}