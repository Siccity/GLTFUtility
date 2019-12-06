using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Shims;
using UnityEngine;

namespace Siccity.GLTFUtility {
	public class GLTFObject {
		[Preserve] public GLTFObject() { }

		public int? scene;
		[JsonProperty(Required = Required.Always)] public GLTFAsset asset;
		public List<GLTFScene> scenes;
		public List<GLTFNode> nodes;
		public List<GLTFMesh> meshes;
		public List<GLTFAnimation> animations;
		public List<GLTFBuffer> buffers;
		public List<GLTFBufferView> bufferViews;
		public List<GLTFAccessor> accessors;
		public List<GLTFSkin> skins;
		public List<GLTFTexture> textures;
		public List<GLTFImage> images;
		public List<GLTFMaterial> materials;
		public List<GLTFCamera> cameras;
		//public List<string> extensionsUsed; not supported yet
		//public List<string> extensionsRequired; not supported yet
	}
}