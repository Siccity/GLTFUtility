using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> API used for importing .gltf and .glb files </summary>
	public static class Importer {
		public static GameObject LoadFromFile(string filepath) {
			string extension = Path.GetExtension(filepath).ToLower();
			if (extension == ".glb") return ImportGLB(filepath);
			else if (extension == ".gltf") return ImportGLTF(filepath);
			else {
				Debug.Log("Extension '" + extension + "' not recognized in " + filepath);
				return null;
			}
		}

		public static GameObject ImportGLB(string filepath) {
			GLTFAnimation.ImportResult[] animations;
			return ImportGLB(filepath, new ImportSettings(), out animations);
		}

		public static GameObject ImportGLB(string filepath, ImportSettings importSettings) {
			GLTFAnimation.ImportResult[] animations;
			return ImportGLB(filepath, importSettings, out animations);
		}

		public static GameObject ImportGLB(string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
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

		public static GameObject ImportGLTF(string filepath) {
			GLTFAnimation.ImportResult[] animations;
			return ImportGLTF(filepath, new ImportSettings(), out animations);
		}

		public static GameObject ImportGLTF(string filepath, ImportSettings importSettings) {
			GLTFAnimation.ImportResult[] animations;
			return ImportGLTF(filepath, importSettings, out animations);
		}

		public static GameObject ImportGLTF(string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
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

		private static GameObject LoadInternal(this GLTFObject gltfObject, string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
			string directoryRoot = Directory.GetParent(filepath).ToString() + "/";

			GLTFBuffer.ImportTask bufferTask = new GLTFBuffer.ImportTask(gltfObject.buffers, filepath);
			bufferTask.task.RunSynchronously();
			GLTFBufferView.ImportTask bufferViewTask = new GLTFBufferView.ImportTask(gltfObject.bufferViews, bufferTask);
			bufferViewTask.task.RunSynchronously();
			GLTFAccessor.ImportTask accessorTask = new GLTFAccessor.ImportTask(gltfObject.accessors, bufferViewTask);
			accessorTask.task.RunSynchronously();
			GLTFImage.ImportTask imageTask = new GLTFImage.ImportTask(gltfObject.images, directoryRoot, bufferViewTask);
			imageTask.task.RunSynchronously();
			GLTFTexture.ImportResult[] textures = gltfObject.textures.Import(imageTask.task.Result);
			GLTFMaterial.ImportResult materials = gltfObject.materials.Import(textures, importSettings);
			GLTFMesh.ImportResult[] meshes = gltfObject.meshes.Import(accessorTask.task.Result, materials, importSettings);
			GLTFSkin.ImportResult[] skins = gltfObject.skins.Import(accessorTask.task.Result);
			GLTFNode.ImportResult[] nodes = gltfObject.nodes.Import(meshes, skins);
			animations = gltfObject.animations.Import(accessorTask.task.Result, nodes);

			return nodes.GetRoot();
		}

		private static void LoadAsync(this GLTFObject gltfObject, string filepath, ImportSettings importSettings, Action onFinished) {
			// directory root is sometimes used for loading buffers from containing file, or local images
			string directoryRoot = Directory.GetParent(filepath).ToString() + "/";

			// Setup import tasks
			List<ImportTask> importTasks = new List<ImportTask>();

			GLTFBuffer.ImportTask bufferTask = new GLTFBuffer.ImportTask(gltfObject.buffers, filepath);
			importTasks.Add(bufferTask);

			GLTFBufferView.ImportTask bufferViewTask = new GLTFBufferView.ImportTask(gltfObject.bufferViews, bufferTask);
			importTasks.Add(bufferViewTask);

			GLTFAccessor.ImportTask accessorTask = new GLTFAccessor.ImportTask(gltfObject.accessors, bufferViewTask);
			importTasks.Add(accessorTask);

			// Ignite
			for (int i = 0; i < importTasks.Count; i++) {
				TaskSupervisor(importTasks[i]).RunCoroutine();
			}
			if (onFinished != null) TaskIsDone(importTasks, onFinished).RunCoroutine();
		}

		public abstract class ImportTask {
			public abstract Task Task { get; }
			public bool IsCompleted { get { return Task.IsCompleted; } }
			public readonly ImportTask[] waitFor;
			public bool IsReady { get { return waitFor.All(x => x.IsCompleted); } }

			public ImportTask(params ImportTask[] waitFor) {
				this.waitFor = waitFor;
			}

			/// <summary> Called from the main thread </summary>
			public abstract void OnCompleted();
		}

		private static IEnumerator TaskSupervisor(ImportTask importTask) {
			// Wait for required results to complete
			while (!importTask.IsReady) yield return null;
			importTask.Task.Start();
			while (!importTask.IsCompleted) yield return null;
			importTask.OnCompleted();
		}

		private static IEnumerator TaskIsDone(List<ImportTask> tasks, Action onFinish) {
			// Wait for required results to complete
			while (!tasks.All(x => x.IsCompleted)) yield return null;
			onFinish();
		}
	}
}