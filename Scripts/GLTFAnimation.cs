using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFAnimation {
        public Sampler[] samplers = null;
        /// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
        public Channel[] channels = null;

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

        public AnimationClip GetAnimationClip(GLTFObject gLTFObject) {
            AnimationClip clip = new AnimationClip();
            for (int i = 0; i < channels.Length; i++) {
                Channel channel = channels[i];
                if (samplers.Length <= channel.sampler) {
                    Debug.LogWarning("Animation channel points to sampler at index " + channel.sampler + " which doesn't exist. Skipping animation clip.");
                    continue;
                }
                Sampler sampler = samplers[channel.sampler];

                string relativePath = "";
                GLTFNode node = gLTFObject.nodes[channel.target.node];
                while (node != null && !node.IsRootTransform(gLTFObject)) {
                    if (string.IsNullOrEmpty(relativePath)) relativePath = node.name;
                    else relativePath = node.name + "/" + relativePath;
                    node = node.GetParentNode(gLTFObject);
                }

                float[] keyframeInput = gLTFObject.accessors[sampler.input].ReadFloat(gLTFObject).ToArray();
                switch (channel.target.path) {
                    case "translation":
                        Vector3[] pos = gLTFObject.accessors[sampler.output].ReadVec3(gLTFObject).ToArray();
                        AnimationCurve posX = new AnimationCurve();
                        AnimationCurve posY = new AnimationCurve();
                        AnimationCurve posZ = new AnimationCurve();
                        for (int k = 0; k < keyframeInput.Length; k++) {
                            posX.AddKey(keyframeInput[k], pos[k].x);
                            posY.AddKey(keyframeInput[k], pos[k].y);
                            posZ.AddKey(keyframeInput[k], pos[k].z);
                        }
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
                        clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
                        break;
                    case "rotation":
                        Vector4[] rot = gLTFObject.accessors[sampler.output].ReadVec4(gLTFObject).ToArray();
                        AnimationCurve rotX = new AnimationCurve();
                        AnimationCurve rotY = new AnimationCurve();
                        AnimationCurve rotZ = new AnimationCurve();
                        AnimationCurve rotW = new AnimationCurve();
                        for (int k = 0; k < keyframeInput.Length; k++) {
                            rotX.AddKey(keyframeInput[k], rot[k].x);
                            rotY.AddKey(keyframeInput[k], rot[k].y);
                            rotZ.AddKey(keyframeInput[k], rot[k].z);
                            rotW.AddKey(keyframeInput[k], rot[k].w);
                        }
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
                        clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
                        break;
                    case "scale":
                        Vector3[] scale = gLTFObject.accessors[sampler.output].ReadVec3(gLTFObject).ToArray();
                        AnimationCurve scaleX = new AnimationCurve();
                        AnimationCurve scaleY = new AnimationCurve();
                        AnimationCurve scaleZ = new AnimationCurve();
                        for (int k = 0; k < keyframeInput.Length; k++) {
                            scaleX.AddKey(keyframeInput[k], scale[k].x);
                            scaleY.AddKey(keyframeInput[k], scale[k].y);
                            scaleZ.AddKey(keyframeInput[k], scale[k].z);
                        }
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.x", scaleX);
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.y", scaleY);
                        clip.SetCurve(relativePath, typeof(Transform), "localScale.z", scaleZ);
                        break;
                    case "weights":
                        Debug.Log("Not supported");
                        break;
                }
            }
            return clip;
        }
    }
}