using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> API used for importing .gltf and .glb files </summary>
	public static class Importer {
		public static GameObject LoadFromFile(string filepath, Format format = Format.AUTO) {
			GLTFAnimation.ImportResult[] animations;
			return LoadFromFile(filepath, new ImportSettings(), out animations, format);
		}

		public static GameObject LoadFromFile(string filepath, ImportSettings importSettings, Format format = Format.AUTO) {
			GLTFAnimation.ImportResult[] animations;
			return LoadFromFile(filepath, importSettings, out animations, format);
		}

		public static GameObject LoadFromFile(string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations, Format format = Format.AUTO) {
			if (format == Format.GLB) {
				return ImportGLB(filepath, importSettings, out animations);
			} else if (format == Format.GLTF) {
				return ImportGLTF(filepath, importSettings, out animations);
			} else {
				string extension = Path.GetExtension(filepath).ToLower();
				if (extension == ".glb") return ImportGLB(filepath, importSettings, out animations);
				else if (extension == ".gltf") return ImportGLTF(filepath, importSettings, out animations);
				else {
					Debug.Log("Extension '" + extension + "' not recognized in " + filepath);
					animations = null;
					return null;
				}
			}
		}

		private static GameObject ImportGLB(string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
			byte[] bytes = File.ReadAllBytes(filepath);
			animations = null;

			// 12 byte header
			// 0-4  - magic = "glTF"
			// 4-8  - version = 2
			// 8-12 - length = total length of glb, including Header and all Chunks, in bytes.
			string magic = Encoding.ASCII.GetString(bytes.SubArray(0, 4));
			if (magic != "glTF") {
				Debug.LogWarning("File at " + filepath + " does not look like a .glb file");
				return null;
			}
			uint version = System.BitConverter.ToUInt32(bytes, 4);
			if (version != 2) {
				Debug.LogWarning("Importer does not support gltf version " + version);
				return null;
			}
			// What do we even need the length for.
			//uint length = System.BitConverter.ToUInt32(bytes, 8);

			// Chunk 0 (json)
			uint chunkLength = System.BitConverter.ToUInt32(bytes, 12);
			// This prints out JSON. So predictable.
			//string chunkType = Encoding.ASCII.GetString(bytes.SubArray(16, 4));
			string json = Encoding.ASCII.GetString(bytes.SubArray(20, (int) chunkLength));
			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			return gltfObject.LoadInternal(filepath, importSettings, out animations);
		}

		private static GameObject ImportGLTF(string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
			string json = File.ReadAllText(filepath);

			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			return gltfObject.LoadInternal(filepath, importSettings, out animations);
		}

		public static void ImportGLTFAsync(string filepath, ImportSettings importSettings, Action onFinished) {
			string json = File.ReadAllText(filepath);

			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			gltfObject.LoadAsync(filepath, importSettings, onFinished);
		}

		public abstract class ImportTask<TReturn> : ImportTask {
			public TReturn Result;

			/// <summary> Constructor. Sets waitFor which ensures ImportTasks are completed before running. </summary>
			public ImportTask(params ImportTask[] waitFor) : base(waitFor) { }

			/// <summary> Runs task followed by OnCompleted </summary>
			public TReturn RunSynchronously() {
				task.RunSynchronously();
				OnMainThreadFinalize();
				return Result;
			}
		}

		public abstract class ImportTask {
			public Task task;
			public readonly ImportTask[] waitFor;
			public bool IsReady { get { return waitFor.All(x => x.IsCompleted); } }
			public bool IsCompleted { get; protected set; }

			/// <summary> Constructor. Sets waitFor which ensures ImportTasks are completed before running. </summary>
			public ImportTask(params ImportTask[] waitFor) {
				IsCompleted = false;
				this.waitFor = waitFor;
			}

			public void MainThreadFinalize() {
				OnMainThreadFinalize();
				IsCompleted = true;
			}

			protected virtual void OnMainThreadFinalize() { }
		}

#region Sync
		private static GameObject LoadInternal(this GLTFObject gltfObject, string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
			// directory root is sometimes used for loading buffers from containing file, or local images
			string directoryRoot = Directory.GetParent(filepath).ToString() + "/";

			// Import tasks synchronously
			GLTFBuffer.ImportTask bufferTask = new GLTFBuffer.ImportTask(gltfObject.buffers, filepath);
			bufferTask.RunSynchronously();
			GLTFBufferView.ImportTask bufferViewTask = new GLTFBufferView.ImportTask(gltfObject.bufferViews, bufferTask);
			bufferViewTask.RunSynchronously();
			GLTFAccessor.ImportTask accessorTask = new GLTFAccessor.ImportTask(gltfObject.accessors, bufferViewTask);
			accessorTask.RunSynchronously();
			GLTFImage.ImportTask imageTask = new GLTFImage.ImportTask(gltfObject.images, directoryRoot, bufferViewTask);
			imageTask.RunSynchronously();
			GLTFTexture.ImportTask textureTask = new GLTFTexture.ImportTask(gltfObject.textures, imageTask);
			textureTask.RunSynchronously();
			GLTFMaterial.ImportTask materialTask = new GLTFMaterial.ImportTask(gltfObject.materials, textureTask, importSettings);
			materialTask.RunSynchronously();
			GLTFMesh.ImportTask meshTask = new GLTFMesh.ImportTask(gltfObject.meshes, accessorTask, materialTask, importSettings);
			meshTask.RunSynchronously();
			GLTFSkin.ImportTask skinTask = new GLTFSkin.ImportTask(gltfObject.skins, accessorTask);
			skinTask.RunSynchronously();
			GLTFNode.ImportTask nodeTask = new GLTFNode.ImportTask(gltfObject.nodes, meshTask, skinTask);
			nodeTask.RunSynchronously();
			animations = gltfObject.animations.Import(accessorTask.Result, nodeTask.Result);

			return nodeTask.Result.GetRoot();
		}
#endregion

#region Async
		private static void LoadAsync(this GLTFObject gltfObject, string filepath, ImportSettings importSettings, Action onFinished) {
			// directory root is sometimes used for loading buffers from containing file, or local images
			string directoryRoot = Directory.GetParent(filepath).ToString() + "/";

			importSettings.shaders.CacheDefaultShaders();

			// Setup import tasks
			List<ImportTask> importTasks = new List<ImportTask>();

			GLTFBuffer.ImportTask bufferTask = new GLTFBuffer.ImportTask(gltfObject.buffers, filepath);
			importTasks.Add(bufferTask);
			GLTFBufferView.ImportTask bufferViewTask = new GLTFBufferView.ImportTask(gltfObject.bufferViews, bufferTask);
			importTasks.Add(bufferViewTask);
			GLTFAccessor.ImportTask accessorTask = new GLTFAccessor.ImportTask(gltfObject.accessors, bufferViewTask);
			importTasks.Add(accessorTask);
			GLTFImage.ImportTask imageTask = new GLTFImage.ImportTask(gltfObject.images, directoryRoot, bufferViewTask);
			importTasks.Add(imageTask);
			GLTFTexture.ImportTask textureTask = new GLTFTexture.ImportTask(gltfObject.textures, imageTask);
			importTasks.Add(textureTask);
			GLTFMaterial.ImportTask materialTask = new GLTFMaterial.ImportTask(gltfObject.materials, textureTask, importSettings);
			importTasks.Add(materialTask);
			GLTFMesh.ImportTask meshTask = new GLTFMesh.ImportTask(gltfObject.meshes, accessorTask, materialTask, importSettings);
			importTasks.Add(meshTask);
			GLTFSkin.ImportTask skinTask = new GLTFSkin.ImportTask(gltfObject.skins, accessorTask);
			importTasks.Add(skinTask);
			GLTFNode.ImportTask nodeTask = new GLTFNode.ImportTask(gltfObject.nodes, meshTask, skinTask);
			importTasks.Add(nodeTask);

			// Ignite
			for (int i = 0; i < importTasks.Count; i++) {
				TaskSupervisor(importTasks[i]).RunCoroutine();
			}
			if (onFinished != null) TaskIsDone(importTasks, onFinished).RunCoroutine();
		}

		private static IEnumerator TaskSupervisor(ImportTask importTask) {
			// Wait for required results to complete
			while (!importTask.IsReady) yield return null;
			importTask.task.Start();
			while (!importTask.task.IsCompleted) yield return null;
			importTask.MainThreadFinalize();
		}

		private static IEnumerator TaskIsDone(List<ImportTask> tasks, Action onFinish) {
			// Wait for required results to complete
			while (!tasks.All(x => x.IsCompleted)) yield return null;
			onFinish();
		}
#endregion
	}
}