using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#specifying-extensions
	// https://github.com/KhronosGroup/glTF/issues/1628
	public class GLTFProperty {
		[JsonConverter(typeof(ExtensionsConverter))] public Dictionary<string, IExtension> extensions;
		public Dictionary<string, object> extras;
	}
}