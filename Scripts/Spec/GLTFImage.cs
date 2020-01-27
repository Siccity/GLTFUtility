using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	[Preserve] public class GLTFImage {
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
			}

			public ImportTask(List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask) {
				task = new Task(() => {
					// No images
					if (images == null) return;

					imageData = new ImageData[images.Count];
					for (int i = 0; i < imageData.Length; i++) {
						string fullUri = directoryRoot + images[i].uri;
						if (!string.IsNullOrEmpty(images[i].uri)) {
							if (File.Exists(fullUri)) {
								// If the file is found at fullUri, read it
								byte[] bytes = File.ReadAllBytes(fullUri);
								imageData[i] = new ImageData(bytes, fullUri);
							} else if (images[i].uri.StartsWith("data:")) {
								// If the image is embedded, find its Base64 content and save as byte array
								string content = images[i].uri.Split(',').Last();
								byte[] imageBytes = Convert.FromBase64String(content);
								imageData[i] = new ImageData(imageBytes);
							}
						} else if (images[i].bufferView.HasValue && !string.IsNullOrEmpty(images[i].mimeType)) {
							GLTFBufferView.ImportResult view = bufferViewTask.Result[images[i].bufferView.Value];
							byte[] bytes = new byte[view.byteLength];
							view.stream.Position = view.byteOffset;
							view.stream.Read(bytes, 0, view.byteLength);
							imageData[i] = new ImageData(bytes);
						} else {
							Debug.Log("Couldn't find texture at " + fullUri);
						}
					}
				});
			}

			public override IEnumerator OnCoroutine(Action<float> onProgress = null) {
				// No images
				if (imageData == null) {
					if (onProgress != null) onProgress.Invoke(1f);
					IsCompleted = true;
					yield break;
				}

				Result = new ImportResult[imageData.Length];

				for (int i = 0; i < imageData.Length; i++) {
					if (imageData[i] == null) {
						Debug.LogWarning("imageData[" + i + "] is null");
						continue;
					}

					if (!string.IsNullOrEmpty(imageData[i].path)) {
						string path = imageData[i].path;
#if UNITY_EDITOR
						// Load textures from asset database if we can
						Texture2D assetTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
						if (assetTexture != null) {
							Result[i] = new ImportResult() { texture = assetTexture };
							if (onProgress != null) onProgress(((float) (i + 1) / (float) imageData.Length));
							continue;
						}
#endif

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
						path = "File://" + path;
#endif
						using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path, true)) {
							UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();
							float progress = 0;
							while (!operation.isDone) {
								if (progress != uwr.downloadProgress) {
									progress = uwr.downloadProgress;
									float totalprogress = ((float) i / (float) imageData.Length) + progress;
									if (onProgress != null) onProgress(totalprogress);
								}
								yield return null;
							}

							if (onProgress != null) onProgress(((float) (i + 1) / (float) imageData.Length));

							if (uwr.isNetworkError || uwr.isHttpError) {
								Debug.LogError("GLTFImage.cs ToTexture2D() ERROR: " + uwr.error);
							} else {
								Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
								tex.name = Path.GetFileNameWithoutExtension(path);
								Result[i] = new ImportResult() { texture = tex };
							}
							uwr.Dispose();
						}
					} else {
						Texture2D tex = new Texture2D(2, 2);
						if (!tex.LoadImage(imageData[i].bytes)) {
							Debug.Log("mimeType not supported");
							continue;
						} else Result[i] = new ImportResult() { texture = tex };
					}
				}
				IsCompleted = true;
				yield break;
			}
		}
	}
}