using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFSkin {
        public int inverseBindMatrices = -1;
        public int[] joints;
        public int skeleton = -1;
    }
}