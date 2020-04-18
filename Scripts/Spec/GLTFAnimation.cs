using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
			result.clip.frameRate = importSettings.frameRate;

			if (importSettings.useLegacyClips) {
				result.clip.legacy = true;
			}

			for (int i = 0; i < channels.Length; i++) {
				Channel channel = channels[i];
				if (samplers.Length <= channel.sampler) {
					Debug.LogWarning($"GLTFUtility: Animation channel points to sampler at index {channel.sampler} which doesn't exist. Skipping animation clip.");
					continue;
				}
				Sampler sampler = samplers[channel.sampler];

				ImportSettings.InterpolationMode GetInterpolationMode( string samplerInterpolationMode ) {
					if( importSettings.interpolationMode == ImportSettings.InterpolationMode.ImportFromFile ) {
						if( samplerInterpolationMode == "STEP" ) {
							return ImportSettings.InterpolationMode.Step;
						} else if( samplerInterpolationMode == "LINEAR" ) {
							return ImportSettings.InterpolationMode.Linear;
						} else if( samplerInterpolationMode == "CUBICSPLINE" ) {
							return ImportSettings.InterpolationMode.CubicSpline;
						} else {
							Debug.LogWarning( $"Unsupported interpolation mode: {samplerInterpolationMode}. Defaulting to STEP." );
							return ImportSettings.InterpolationMode.Step;
						}
					} else {
						return importSettings.interpolationMode;
					}
				}
				var interpolationMode = GetInterpolationMode( sampler.interpolation );

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
							posX.AddKey( CreateKeyframe( keyframeInput[k], -pos[k].x, interpolationMode ) );
							posY.AddKey( CreateKeyframe( keyframeInput[k], pos[k].y, interpolationMode ) );
							posZ.AddKey( CreateKeyframe( keyframeInput[k], pos[k].z, interpolationMode ) );
						}
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
						result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
						break;
					case "rotation":
						Vector4[] rot = accessors[sampler.output].ReadVec4().ToArray();

						bool willProcessSteppedKeyframes = interpolationMode == ImportSettings.InterpolationMode.Step && Application.isEditor && !Application.isPlaying;

						// @HACK: Creating stepped tangent keyframes is only supported in-editor -- not at runtime (Unity API restriction)
						#if UNITY_EDITOR // 🤢🤮💔
						if( willProcessSteppedKeyframes ) {
							AnimationCurve rotX = new AnimationCurve();
							AnimationCurve rotY = new AnimationCurve();
							AnimationCurve rotZ = new AnimationCurve();
							for (int k = 0; k < keyframeInput.Length; k++) {
								Vector3 eulerRotation = new Quaternion( rot[ k ].x, -rot[ k ].y, -rot[ k ].z, rot[ k ].w ).eulerAngles;

								rotX.AddKey( CreateKeyframe( keyframeInput[ k ], eulerRotation.x, interpolationMode ) );
								rotY.AddKey( CreateKeyframe( keyframeInput[ k ], eulerRotation.y, interpolationMode ) );
								rotZ.AddKey( CreateKeyframe( keyframeInput[ k ], eulerRotation.z, interpolationMode ) );
							}

							EditorCurveBinding GetEditorBinding( string property ) {
								return EditorCurveBinding.DiscreteCurve( relativePath, typeof( Transform ), property );
							}

							// Null out any other euler rotation curves on this clip, just to be safe.
							// https://forum.unity.com/threads/new-animationclip-property-names.367288/#post-2384172
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAngles.x" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAngles.y" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAngles.z" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAngles.x" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAngles.y" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAngles.z" ), null );

							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesBaked.x" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesBaked.y" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesBaked.z" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAnglesBaked.x" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAnglesBaked.y" ), null );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "m_LocalEulerAnglesBaked.z" ), null );


							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesRaw.x" ), rotX );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesRaw.y" ), rotY );
							AnimationUtility.SetEditorCurve( result.clip, GetEditorBinding( "localEulerAnglesRaw.z" ), rotZ );
						}
						#endif

						if( !willProcessSteppedKeyframes ) {
							AnimationCurve rotX = new AnimationCurve();
							AnimationCurve rotY = new AnimationCurve();
							AnimationCurve rotZ = new AnimationCurve();
							AnimationCurve rotW = new AnimationCurve();
							for (int k = 0; k < keyframeInput.Length; k++) {
								rotX.AddKey( CreateKeyframe( keyframeInput[k], rot[k].x, interpolationMode ) );
								rotY.AddKey( CreateKeyframe( keyframeInput[k], -rot[k].y, interpolationMode ) );
								rotZ.AddKey( CreateKeyframe( keyframeInput[k], -rot[k].z, interpolationMode ) );
								rotW.AddKey( CreateKeyframe( keyframeInput[k], rot[k].w, interpolationMode ) );
							}

							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
						}

						break;
					case "scale":
						Vector3[] scale = accessors[sampler.output].ReadVec3().ToArray();
						AnimationCurve scaleX = new AnimationCurve();
						AnimationCurve scaleY = new AnimationCurve();
						AnimationCurve scaleZ = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							scaleX.AddKey( CreateKeyframe( keyframeInput[k], scale[k].x, interpolationMode ) );
							scaleY.AddKey( CreateKeyframe( keyframeInput[k], scale[k].y, interpolationMode ) );
							scaleZ.AddKey( CreateKeyframe( keyframeInput[k], scale[k].z, interpolationMode ) );
						}
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.x", scaleX);
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.y", scaleY);
						result.clip.SetCurve(relativePath, typeof(Transform), "localScale.z", scaleZ);
						break;
					case "weights":
						GLTFNode.ImportResult skinnedMeshNode = nodes[ channel.target.node.Value ];
						SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshNode.transform.GetComponent<SkinnedMeshRenderer>();

						int numberOfBlendShapes = skinnedMeshRenderer.sharedMesh.blendShapeCount;
						AnimationCurve[] blendShapeCurves = new AnimationCurve[ numberOfBlendShapes ];
						for( int j = 0; j < numberOfBlendShapes; ++j ) {
							blendShapeCurves[ j ] = new AnimationCurve();
						}

						float[] weights = accessors[ sampler.output ].ReadFloat().ToArray();

						float[] previouslyKeyedValues = new float[ numberOfBlendShapes ];

						// Reference for my future self:
						// keyframeInput.Length = number of keyframes
						// keyframeInput[ k ] = timestamp of keyframe
						// weights.Length = number of keyframes * number of blendshapes
						// weights[ j ] = actual animated weight of a specific blend shape
						// (index into weights[] array accounts for keyframe index and blend shape index)

						for( int k = 0; k < keyframeInput.Length; ++k ) {
							for( int j = 0; j < numberOfBlendShapes; ++j ) {
								int weightIndex = ( k * numberOfBlendShapes ) + j;
								float weightValue = weights[ weightIndex ];

								bool addKey = true;
								if( importSettings.compressBlendShapeKeyFrames ) {
									if( k == 0 || !Mathf.Approximately( weightValue, previouslyKeyedValues[ j ] ) ) {
										previouslyKeyedValues[ j ] = weightValue;
										addKey = true;
									} else {
										addKey = false;
									}
								}

								if( addKey ) {
									blendShapeCurves[ j ].AddKey( CreateKeyframe( keyframeInput[ k ], weightValue, interpolationMode ) );
								}
							}
						}

						for( int j = 0; j < numberOfBlendShapes; ++j ) {
							string propertyName = "blendShape." + skinnedMeshRenderer.sharedMesh.GetBlendShapeName( j );
							result.clip.SetCurve( relativePath, typeof( SkinnedMeshRenderer ), propertyName, blendShapeCurves[ j ] );
						}

						break;
				}
			}
			return result;
		}

		public static Keyframe CreateKeyframe( float time, float value, ImportSettings.InterpolationMode interpolationMode ) {
			float inTangent = 0;
			float outTangent = 0;
			float inWeight = 0;
			float outWeight = 0;
			WeightedMode weightedMode = WeightedMode.None;
			bool isKeyBroken = false;

			if( interpolationMode == ImportSettings.InterpolationMode.Step ) {
				inTangent = float.PositiveInfinity;
				inWeight = 1;
				outTangent = float.PositiveInfinity;
				outWeight = 1;
				weightedMode = WeightedMode.Both;
				isKeyBroken = true;
			} else if( interpolationMode == ImportSettings.InterpolationMode.CubicSpline ) {
				// @TODO: Find out what the right math is to calculate the tangent/weight values.
				// For now, just let Unity do whatever it does by default :)
				return new Keyframe( time, value );
			} else if( interpolationMode == ImportSettings.InterpolationMode.Linear ) {
				inTangent = 0;
				inWeight = 0;
				outTangent = 0;
				outWeight = 0;
				weightedMode = WeightedMode.Both;
				isKeyBroken = true;
			}

			Keyframe keyframe = new Keyframe( time, value, inTangent, outTangent, inWeight, outWeight );
			keyframe.weightedMode = weightedMode;
			if( isKeyBroken ) {
				#pragma warning disable CS0618
				// https://github.com/unity-cn/CustomAnimationTools/blob/0b3b340c0880452949c34355052d9efb02990d4b/AnimationTools/Assets/Editor/CustomAnimationTools.cs#L164
				keyframe.tangentMode |= ( 1 << 0 );
				#pragma warning restore CS0618
			}
			return keyframe;
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