using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Siccity.GLTFUtility {
	[Serializable]
	public class ImportSettings {

		public bool materials = true;
		[FormerlySerializedAs("shaders")]
		public ShaderSettings shaderOverrides = new ShaderSettings();
		public AnimationSettings animationSettings = new AnimationSettings();
		public bool generateLightmapUVs;
		[Range(0, 180)]
		public float hardAngle = 88;

		[Range(1, 75)]
		public float angleError = 8;

		[Range(1, 75)]
		public float areaError = 15;

		[Range(1, 64)]
		public float packMargin = 4;

		[Tooltip("Script used to process extra data.")]
		public GLTFExtrasProcessor extrasProcessor;
	}
}
