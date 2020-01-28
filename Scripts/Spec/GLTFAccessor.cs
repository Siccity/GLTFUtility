using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Siccity.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Scripting;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#accessor
	/// <summary> Reads data from BufferViews </summary>
	[Preserve] public class GLTFAccessor {
#region Serialized fields
		public int? bufferView;
		public int byteOffset = 0;
		[JsonProperty(Required = Required.Always), JsonConverter(typeof(EnumConverter))] public AccessorType type;
		[JsonProperty(Required = Required.Always)] public GLType componentType;
		[JsonProperty(Required = Required.Always)] public int count;
		public float[] min;
		public float[] max;
		public Sparse sparse;
#endregion

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#sparse
		[Preserve] public class Sparse {
			[JsonProperty(Required = Required.Always)] public int count;
			[JsonProperty(Required = Required.Always)] public Indices indices;
			[JsonProperty(Required = Required.Always)] public Values values;

			// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#values
			[Preserve] public class Values {
				[JsonProperty(Required = Required.Always)] public int bufferView;
				public int byteOffset = 0;
			}

			// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#indices
			[Preserve] public class Indices {
				[JsonProperty(Required = Required.Always)] public int bufferView;
				[JsonProperty(Required = Required.Always)] public int componentType;
				public int byteOffset = 0;
			}
		}

#region Import
		public class ImportResult {
			public GLTFBufferView.ImportResult bufferView;
			public int? byteStride;
			public int count;
			public GLType componentType;
			public AccessorType type;
			public int byteOffset;

			public Matrix4x4[] ReadMatrix4x4() {
				if (!ValidateAccessorType(type, AccessorType.MAT4)) return new Matrix4x4[count];

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				Matrix4x4[] m = new Matrix4x4[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					m[i].m00 = floatReader(reader);
					m[i].m01 = floatReader(reader);
					m[i].m02 = floatReader(reader);
					m[i].m03 = floatReader(reader);
					m[i].m10 = floatReader(reader);
					m[i].m11 = floatReader(reader);
					m[i].m12 = floatReader(reader);
					m[i].m13 = floatReader(reader);
					m[i].m20 = floatReader(reader);
					m[i].m21 = floatReader(reader);
					m[i].m22 = floatReader(reader);
					m[i].m23 = floatReader(reader);
					m[i].m30 = floatReader(reader);
					m[i].m31 = floatReader(reader);
					m[i].m32 = floatReader(reader);
					m[i].m33 = floatReader(reader);
					reader.Skip(byteSkip);
				}
				return m;
			}

			public Vector4[] ReadVec4() {
				if (!ValidateAccessorType(type, AccessorType.VEC4)) return new Vector4[count];

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				Vector4[] verts = new Vector4[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					verts[i].x = floatReader(reader);
					verts[i].y = floatReader(reader);
					verts[i].z = floatReader(reader);
					verts[i].w = floatReader(reader);
					reader.Skip(byteSkip);
				}
				return verts;
			}

			public Color[] ReadColor() {
				if (!ValidateAccessorTypeAny(type, AccessorType.VEC3, AccessorType.VEC4)) return new Color[count];

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				Color[] cols = new Color[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				if (type == AccessorType.VEC3) {
					for (int i = 0; i < count; i++) {
						cols[i].r = floatReader(reader);
						cols[i].g = floatReader(reader);
						cols[i].b = floatReader(reader);
						reader.Skip(byteSkip);
					}
				} else if (type == AccessorType.VEC4) {
					for (int i = 0; i < count; i++) {
						cols[i].r = floatReader(reader);
						cols[i].g = floatReader(reader);
						cols[i].b = floatReader(reader);
						cols[i].a = floatReader(reader);
						reader.Skip(byteSkip);
					}
				}
				return cols;
			}

			public Vector3[] ReadVec3() {
				if (!ValidateAccessorType(type, AccessorType.VEC3)) return new Vector3[count];

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				Vector3[] verts = new Vector3[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					verts[i].x = floatReader(reader);
					verts[i].y = floatReader(reader);
					verts[i].z = floatReader(reader);
					reader.Skip(byteSkip);
				}
				return verts;
			}

			public Vector2[] ReadVec2() {
				if (!ValidateAccessorType(type, AccessorType.VEC2)) return new Vector2[count];
				if (componentType != GLType.FLOAT) {
					Debug.LogError("Non-float componentType not supported. Got " + (int) componentType);
					return new Vector2[count];
				}

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				Vector2[] verts = new Vector2[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					verts[i].x = floatReader(reader);
					verts[i].y = floatReader(reader);
					reader.Skip(byteSkip);
				}
				return verts;
			}

			public float[] ReadFloat() {
				if (!ValidateAccessorType(type, AccessorType.SCALAR)) return new float[count];

				Func<BufferedBinaryReader, float> floatReader = GetFloatReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				float[] result = new float[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					result[i] = floatReader(reader);
					reader.Skip(byteSkip);
				}
				return result;
			}

			public int[] ReadInt() {
				if (!ValidateAccessorType(type, AccessorType.SCALAR)) return new int[count];

				Func<BufferedBinaryReader, int> intReader = GetIntReader(componentType);
				BufferedBinaryReader reader = new BufferedBinaryReader(bufferView.stream, 1024);
				int[] result = new int[count];
				reader.Position = bufferView.byteOffset + byteOffset;
				int byteSkip = byteStride.HasValue ? byteStride.Value - GetComponentSize() : 0;
				for (int i = 0; i < count; i++) {
					result[i] = intReader(reader);
					reader.Skip(byteSkip);
				}
				return result;
			}

			public Func<BufferedBinaryReader, int> GetIntReader(GLType componentType) {
				Func<BufferedBinaryReader, int> readMethod;
				switch (componentType) {
					case GLType.BYTE:
						return x => x.ReadSByte();
					case GLType.UNSIGNED_BYTE:
						return readMethod = x => x.ReadByte();
					case GLType.FLOAT:
						return readMethod = x => (int) x.ReadSingle();
					case GLType.SHORT:
						return readMethod = x => x.ReadInt16();
					case GLType.UNSIGNED_SHORT:
						return readMethod = x => x.ReadUInt16();
					case GLType.UNSIGNED_INT:
						return readMethod = x => (int) x.ReadUInt32();
					default:
						Debug.LogWarning("No componentType defined");
						return readMethod = x => x.ReadInt32();
				}
			}

			public Func<BufferedBinaryReader, float> GetFloatReader(GLType componentType) {
				Func<BufferedBinaryReader, float> readMethod;
				switch (componentType) {
					case GLType.BYTE:
						return x => x.ReadSByte();
					case GLType.UNSIGNED_BYTE:
						return readMethod = x => x.ReadByte();
					case GLType.FLOAT:
						return readMethod = x => x.ReadSingle();
					case GLType.SHORT:
						return readMethod = x => x.ReadInt16();
					case GLType.UNSIGNED_SHORT:
						return readMethod = x => x.ReadUInt16();
					case GLType.UNSIGNED_INT:
						return readMethod = x => x.ReadUInt32();
					default:
						Debug.LogWarning("No componentType defined");
						return readMethod = x => x.ReadSingle();
				}
			}

			/// <summary> Get the size of the attribute type, in bytes </summary>
			public int GetComponentSize() {
				return GetComponentNumber() * GetComponentTypeSize();
			}

			public int GetComponentTypeSize() {
				switch (componentType) {
					case GLType.BYTE:
						return sizeof(sbyte);
					case GLType.UNSIGNED_BYTE:
						return sizeof(byte);
					case GLType.SHORT:
						return sizeof(short);
					case GLType.UNSIGNED_SHORT:
						return sizeof(ushort);
					case GLType.FLOAT:
						return sizeof(Single);
					case GLType.UNSIGNED_INT:
						return sizeof(uint);
					default:
						Debug.LogError("componentType " + (int) componentType + " not supported!");
						return 0;
				}
			}

			public int GetComponentNumber() {
				switch (type) {
					case AccessorType.SCALAR:
						return 1;
					case AccessorType.VEC2:
						return 2;
					case AccessorType.VEC3:
						return 3;
					case AccessorType.VEC4:
						return 4;
					case AccessorType.MAT2:
						return 4;
					case AccessorType.MAT3:
						return 9;
					case AccessorType.MAT4:
						return 16;
					default:
						Debug.LogError("type " + type + " not supported!");
						return 0;
				}
			}

			public static bool ValidateByteStride(int byteStride) {
				if (byteStride >= 4 && byteStride <= 252 && byteStride % 4 == 0) return true;
				Debug.Log("ByteStride of " + byteStride + " is invalid. Ignoring.");
				return false;
			}

			private static bool ValidateAccessorType(AccessorType type, AccessorType expected) {
				if (type == expected) return true;
				else {
					Debug.LogError("Type mismatch! Expected " + expected + " got " + type);
					return false;
				}
			}

			public static bool ValidateAccessorTypeAny(AccessorType type, params AccessorType[] expected) {
				for (int i = 0; i < expected.Length; i++) {
					if (type == expected[i]) return true;
				}
				Debug.Log("Type mismatch! Expected " + string.Join("or ", expected) + ", got " + type);
				return false;
			}
		}

		public ImportResult Import(GLTFBufferView.ImportResult[] bufferViews) {
			ImportResult result = new ImportResult();

			result.bufferView = bufferViews[this.bufferView.Value];
			result.componentType = componentType;
			result.type = type;
			result.count = count;
			result.byteOffset = byteOffset;
			// If an optional byteStride was added on bufferView in file, and it matches spec requirements
			if (result.bufferView.byteStride.HasValue && ImportResult.ValidateByteStride((int) result.bufferView.byteStride)) {
				result.byteStride = result.bufferView.byteStride;
			}
			return result;
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFAccessor> accessors, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask) {
				task = new Task(() => {
					Result = new ImportResult[accessors.Count];
					for (int i = 0; i < Result.Length; i++) {
						Result[i] = accessors[i].Import(bufferViewTask.Result);
					}
				});
			}
		}
#endregion
	}
}