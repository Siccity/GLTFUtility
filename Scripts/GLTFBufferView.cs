using System;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    /// <summary> Defines sections within the Buffer </summary>
    public class GLTFBufferView : GLTFProperty {

#region Serialized fields
        public int buffer = -1;
        public int byteOffset = 0;
        public int byteLength = -1;
        public int byteStride = -1;
        /// <summary> OpenGL buffer target </summary>
        public int target = -1;
#endregion

#region Non-serialized fields
        [JsonIgnore] private byte[] cache;
#endregion

        protected override bool OnLoad() {
            cache = glTFObject.buffers[buffer].GetBytes().SubArray(this.byteOffset, byteLength);
            return true;
        }

        public byte[] GetBytes(int byteOffset = 0) {
            if (byteOffset != 0) return cache.SubArray(byteOffset, byteLength - byteOffset);
            else return cache;
        }
    }
}