using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#primitive
	[Preserve] public class GLTFPrimitive {
		[JsonProperty(Required = Required.Always)] public GLTFAttributes attributes;
		/// <summary> Rendering mode</summary>
		public RenderingMode mode = RenderingMode.TRIANGLES;
		public int? indices;
		public int? material;
		/// <summary> Morph targets </summary>
		public List<GLTFAttributes> targets;
		public Extensions extensions;

		[Preserve] public class GLTFAttributes {
			public int? POSITION;
			public int? NORMAL;
			public int? TANGENT;
			public int? COLOR_0;
			public int? TEXCOORD_0;
			public int? TEXCOORD_1;
			public int? TEXCOORD_2;
			public int? TEXCOORD_3;
			public int? TEXCOORD_4;
			public int? TEXCOORD_5;
			public int? TEXCOORD_6;
			public int? TEXCOORD_7;
			public int? JOINTS_0;
			public int? JOINTS_1;
			public int? JOINTS_2;
			public int? JOINTS_3;
			public int? WEIGHTS_0;
			public int? WEIGHTS_1;
			public int? WEIGHTS_2;
			public int? WEIGHTS_3;
		}

		[Preserve] public class Extensions {
			public DracoMeshCompression KHR_draco_mesh_compression;
		}

		[Preserve] public class DracoMeshCompression {
			public int bufferView = 0;
			public GLTFAttributes attributes;
		}
	}
}