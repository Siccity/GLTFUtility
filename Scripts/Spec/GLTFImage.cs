using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	public class GLTFImage {
		/// <summary>
		/// The uri of the image.
		/// Relative paths are relative to the .gltf file.
		/// Instead of referencing an external file, the uri can also be a data-uri.
		/// The image format must be jpg or png.
		/// </summary>
		public string uri;
		/// <summary> Either "image/jpeg" or "image/png" </summary>
		public string mimeType;
		public int? bufferView;
		public string name;

		public class ImportResult {
			public Texture2D texture;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			private ImageData[] imageData;

			private class ImageData {
				public byte[] bytes;
				public string path;

				public ImageData(byte[] bytes, string path = null) {
					this.bytes = bytes;
					this.path = path;
				}

				public Texture2D ToTexture2D() {
#if UNITY_EDITOR
					Texture2D assetTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
					if (assetTexture != null) return assetTexture;
#endif

					Texture2D tex = new Texture2D(2, 2);
					if (!tex.LoadImage(bytes)) {
						Debug.Log("mimeType not supported");
						return null;
					}
					return tex;
				}
			}

			public ImportTask(List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask) {
				task = new Task(() => {
					// No images
					if (images == null) return;

					imageData = new ImageData[images.Count];
					for (int i = 0; i < imageData.Length; i++) {
						string fullUri = directoryRoot + images[i].uri;
						if (!string.IsNullOrEmpty(images[i].uri) && File.Exists(fullUri)) {
							byte[] bytes = File.ReadAllBytes(fullUri);
							imageData[i] = new ImageData(bytes, fullUri);
						} else if (images[i].bufferView.HasValue && !string.IsNullOrEmpty(images[i].mimeType)) {
							byte[] bytes = bufferViewTask.Result[images[i].bufferView.Value].bytes;
							imageData[i] = new ImageData(bytes);
						} else {
							Debug.Log("Couldn't find texture at " + fullUri);
						}
					}
				});
			}

			protected override void OnMainThreadFinalize() {
				// No images
				if (imageData == null) return;

				Result = new ImportResult[imageData.Length];
				for (int i = 0; i < imageData.Length; i++) {
					if (imageData[i] == null) {
						Debug.LogWarning("imageData[" + i + "] is null");
						continue;
					}
					Result[i] = new ImportResult() { texture = imageData[i].ToTexture2D() };
				}
			}
		}
	}
}