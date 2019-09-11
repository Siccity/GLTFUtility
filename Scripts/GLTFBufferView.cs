using System;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#bufferview
    /// <summary> Defines sections within the Buffer </summary>
    public class GLTFBufferView {

        [JsonProperty(Required = Required.Always)] public int buffer;
        [JsonProperty(Required = Required.Always)] public int byteLength;
        public int byteOffset = 0;
        public int? byteStride;
        /// <summary> OpenGL buffer target </summary>
        public int? target;
        public string name;

        public class ImportResult {
            public byte[] bytes;

            public byte[] GetBytes(int byteOffset = 0) {
                if (byteOffset != 0) return bytes.SubArray(byteOffset, bytes.Length - byteOffset);
                else return bytes;
            }
        }

        public ImportResult Import(GLTFBuffer.ImportResult[] buffers) {
            ImportResult result = new ImportResult();
            result.bytes = buffers[buffer].bytes.SubArray(this.byteOffset, byteLength);
            return result;
        }
    }
}