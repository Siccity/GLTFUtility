using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
	public abstract class GLTFProperty {
		public GLTFObject glTFObject { get; private set; }
		public bool isLoaded { get; private set; }

		protected abstract bool OnLoad();

		public bool Load(GLTFObject glTFObject) {
			this.glTFObject = glTFObject;
			if (OnLoad()) isLoaded = true;
			return isLoaded;
		}

		/// <summary> Convenience method. Load multiple GLTFProperties with null check </summary>
		public static void Load(GLTFObject glTFObject, params GLTFProperty[] properties) {
			for (int i = 0; i < properties.Length; i++) {
				if (properties[i] != null) {
					properties[i].Load(glTFObject);
				}
			}
		}

		/// <summary> Convenience method. Load multiple GLTFProperties with null check </summary>
		public static void Load<T>(GLTFObject glTFObject, IList<T> properties) where T : GLTFProperty {
			for (int i = 0; i < properties.Count; i++) {
				if (properties[i] != null) {
					properties[i].Load(glTFObject);
				}
			}
		}
	}
}