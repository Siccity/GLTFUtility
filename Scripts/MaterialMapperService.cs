using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility
{
	public interface IMaterialMapper
	{
		void Apply(Material material, GLTFMaterial gltfMaterial, GLTFTexture.ImportResult[] textures);
	}

	public static class MaterialMapperService
	{
		public static IMaterialMapper defaultMapper { get; private set; }

		private static readonly Dictionary<string, IMaterialMapper> _mappers = new Dictionary<string, IMaterialMapper>();

		public static void SetDefaultMapper(IMaterialMapper materialMapper)
		{
			defaultMapper = materialMapper;
		}

		public static void RegisterMapper(string shaderName, IMaterialMapper materialMapper)
		{
			if (_mappers.ContainsKey(shaderName))
			{
				Debug.LogErrorFormat("Mapper for \"{0}\" was already been registered", shaderName);
				return;
			}

			_mappers.Add(shaderName, materialMapper);
		}

		public static bool ContainsMapper(string shaderName)
		{
			return _mappers.ContainsKey(shaderName);
		}

		public static bool TryGetMapper(string shaderName, out IMaterialMapper materialMapper, bool defaultIfNotFound = true)
		{
			if (_mappers.TryGetValue(shaderName, out materialMapper))
				return true;

			if (defaultIfNotFound && defaultMapper != null)
			{
				materialMapper = defaultMapper;
				return true;
			}

			return false;
		}
	}
}