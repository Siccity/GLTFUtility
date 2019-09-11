using System;
using System.IO;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#buffer
    /// <summary> Contains raw binary data </summary>
    public class GLTFBuffer : GLTFProperty {

        [JsonProperty(Required = Required.Always)] public int byteLength;
        public string uri;
        public string name;

        [JsonIgnore] private const string embeddedPrefix = "data:application/octet-stream;base64,";
        [JsonIgnore] private byte[] cache;

        public byte[] LoadBytes(string filepath) {
            byte[] bytes;
            if (uri == null) {
                // Load entire file
                bytes = File.ReadAllBytes(filepath);
            } else if (uri.StartsWith(embeddedPrefix)) {
                // Load embedded 
                string b64 = uri.Substring(embeddedPrefix.Length, uri.Length - embeddedPrefix.Length);
                bytes = Convert.FromBase64String(b64);
            } else {
                // Load URI
			    string directoryRoot = Directory.GetParent(filepath).ToString() + "/";
                bytes = File.ReadAllBytes(directoryRoot + uri);
            }

            // Sometimes the buffer is part of a larger file. Since we dont have a byteOffset we have to assume it's at the end of the file.
            // In case you're trying to load a gltf with more than one buffers this might cause issues, but it'll work for now.
            int startIndex = bytes.Length - byteLength;
            if (startIndex != 0) bytes = bytes.SubArray(startIndex, byteLength);
            return bytes;
        }

        protected override bool OnLoad() {
            cache = LoadBytes(glTFObject.directoryRoot + glTFObject.mainFile);
            return true;
        }

        public byte[] GetBytes() {
            return cache;
        }
    }
}