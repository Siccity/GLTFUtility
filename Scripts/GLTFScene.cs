using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFScene : GLTFProperty {

#region Serialized fields
        /// <summary> Indices of nodes </summary>
        public List<int> nodes;
#endregion

        protected override bool OnLoad() {
            return true;
        }
    }
}