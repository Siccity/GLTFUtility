using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation
    /// <summary> Contains info for a single animation clip </summary>
    public class GLTFAnimation : GLTFProperty {

#region Serialized fields
        /// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
        [JsonProperty(Required = Required.Always)] public Channel[] channels;
        [JsonProperty(Required = Required.Always)] public Sampler[] samplers;
        public string name;
#endregion

#region Non-serialized fields
        [JsonIgnore] public AnimationClip Clip { get; private set; }
#endregion

#region Classes
        // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation-sampler
        public class Sampler {
            /// <summary> The index of an accessor containing keyframe input values, e.g., time. </summary>
            [JsonProperty(Required = Required.Always)] public int input;
            /// <summary> The index of an accessor containing keyframe output values. </summary>
            [JsonProperty(Required = Required.Always)] public int output;
            /// <summary> Valid names include: "LINEAR", "STEP", "CUBICSPLINE" </summary>
            public string interpolation = "LINEAR";
        }

        // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#channel
        /// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
        public class Channel {
            /// <summary> Target sampler index </summary>
            [JsonProperty(Required = Required.Always)] public int sampler;
            /// <summary> Target sampler index </summary>
            [JsonProperty(Required = Required.Always)] public Target target;
        }

        // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#target
        /// <summary> Identifies which node and property to animate </summary>
        public class Target {
            /// <summary> Target node index.</summary>
            public int? node;
            /// <summary> Which property to animate. Valid names are: "translation", "rotation", "scale", "weights" </summary>
            [JsonProperty(Required = Required.Always)] public string path;
        }
#endregion

        protected override bool OnLoad() {
            Clip = new AnimationClip();

            // Name
            if (string.IsNullOrEmpty(name)) Clip.name = "animation" + glTFObject.animations.IndexOf(this);
            else Clip.name = name;

            for (int i = 0; i < channels.Length; i++) {
                Channel channel = channels[i];
                if (samplers.Length <= channel.sampler) {
                    Debug.LogWarning("Animation channel points to sampler at index " + channel.sampler + " which doesn't exist. Skipping animation clip.");
                    continue;
                }
                Sampler sampler = samplers[channel.sampler];

                string relativePath = "";
                GLTFNode node = glTFObject.nodes[channel.target.node.Value];
                while (node != null && !node.IsRootTransform()) {
                    if (string.IsNullOrEmpty(relativePath)) relativePath = node.GetName();
                    else relativePath = node.GetName() + "/" + relativePath;
                    node = node.GetParentNode();
                }

                float[] keyframeInput = glTFObject.accessors[sampler.input].ReadFloat().ToArray();
                switch (channel.target.path) {
                    case "translation":
                        Vector3[] pos = glTFObject.accessors[sampler.output].ReadVec3().ToArray();
                        AnimationCurve posX = new AnimationCurve();
                        AnimationCurve posY = new AnimationCurve();
                        AnimationCurve posZ = new AnimationCurve();
                        for (int k = 0; k < keyframeInput.Length; k++) {
                            posX.AddKey(keyframeInput[k], pos[k].x);
                            posY.AddKey(keyframeInput[k], pos[k].y);
                            posZ.AddKey(keyframeInput[k], -pos[k].z);
                        }
                        Clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
                        Clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
                        Clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
                        break;
                    case "rotation":
                        Vector4[] rot = glTFObject.accessors[sampler.output].ReadVec4().ToArray();
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
                        Clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
                        Clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
                        Clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
                        Clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
                        break;
                    case "scale":
                        Vector3[] scale = glTFObject.accessors[sampler.output].ReadVec3().ToArray();
                        AnimationCurve scaleX = new AnimationCurve();
                        AnimationCurve scaleY = new AnimationCurve();
                        AnimationCurve scaleZ = new AnimationCurve();
                        for (int k = 0; k < keyframeInput.Length; k++) {
                            scaleX.AddKey(keyframeInput[k], scale[k].x);
                            scaleY.AddKey(keyframeInput[k], scale[k].y);
                            scaleZ.AddKey(keyframeInput[k], scale[k].z);
                        }
                        Clip.SetCurve(relativePath, typeof(Transform), "localScale.x", scaleX);
                        Clip.SetCurve(relativePath, typeof(Transform), "localScale.y", scaleY);
                        Clip.SetCurve(relativePath, typeof(Transform), "localScale.z", scaleZ);
                        break;
                    case "weights":
                        Debug.LogWarning("morph weights not supported");
                        break;
                }
            }
            return true;
        }
    }
}