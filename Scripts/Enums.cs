namespace Siccity.GLTFUtility {
	public enum AlphaMode { OPAQUE, MASK, BLEND }
	public enum AccessorType { SCALAR, VEC2, VEC3, VEC4, MAT2, MAT3, MAT4 }
	public enum RenderingMode { POINTS = 1, LINES = 2, LINE_LOOP = 3, TRIANGLES = 4, TRIANGLE_STRIP = 5, TRIANGLE_FAN = 6 }
	public enum GLType { UNSET = -1, BYTE = 5120, UNSIGNED_BYTE = 5121, SHORT = 5122, UNSIGNED_SHORT = 5123, UNSIGNED_INT = 5125, FLOAT = 5126 }
	public enum Format { AUTO, GLTF, GLB }
}