using System;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#bufferview
    /// <summary> Defines sections within the Buffer </summary>
    public class GLTFBufferView : GLTFProperty {

#region Serialized fields
        [JsonProperty(Required = Required.Always)] public int buffer;
        [JsonProperty(Required = Required.Always)] public int byteLength;
        public int byteOffset = 0;
        public int? byteStride;
        /// <summary> OpenGL buffer target </summary>
        public int? target;
        public string name;
#endregion

#region Non-serialized fields
        [JsonIgnore] private byte[] cache;
#endregion

        public byte[] LoadBytes(byte[][] buffers) {
            return buffers[buffer].SubArray(this.byteOffset, byteLength);
        }

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