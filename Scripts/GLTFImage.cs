using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFImage {
		public string uri;

		private Texture cache;

		public bool Initialize(string directoryRoot) {
			if (File.Exists(directoryRoot + uri)) {
				Debug.Log("Found texture " + directoryRoot + uri);
				return true;
			} else {
				Debug.Log("Didnt find texture " + directoryRoot + uri);
				return false;
			}
		}

		public Texture GetTexture() {
			if (cache != null) return cache;
			else {
				Debug.Log("GLTFImage not initialized");
				return null;
			}
		}
	}
}