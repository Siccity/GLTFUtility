using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class ImportSettings {
		public bool materials = true;
		[FormerlySerializedAs("shaders")]
		public ShaderSettings shaderOverrides = new ShaderSettings();
		public bool useLegacyClips;
	}
}