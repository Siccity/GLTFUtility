using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    [Serializable]
    public class GLTFSkin : GLTFProperty {
        public int[] bindShapeMatrix = new int[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        public int inverseBindMatrices;
        public int[] joints;
        public int skeleton = -1;

        public override void Load() {

        }

        public Matrix4x4 GetBindShapeMatrix() {
            return new Matrix4x4(
                new Vector4(bindShapeMatrix[0], bindShapeMatrix[1], bindShapeMatrix[2], bindShapeMatrix[3]),
                new Vector4(bindShapeMatrix[4], bindShapeMatrix[5], bindShapeMatrix[6], bindShapeMatrix[7]),
                new Vector4(bindShapeMatrix[8], bindShapeMatrix[9], bindShapeMatrix[10], bindShapeMatrix[11]),
                new Vector4(bindShapeMatrix[12], bindShapeMatrix[13], bindShapeMatrix[14], bindShapeMatrix[15])
            );
        }
    }
}