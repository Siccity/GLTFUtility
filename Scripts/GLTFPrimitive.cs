using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFPrimitive {
        public enum RenderingMode { Points = 1, Lines = 2, Triangles = 3 }
        /// <summary> Rendering mode</summary>
        public RenderingMode mode;
        public int indices = -1;
        public GLTFAttributes attributes;
        public int material = -1;
        /// <summary> Morph targets </summary>
        public List<GLTFAttributes> targets;

        [Serializable]
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