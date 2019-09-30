﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class ImportSettings {
		public bool materials = true;
		public ShaderSettings shaders;

		public ImportSettings()
		{
			shaders = Resources.Load<ShaderSettings>("ShaderSettings");
		}
	}
}