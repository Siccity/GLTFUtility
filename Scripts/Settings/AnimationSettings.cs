using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siccity.GLTFUtility {
	/// <summary> Defines how animations are imported </summary>
	[Serializable]
	public class AnimationSettings {
		public bool looping;
		[Tooltip("Sample rate set on all imported animation clips.")]
		public float frameRate = 24;
		[Tooltip("Interpolation mode applied to all keyframe tangents. Use Import From File when mixing modes within an animation.")]
		public InterpolationMode interpolationMode = InterpolationMode.ImportFromFile;
		[Tooltip("When true, remove redundant keyframes from blend shape animations.")]
		public bool compressBlendShapeKeyFrames = true;
		[Tooltip("Load animations as legacy AnimationClips.")]
		public bool useLegacyClips;
	}
}
