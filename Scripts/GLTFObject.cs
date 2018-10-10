using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, FLOAT = 5126 }

    [Serializable]
    public class GLTFObject {

        /// <summary> Default scene </summary>
        int scene = -1;
        public List<GLTFScene> scenes;
        public List<GLTFNode> nodes;
        public List<GLTFMesh> meshes;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFSkin> skins;
    }
}