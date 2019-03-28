using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Defines sections within the Buffer </summary>
    [Serializable]
    public class GLTFBufferView {
        public int buffer = -1;
        public int byteOffset = 0;
        public int byteLength = -1;
        public int byteStride = -1;
        /// <summary> OpenGL buffer target </summary>
        public int target = -1;

        private byte[] cache;

        public byte[] GetBytes(GLTFObject gLTFObject, int byteOffset = 0) {
            if (cache == null) cache = gLTFObject.buffers[buffer].GetBytes().SubArray(this.byteOffset, byteLength);
            if (byteOffset != 0) return cache.SubArray(byteOffset, byteLength - byteOffset);
            else return cache;
        }
    }
}