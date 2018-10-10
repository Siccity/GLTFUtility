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
        public string uri;

        public byte[] cache;

        public void Read(string directoryRoot) {
            if (cache == null) cache = File.ReadAllBytes(directoryRoot + uri);
        }

        public byte[] GetBytes() {
            if (cache == null) Debug.LogError("Need to call Read(directoryRoot) on GLTFBuffer before GetBytes()");
            return cache;
        }
    }
}