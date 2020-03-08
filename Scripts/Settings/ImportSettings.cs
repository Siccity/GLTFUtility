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

		[Header("Custom Animation Settings")]
		[Tooltip("Sample rate set on all imported animation clips.")]
		public float frameRate = 24;
		public enum InterpolationMode { ImportFromFile = 0, CubicSpline = 1, Linear = 2, Step = 3 };
		[Tooltip("Interpolation mode applied to all keyframe tangents. Use Import From File when mixing modes within an animation.")]
		public InterpolationMode interpolationMode = InterpolationMode.Step;
		[Tooltip("When true, remove redundant keyframes from blend shape animations.")]
		public bool compressBlendShapeKeyFrames = true;
	}
}