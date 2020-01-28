using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation
	/// <summary> Contains info for a single animation clip </summary>
	[Preserve] public class GLTFAnimation {
		/// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
		[JsonProperty(Required = Required.Always)] public Channel[] channels;
		[JsonProperty(Required = Required.Always)] public Sampler[] samplers;
		public string name;

#region Classes
		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation-sampler
		[Preserve] public class Sampler {
			/// <summary> The index of an accessor containing keyframe input values, e.g., time. </summary>
			[JsonProperty(Required = Required.Always)] public int input;
			/// <summary> The index of an accessor containing keyframe output values. </summary>
			[JsonProperty(Required = Required.Always)] public int output;
			/// <summary> Valid names include: "LINEAR", "STEP", "CUBICSPLINE" </summary>
			public string interpolation = "LINEAR";
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#channel
		/// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
		[Preserve] public class Channel {
			/// <summary> Target sampler index </summary>
			[JsonProperty(Required = Required.Always)] public int sampler;
			/// <summary> Target sampler index </summary>
			[JsonProperty(Required = Required.Always)] public Target target;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#target
		/// <summary> Identifies which node and property to animate </summary>
		[Preserve] public class Target {
			/// <summary> Target node index.</summary>
			public int? node;
			/// <summary> Which property to animate. Valid names are: "translation", "rotation", "scale", "weights" </summary>
			[JsonProperty(Required = Required.Always)] public string path;
		}

		[Preserve] public class ImportResult {
			public AnimationClip clip;
		}
#endregion

		public ImportResult Import(GLTFAccessor.ImportResult[] accessors, GLTFNode.ImportResult[] nodes, ImportSettings importSettings) {
			ImportResult result = new ImportResult();
			result.clip = new AnimationClip();
			result.clip.name = name;

			if (importSettings.useLegacyClips) {
				result.clip.legacy = true;
			}

			for (int i = 0; i < channels.Length; i++) {
				Channel channel = channels[i];
				if (samplers.Length <= channel.sampler) {
					Debug.LogWarning("Animation channel points to sampler at index " + channel.sampler + " which doesn't exist. Skipping animation clip.");
					continue;
				}
				Sampler sampler = samplers[channel.sampler];

				string relativePath = "";

				GLTFNode.ImportResult node = nodes[channel.target.node.Value];
				while (node != null && !node.IsRoot) {
					if (string.IsNullOrEmpty(relativePath)) relativePath = node.transform.name;
					else relativePath = node.transform.name + "/" + relativePath;

					if (node.parent.HasValue) node = nodes[node.parent.Value];
					else node = null;
				}

				float[] keyframeInput = accessors[sampler.input].ReadFloat().ToArray();
				switch (channel.target.path) {
					case "translation":
						Vector3[] pos = accessors[sampler.output].ReadVec3().ToArray();
						AnimationCurve posX = new AnimationCurve();
						AnimationCurve posY = new AnimationCurve();
						AnimationCurve posZ = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							posX.AddKey(keyframeInput[k], pos[k].x);
							posY.AddKey(keyframeInput[k], pos[k].y);
							posZ.AddKey(keyframeInput[k], -pos[k].z);
						}
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
						break;
					case "rotation":
						Vector4[] rot = accessors[sampler.output].ReadVec4().ToArray();
						AnimationCurve rotX = new AnimationCurve();
						AnimationCurve rotY = new AnimationCurve();
						AnimationCurve rotZ = new AnimationCurve();
						AnimationCurve rotW = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							rotX.AddKey(keyframeInput[k], rot[k].x);
							rotY.AddKey(keyframeInput[k], rot[k].y);
							rotZ.AddKey(keyframeInput[k], -rot[k].z);
							rotW.AddKey(keyframeInput[k], -rot[k].w);
						}
						result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
						result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
						result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
						result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
						break;
					case "scale":
						Vector3[] scale = accessors[sampler.output].ReadVec3().ToArray();
						AnimationCurve scaleX = new AnimationCurve();
						AnimationCurve scaleY = new AnimationCurve();
						AnimationCurve scaleZ = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							scaleX.AddKey(keyframeInput[k], scale[k].x);
							scaleY.AddKey(keyframeInput[k], scale[k].y);
							scaleZ.AddKey(keyframeInput[k], scale[k].z);
						}
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.x", scaleX);
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.y", scaleY);
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.z", scaleZ);
						break;
					case "weights":
						Debug.LogWarning("morph weights in animation is not supported");
						break;
				}
			}
			return result;
		}
	}

	public static class GLTFAnimationExtensions {
		public static GLTFAnimation.ImportResult[] Import(this List<GLTFAnimation> animations, GLTFAccessor.ImportResult[] accessors, GLTFNode.ImportResult[] nodes, ImportSettings importSettings) {
			if (animations == null) return null;

			GLTFAnimation.ImportResult[] results = new GLTFAnimation.ImportResult[animations.Count];
			for (int i = 0; i < results.Length; i++) {
				results[i] = animations[i].Import(accessors, nodes, importSettings);
				if (string.IsNullOrEmpty(results[i].clip.name)) results[i].clip.name = "animation" + i;
			}
			return results;
		}
	}
}