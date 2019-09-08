using System;
using System.IO;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#buffer
    /// <summary> Contains raw binary data </summary>
    public class GLTFBuffer : GLTFProperty {

#region Serialized fields
        [JsonProperty(Required = Required.Always)] public int byteLength;
        public string uri;
        public string name;
#endregion

#region Non-serialized fields
        [JsonIgnore] private const string embeddedPrefix = "data:application/octet-stream;base64,";
        [JsonIgnore] private byte[] cache;
        [JsonIgnore] public bool isEmbedded { get { return CheckEmbedded(); } }
#endregion

        protected override bool OnLoad() {
            if (string.IsNullOrEmpty(uri)) {
                cache = File.ReadAllBytes(glTFObject.directoryRoot + glTFObject.mainFile);
            } else if (!isEmbedded) {
                cache = File.ReadAllBytes(glTFObject.directoryRoot + uri);
            } else {
                string b64 = uri.Substring(embeddedPrefix.Length, uri.Length - embeddedPrefix.Length);
                cache = Convert.FromBase64String(b64);
            }

            // Sometimes the buffer is part of a larger file. Since we dont have a byteOffset we have to assume it's at the end of the file.
            // In case you're trying to load a gltf with more than one buffers this might cause issues, but it'll work for now.
            int startIndex = cache.Length - byteLength;
            if (startIndex != 0) cache = cache.SubArray(startIndex, byteLength);
            return true;
        }

        public byte[] GetBytes() {
            return cache;
        }

        private bool CheckEmbedded() {
            if (uri.Length < embeddedPrefix.Length) {
                return false;
            }
            if (uri.Substring(0, embeddedPrefix.Length) != embeddedPrefix) {
                return false;
            }
            return true;
        }
    }
}