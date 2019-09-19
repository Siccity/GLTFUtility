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

		/* public static void ImportGLTFAsync(MonoBehaviour go, string filepath, ImportSettings importSettings) {
			string json = File.ReadAllText(filepath);

			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			go.StartCoroutine(gltfObject.LoadInternalAsync(filepath, importSettings));
		} */

		private static GameObject LoadInternal(this GLTFObject gltfObject, string filepath, ImportSettings importSettings, out GLTFAnimation.ImportResult[] animations) {
			string directoryRoot = Directory.GetParent(filepath).ToString() + "/";

			Task<GLTFBuffer.ImportResult[]> bufferTask = gltfObject.buffers.ImportTask(filepath);
			bufferTask.RunSynchronously();
			Task<GLTFBufferView.ImportResult[]> bufferViewTask = gltfObject.bufferViews.ImportTask(bufferTask.Result);
			bufferViewTask.RunSynchronously();
			GLTFAccessor.ImportResult[] accessors = gltfObject.accessors.Select(x => x.Import(bufferViewTask.Result)).ToArray();
			GLTFImage.ImportResult[] images = gltfObject.images.Import(directoryRoot, bufferViewTask.Result);
			GLTFTexture.ImportResult[] textures = gltfObject.textures.Import(images);
			GLTFMaterial.ImportResult materials = gltfObject.materials.Import(textures, importSettings);
			GLTFMesh.ImportResult[] meshes = gltfObject.meshes.Import(accessors, materials, importSettings);
			GLTFSkin.ImportResult[] skins = gltfObject.skins.Import(accessors);
			GLTFNode.ImportResult[] nodes = gltfObject.nodes.Import(meshes, skins);
			animations = gltfObject.animations.Import(accessors, nodes);

			return nodes.GetRoot();
		}
	}
}