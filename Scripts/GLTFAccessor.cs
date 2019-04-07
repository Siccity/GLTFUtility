using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Reads data from BufferViews </summary>
    [Serializable]
    public class GLTFAccessor : GLTFProperty {

#region Serialized fields
        public int bufferView = -1;
        public int byteOffset = 0;
        public string type;
        public GLType componentType = GLType.UNSET;
        public int count = -1;
        public float[] min;
        public float[] max;
        public Sparse sparse;
        public Indices indices;
#endregion

        protected override bool OnLoad() {
            return true;
        }

        public Matrix4x4[] ReadMatrix4x4() {
            if (type != "MAT4") {
                Debug.LogError("Type mismatch! Expected MAT4 got " + type);
                return new Matrix4x4[count];
            }

            Matrix4x4[] m = new Matrix4x4[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
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

        public Vector4[] ReadVec4() {
            if (type != "VEC4") {
                Debug.LogError("Type mismatch! Expected VEC4 got " + type);
                return new Vector4[count];
            }

            Vector4[] verts = new Vector4[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
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

        public Color[] ReadColor() {
            if (type != "VEC4" && type != "VEC3") {
                Debug.LogError("Type mismatch! Expected VEC4 or VEC3 got " + type);
                return new Color[count];
            }

            Color[] colors = new Color[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            if (componentType == GLType.BYTE || componentType == GLType.UNSIGNED_BYTE) {
                Color32 color = Color.black;
                for (int i = 0; i < count; i++) {
                    int startIndex = i * componentSize;
                    color.r = bytes[startIndex];
                    startIndex += GetComponentTypeSize(componentType);
                    color.g = bytes[startIndex];
                    startIndex += GetComponentTypeSize(componentType);
                    color.b = bytes[startIndex];
                    if (type == "VEC4") {
                        startIndex += GetComponentTypeSize(componentType);
                        color.a = bytes[startIndex];
                    } else {
                        color.a = (byte) 255;
                    }
                    colors[i] = color;
                }
            } else if (componentType == GLType.FLOAT) {
                Func<byte[], int, float> converter = GetFloatConverter();
                for (int i = 0; i < count; i++) {
                    int startIndex = i * componentSize;
                    colors[i].r = converter(bytes, startIndex);
                    startIndex += GetComponentTypeSize(componentType);
                    colors[i].g = converter(bytes, startIndex);
                    startIndex += GetComponentTypeSize(componentType);
                    colors[i].b = converter(bytes, startIndex);
                    if (type == "VEC4") {
                        startIndex += GetComponentTypeSize(componentType);
                        colors[i].a = converter(bytes, startIndex);
                    } else {
                        colors[i].a = 1;
                    }
                }
            } else {
                Debug.LogWarning("Unexpected componentType! " + componentType);
            }

            return colors;
        }

        public Vector3[] ReadVec3() {
            if (type != "VEC3") {
                Debug.LogError("Type mismatch! Expected VEC3 got " + type);
                return new Vector3[count];
            }
            if (bufferView == -1) {
                Debug.LogError("Accessor bufferView was unassigned");
                return new Vector3[count];
            }

            Vector3[] verts = new Vector3[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
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

        public Vector2[] ReadVec2() {
            if (type != "VEC2") {
                Debug.LogError("Type mismatch! Expected VEC2 got " + type);
                return new Vector2[count];
            }
            if (componentType != GLType.FLOAT) {
                Debug.LogError("Non-float componentType not supported. Got " + (int) componentType);
                return new Vector2[count];
            }

            Vector2[] verts = new Vector2[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
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

        public float[] ReadFloat() {
            if (type != "SCALAR") {
                Debug.LogError("Type mismatch! Expected SCALAR got " + type);
                return new float[count];
            }

            float[] floats = new float[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, float> converter = GetFloatConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                floats[i] = converter(bytes, startIndex);
            }
            return floats;
        }

        public int[] ReadInt() {
            if (type != "SCALAR") {
                Debug.LogError("Type mismatch! Expected SCALAR got " + type);
                return new int[count];
            }

            int[] ints = new int[count];
            byte[] bytes = glTFObject.bufferViews[bufferView].GetBytes(byteOffset);
            int componentSize = GetComponentSize();
            Func<byte[], int, int> converter = GetIntConverter();
            for (int i = 0; i < count; i++) {
                int startIndex = i * componentSize;
                ints[i] = converter(bytes, startIndex);
            }
            return ints;
        }

        public Func<byte[], int, float> GetFloatConverter() {
            switch (componentType) {
                case GLType.BYTE:
                    return (x, y) => (float) (sbyte) x[y];
                case GLType.UNSIGNED_BYTE:
                    return (x, y) => (float) x[y];
                case GLType.FLOAT:
                    return System.BitConverter.ToSingle;
                case GLType.SHORT:
                    return (x, y) => (float) System.BitConverter.ToInt16(x, y);
                case GLType.UNSIGNED_SHORT:
                    return (x, y) => (float) System.BitConverter.ToUInt16(x, y);
                case GLType.UNSIGNED_INT:
                    return (x, y) => (float) System.BitConverter.ToUInt16(x, y);
                default:
                    Debug.LogWarning("No componentType defined");
                    return System.BitConverter.ToSingle;
            }
        }

        public Func<byte[], int, int> GetIntConverter() {
            switch (componentType) {
                case GLType.BYTE:
                    return (x, y) => (int) (sbyte) x[y];
                case GLType.UNSIGNED_BYTE:
                    return (x, y) => (int) x[y];
                case GLType.FLOAT:
                    return (x, y) => (int) System.BitConverter.ToSingle(x, y);
                case GLType.SHORT:
                    return (x, y) => (int) System.BitConverter.ToInt16(x, y);
                case GLType.UNSIGNED_SHORT:
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
                case GLType.UNSIGNED_INT:
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
                default:
                    Debug.LogWarning("No componentType defined");
                    return (x, y) => (int) System.BitConverter.ToUInt16(x, y);
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
                case GLType.UNSIGNED_INT:
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