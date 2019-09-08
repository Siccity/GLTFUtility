using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	public class GLTFImage : GLTFProperty {

#region Serialized fields
		public string uri;
		public string mimeType;
		public int? bufferView;
		public string name;
#endregion

#region Non-serialized fields
		[JsonIgnore] private Texture2D cache;
		/// <summary> True if image was loaded from a Texture2D asset. False if it was loaded from binary or from another source </summary>
		[JsonIgnore] public bool imageIsAsset { get; private set; }
		[JsonIgnore] public bool isNormalMap { get; private set; }
		[JsonIgnore] public bool isMetallicRoughnessFixed { get; private set; }
		[JsonIgnore] public bool initialized { get { return cache != null; } }
#endregion

		protected override bool OnLoad() {
			imageIsAsset = false;
			if (!string.IsNullOrEmpty(uri) && File.Exists(glTFObject.directoryRoot + uri)) {
#if UNITY_EDITOR
				cache = UnityEditor.AssetDatabase.LoadAssetAtPath(glTFObject.directoryRoot + uri, typeof(Texture2D)) as Texture2D;
				if (cache != null) {
					imageIsAsset = true;
					return true;
				}
#endif
				Debug.Log("Couldn't load texture at " + glTFObject.directoryRoot + uri);
				return false;
			} else if (bufferView != -1 && !string.IsNullOrEmpty(mimeType)) {
				byte[] bytes = glTFObject.bufferViews[bufferView.Value].GetBytes();
				cache = new Texture2D(2, 2);
				// If this fails, you may need to find "Image Conversion" package and enable it
				if (cache.LoadImage(bytes)) {
					return true;
				} else {
					Debug.Log("mimeType not supported: " + mimeType);
					return false;
				}
			} else {
				Debug.Log("Couldn't find texture at " + glTFObject.directoryRoot + uri);
				return false;
			}
		}

		public Texture2D GetNormalMap() {
			if (isNormalMap || imageIsAsset) return cache;
			Color32[] pixels = cache.GetPixels32();
			for (int i = 0; i < pixels.Length; i++) {
				Color32 c = pixels[i];
				c.a = pixels[i].r;
				c.r = c.b = c.g;
				pixels[i] = c;
			}
			cache.SetPixels32(pixels);
			cache.Apply();
			isNormalMap = true;
			return cache;
		}

		public Texture2D GetTexture() {
			if (initialized) return cache;
			else {
				Debug.Log("GLTFImage not initialized");
				return null;
			}
		}

		// glTF stores Metallic in blue channel and roughness in green channel. Unity stores Metallic in red and roughness in alpha. This method returns a unity-fixed texture
		public Texture2D GetFixedMetallicRoughness() {
			if (initialized) {
				if (!isMetallicRoughnessFixed && !imageIsAsset) {
					Color32[] pixels = cache.GetPixels32();
					for (int i = 0; i < pixels.Length; i++) {
						Color32 c = pixels[i];
						c.r = pixels[i].b;
						c.a = pixels[i].g;
						pixels[i] = c;
					}
					cache.SetPixels32(pixels);
					cache.Apply();
					isMetallicRoughnessFixed = true;
				}
				return cache;
			} else {
				Debug.Log("GLTFImage not initialized");
				return null;
			}
		}
	}
}