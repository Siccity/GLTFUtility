using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#primitive
    public class GLTFPrimitive {
        [JsonProperty(Required = Required.Always)] public GLTFAttributes attributes;
        /// <summary> Rendering mode</summary>
        public RenderingMode mode = RenderingMode.TRIANGLES;
        public int? indices;
        public int? material;
        /// <summary> Morph targets </summary>
        public List<GLTFAttributes> targets;

        public class GLTFAttributes {
            public int POSITION = -1;
            public int NORMAL = -1;
            public int TANGENT = -1;
            public int COLOR_0 = -1;
            public int TEXCOORD_0 = -1;
            public int TEXCOORD_1 = -1;
            public int TEXCOORD_2 = -1;
            public int TEXCOORD_3 = -1;
            public int TEXCOORD_4 = -1;
            public int TEXCOORD_5 = -1;
            public int TEXCOORD_6 = -1;
            public int TEXCOORD_7 = -1;
            public int JOINTS_0 = -1;
            public int JOINTS_1 = -1;
            public int JOINTS_2 = -1;
            public int JOINTS_3 = -1;
            public int WEIGHTS_0 = -1;
            public int WEIGHTS_1 = -1;
            public int WEIGHTS_2 = -1;
            public int WEIGHTS_3 = -1;
        }
    }
}