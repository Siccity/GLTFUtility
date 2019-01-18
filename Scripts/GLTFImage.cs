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
		public string mimeType;
		public int bufferView = -1;
		public bool initialized { get { return cache != null; } }
		/// <summary> True if image was loaded from a Texture2D asset. False if it was loaded from binary or from another source </summary>
		public bool imageIsAsset { get; private set; }

		private Texture2D cache;

		public bool Initialize(GLTFObject gLTFObject, string directoryRoot) {
			if (!string.IsNullOrEmpty(uri) && File.Exists(directoryRoot + uri)) {
#if UNITY_EDITOR
				cache = UnityEditor.AssetDatabase.LoadAssetAtPath(directoryRoot + uri, typeof(Texture2D)) as Texture2D;
				if (cache != null) {
					imageIsAsset = true;
					return true;
				}
#endif
				Debug.Log("Couldn't load texture at " + directoryRoot + uri);
				imageIsAsset = false;
				return false;
			} else if (bufferView != -1 && !string.IsNullOrEmpty(mimeType)) {
				byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
				cache = new Texture2D(2, 2);
				if (cache.LoadImage(bytes)) {
					imageIsAsset = false;
					return true;
				} else {
					Debug.Log("mimeType not supported: " + mimeType);
					imageIsAsset = false;
					return false;
				}
			} else {
				Debug.Log("Couldn't find texture at " + directoryRoot + uri);
				imageIsAsset = false;
				return false;
			}
		}

		public Texture2D GetTexture() {
			if (initialized) return cache;
			else {
				Debug.Log("GLTFImage not initialized");
				return null;
			}
		}
	}
}