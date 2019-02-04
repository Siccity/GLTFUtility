using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFAnimation {
        public Sampler[] samplers = null;
        public Channel[] channels = null;

        [Serializable]
        public class Sampler {
            public int input;
            public string interpolation;
            public int output;
        }

        [Serializable]
        public class Channel {
            public int sampler;
            public Target target;
        }

        [Serializable]
        public class Target {
            public int node;
            public string path;
        }
    }
}