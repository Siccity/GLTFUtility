using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KtxUnity;
using Newtonsoft.Json;
using Unity.Collections;
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
			public byte[] bytes;
			public string path;
			public string mimeType;

			public ImportResult(byte[] bytes, string mimeType, string path = null) {
				this.bytes = bytes;
				this.path = path;
				this.mimeType = mimeType;
			}

			public IEnumerator CreateTextureAsync(bool linear, Action<Texture2D> onFinish, string mimeType, Action<float> onProgress = null) {
				if (!string.IsNullOrEmpty(path)) {
#if UNITY_EDITOR
					// Load textures from asset database if we can
					Texture2D assetTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
					if (assetTexture != null) {
						onFinish(assetTexture);
						if (onProgress != null) onProgress(1f);
						yield break;
					}
#endif

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
					path = "File://" + path;
#endif
					// TODO: Support linear/sRGB textures
					using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path, true)) {
						UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();
						float progress = 0;
						while (!operation.isDone) {
							if (progress != uwr.downloadProgress) {
								if (onProgress != null) onProgress(uwr.downloadProgress);
							}
							yield return null;
						}

						if (onProgress != null) onProgress(1f);

						if (uwr.isNetworkError || uwr.isHttpError) {
							Debug.LogError("GLTFImage.cs ToTexture2D() ERROR: " + uwr.error+"- "+path);
						} else {
							Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
							tex.name = Path.GetFileNameWithoutExtension(path);
							onFinish(tex);
						}
						uwr.Dispose();
					}
				} else {
					if(mimeType=="image/png"||mimeType=="image/jpg"||mimeType=="image/jpeg") {
						Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, true, linear);
						if (!tex.LoadImage(bytes)) {
							yield break;
						} else onFinish(tex);
					} else if(mimeType=="image/basis") {
						var nativeArrayBytes = new NativeArray<byte>(bytes,KtxNativeInstance.defaultAllocator);
						var basisTex = new BasisUniversalTexture();
						basisTex.onTextureLoaded += delegate(Texture2D tex) {
							onFinish(tex); 
						};
						IEnumerator en = basisTex.LoadBytesRoutine(nativeArrayBytes,false);
						while (en.MoveNext()) { yield return null; };
						nativeArrayBytes.Dispose();
					}
					else {
						Debug.LogError("mimeType not supported");
					}
				}
			}
        }

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask) {
				task = new Task(() => {
					// No images
					if (images == null) return;

					Result = new ImportResult[images.Count];
					for (int i = 0; i < images.Count; i++) {
						string fullUri = directoryRoot + images[i].uri;
						if (!string.IsNullOrEmpty(images[i].uri)) {
							if (File.Exists(fullUri)) {
								// If the file is found at fullUri, read it
								byte[] bytes = File.ReadAllBytes(fullUri);
								Result[i] = new ImportResult(bytes, fullUri,images[i].mimeType);
							} else if (images[i].uri.StartsWith("data:")) {
								// If the image is embedded, find its Base64 content and save as byte array
								string content = images[i].uri.Split(',').Last();
								byte[] imageBytes = Convert.FromBase64String(content);
								Result[i] = new ImportResult(imageBytes,images[i].mimeType);
							}
						} else if (images[i].bufferView.HasValue && !string.IsNullOrEmpty(images[i].mimeType)) {
							GLTFBufferView.ImportResult view = bufferViewTask.Result[images[i].bufferView.Value];
							byte[] bytes = new byte[view.byteLength];
							view.stream.Position = view.byteOffset;
							view.stream.Read(bytes, 0, view.byteLength);
							Result[i] = new ImportResult(bytes,images[i].mimeType);
						} else {
							Debug.Log("Couldn't find texture at " + fullUri);
						}
					}
				});
			}
		}
	}
}