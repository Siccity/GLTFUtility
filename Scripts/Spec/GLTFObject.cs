using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	public class GLTFObject {

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
		//public List<string> extensionsUsed; not supported yet
		//public List<string> extensionsRequired; not supported yet
	}
}