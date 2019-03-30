using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
	public abstract class GLTFProperty {
		[NonSerialized] public GLTFObject glTFObject;

		public abstract void Load();
	}
}