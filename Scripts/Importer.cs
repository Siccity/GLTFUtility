using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	/// <summary> API used for importing .gltf and .glb files </summary>
	public static class Importer {
		public static GLTFObject LoadFromFile(string filepath) {
			string extension = Path.GetExtension(filepath).ToLower();
			if (extension == ".glb") return ImportGLB(filepath);
			else if (extension == ".gltf") return ImportGLTF(filepath);
			else {
				Debug.Log("Extension '" + extension + "' not recognized in " + filepath);
				return null;
			}
		}

		public static GLTFObject ImportGLB(string filepath) {
			byte[] bytes = File.ReadAllBytes(filepath);

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
			uint length = System.BitConverter.ToUInt32(bytes, 8);

			// Chunk 0 (json)
			uint chunkLength = System.BitConverter.ToUInt32(bytes, 12);
			string chunkType = Encoding.ASCII.GetString(bytes.SubArray(16, 4));
			string json = Encoding.ASCII.GetString(bytes.SubArray(20, (int) chunkLength));

			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			gltfObject.LoadInternal(filepath);
			return gltfObject;
		}

		public static GLTFObject ImportGLTF(string filepath) {
			string json = File.ReadAllText(filepath);

			// Parse json
			GLTFObject gltfObject = JsonConvert.DeserializeObject<GLTFObject>(json);
			gltfObject.LoadInternal(filepath);
			return gltfObject;
		}

		private static void LoadInternal(this GLTFObject gltfObject, string filepath) {
			gltfObject.directoryRoot = Directory.GetParent(filepath).ToString() + "/";
			gltfObject.mainFile = Path.GetFileName(filepath);
			GLTFProperty.Load(gltfObject, gltfObject.buffers);
			GLTFProperty.Load(gltfObject, gltfObject.bufferViews);
			GLTFProperty.Load(gltfObject, gltfObject.accessors);
			GLTFProperty.Load(gltfObject, gltfObject.images);
			GLTFProperty.Load(gltfObject, gltfObject.textures);
			GLTFProperty.Load(gltfObject, gltfObject.materials);
			GLTFProperty.Load(gltfObject, gltfObject.scenes);
			GLTFProperty.Load(gltfObject, gltfObject.nodes);
			GLTFProperty.Load(gltfObject, gltfObject.meshes);
			GLTFProperty.Load(gltfObject, gltfObject.animations);
			GLTFProperty.Load(gltfObject, gltfObject.skins);
			gltfObject.loaded = true;
		}

		private static void NewLoader(this GLTFObject gltfObject, string filepath) {
			gltfObject.directoryRoot = Directory.GetParent(filepath).ToString() + "/";
			gltfObject.mainFile = Path.GetFileName(filepath);

			byte[][] buffers = gltfObject.buffers.Select(x => x.LoadBytes(filepath)).ToArray();
			byte[][] bufferViews = gltfObject.bufferViews.Select(x => x.LoadBytes(buffers)).ToArray();
			GLTFAccessor.Cache[] accessors = gltfObject.accessors.Select(x => x.LoadCache(bufferViews)).ToArray();
			GLTFImage.ImportResult[] images = gltfObject.images.Select(x => x.GetImage(gltfObject.directoryRoot, bufferViews)).ToArray();
			GLTFTexture.ImportResult[] textures = gltfObject.textures.Select(x => x.Import(images)).ToArray();
			GLTFProperty.Load(gltfObject, gltfObject.materials);
			GLTFProperty.Load(gltfObject, gltfObject.scenes);
			GLTFProperty.Load(gltfObject, gltfObject.nodes);
			GLTFProperty.Load(gltfObject, gltfObject.meshes);
			GLTFProperty.Load(gltfObject, gltfObject.animations);
			GLTFProperty.Load(gltfObject, gltfObject.skins);
			gltfObject.loaded = true;
		}

	}
}