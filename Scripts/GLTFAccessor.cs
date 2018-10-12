using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Reads data from BufferViews </summary>
    [Serializable]
    public class GLTFAccessor {
        public int bufferView = -1;
        public int byteOffset = -1;
        public string type;
        public GLType componentType = GLType.UNSET;
        public int count = -1;
        public float[] min;
        public float[] max;
        public Sparse sparse;
        public Indices indices;

        public Matrix4x4[] ReadMatrix4x4(GLTFObject gLTFObject) {
            if (type != "MAT4") {
                Debug.LogError("Type mismatch! Expected MAT4 got " + type);
                return new Matrix4x4[count];
            }

            Matrix4x4[] m = new Matrix4x4[count];
            byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                m[i].m00 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m01 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m02 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m03 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m10 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m11 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m12 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m13 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m20 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m21 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m22 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m23 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m30 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m31 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m32 = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                m[i].m33 = converter(bytes, startIndex);
            }
            return m;
        }

        public Vector4[] ReadVec4(GLTFObject gLTFObject) {
            if (type != "VEC4") {
                Debug.LogError("Type mismatch! Expected VEC4 got " + type);
                return new Vector4[count];
            }

            Vector4[] verts = new Vector4[count];
            byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].z = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].w = converter(bytes, startIndex);
            }
            return verts;
        }

        public Vector3[] ReadVec3(GLTFObject gLTFObject) {
            if (type != "VEC3") {
                Debug.LogError("Type mismatch! Expected VEC3 got " + type);
                return new Vector3[count];
            }

            Vector3[] verts = new Vector3[count];
            byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].z = converter(bytes, startIndex);
            }
            return verts;
        }

        public Vector2[] ReadVec2(GLTFObject gLTFObject) {
            if (type != "VEC2") {
                Debug.LogError("Type mismatch! Expected VEC2 got " + type);
                return new Vector2[count];
            }
            if (componentType != GLType.FLOAT) {
                Debug.LogError("Non-float componentType not supported. Got " + (int) componentType);
                return new Vector2[count];
            }

            Vector2[] verts = new Vector2[count];
            byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                verts[i].x = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
                verts[i].y = converter(bytes, startIndex);
                startIndex += GetComponentTypeSize(componentType);
            }
            return verts;
        }

        public int[] ReadInt(GLTFObject gLTFObject) {
            if (type != "SCALAR") {
                Debug.LogError("Type mismatch! Expected SCALAR got " + type);
                return new int[count];
            }
            if (componentType != GLType.UNSIGNED_SHORT) {
                Debug.LogError("Non-ushort componentType not supported. Got " + (int) componentType);
                return new int[count];
            }

            int[] ints = new int[count];
            byte[] bytes = gLTFObject.bufferViews[bufferView].GetBytes(gLTFObject);
            int componentSize = GetComponentSize();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                ints[i] = System.BitConverter.ToUInt16(bytes, startIndex);
            }
            return ints;
        }

        public Func<byte[], int, float> GetFloatConverter() {
            switch (componentType) {
                case GLType.BYTE:
                    return (x, y) =>(float) (sbyte) x[y];
                case GLType.UNSIGNED_BYTE:
                    return (x, y) =>(float) x[y];
                case GLType.FLOAT:
                    return System.BitConverter.ToSingle;
                case GLType.SHORT:
                    return (x, y) =>(float) System.BitConverter.ToInt16(x, y);
                case GLType.UNSIGNED_SHORT:
                    return (x, y) =>(float) System.BitConverter.ToUInt16(x, y);
                default:
                    Debug.LogWarning("No componentType defined");
                    return System.BitConverter.ToSingle;
            }
        }
        /// <summary> Get the size of the attribute type, in bytes </summary>
        public int GetComponentSize() {
            return GetComponentNumber(type) * GetComponentTypeSize(componentType);
        }

        public static int GetComponentTypeSize(GLType componentType) {
            switch (componentType) {
                case GLType.BYTE:
                    return 1;
                case GLType.UNSIGNED_BYTE:
                    return 1;
                case GLType.SHORT:
                    return 2;
                case GLType.UNSIGNED_SHORT:
                    return 2;
                case GLType.FLOAT:
                    return 4;
                default:
                    Debug.LogError("componentType " + (int) componentType + " not supported!");
                    return 0;
            }
        }

        public static int GetComponentNumber(string type) {
            switch (type) {
                case "SCALAR":
                    return 1;
                case "VEC2":
                    return 2;
                case "VEC3":
                    return 3;
                case "VEC4":
                    return 4;
                case "MAT2":
                    return 4;
                case "MAT3":
                    return 9;
                case "MAT4":
                    return 16;
                default:
                    Debug.LogError("type " + type + " not supported!");
                    return 0;
            }
        }

        [Serializable]
        public class Sparse {
            public int count = -1;
            public Values values;

            [Serializable]
            public class Values {
                public int bufferView = -1;
            }
        }

        [Serializable]
        public class Indices {
            public int bufferView = -1;
            public int componentType = -1;
        }
    }
}