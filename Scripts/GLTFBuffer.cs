using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Contains raw binary data </summary>
    [Serializable]
    public class GLTFBuffer : GLTFProperty {

#region Serialized fields
        public int byteLength = -1;
        public string uri = null;
#endregion

#region Non-serialized fields
        private const string embeddedPrefix = "data:application/octet-stream;base64,";
        private byte[] cache;
        public bool isEmbedded { get { return CheckEmbedded(); } }
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