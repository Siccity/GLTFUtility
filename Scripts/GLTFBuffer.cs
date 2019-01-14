using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Contains raw binary data </summary>
    [Serializable]
    public class GLTFBuffer {
        public int byteLength = -1;
        public string uri = null;
        public bool isEmbedded { get { return uri == null; } }

        public byte[] cache;

        public void Read(string directoryRoot, string mainFile) {
            if (!isEmbedded) cache = File.ReadAllBytes(directoryRoot + uri);
            else cache = File.ReadAllBytes(directoryRoot + mainFile);

            // Sometimes the buffer is part of a larger file. Since we dont have a byteOffset we have to assume it's at the end of the file.
            // In case you're trying to load a gltf with more than one buffers this might cause issues, but it'll work for now.
            int startIndex = cache.Length - byteLength;
            if (startIndex != 0) cache = cache.SubArray(startIndex, byteLength);
        }

        public byte[] GetBytes() {
            if (cache == null) Debug.LogError("Need to call Read(directoryRoot) on GLTFBuffer before GetBytes()");
            return cache;
        }
    }
}