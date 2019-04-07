using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFAnimation : GLTFProperty {

#region Serialized fields
        [SerializeField] private string name;
        public Sampler[] samplers = null;
        /// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
        public Channel[] channels = null;
#endregion

#region Non-serialized fields
        public AnimationClip Clip { get; private set; }
#endregion

#region Classes
        [Serializable]
        public class Sampler {
            /// <summary> The index of an accessor containing keyframe input values, e.g., time. </summary>
            public int input;
            /// <summary> Valid names include: "LINEAR", "STEP", "CUBICSPLINE" </summary>
            public string interpolation;
            /// <summary> The index of an accessor containing keyframe output values. </summary>
            public int output;
        }

        /// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
        [Serializable]
        public class Channel {
            /// <summary> Target sampler index </summary>
            public int sampler;
            /// <summary> Target sampler index </summary>
            public Target target;
        }

        /// <summary> Identifies which node and property to animate </summary>
        [Serializable]
        public class Target {
            /// <summary> Target node index. Ignore if -1 </summary>
            public int node = -1;
            /// <summary> Which property to animate. Valid names are: "translation", "rotation", "scale", "weights" </summary>
            public string path;
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
                GLTFNode node = glTFObject.nodes[channel.target.node];
                while (node != null && !node.IsRootTransform()) {
                    if (string.IsNullOrEmpty(relativePath)) relativePath = node.Name;
                    else relativePath = node.Name + "/" + relativePath;
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