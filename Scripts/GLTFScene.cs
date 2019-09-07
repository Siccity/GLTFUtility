using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Siccity.GLTFUtility {
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