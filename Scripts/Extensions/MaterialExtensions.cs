using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siccity.GLTFUtility
{
	public static class MaterialExtensions
	{
		private static readonly Dictionary<string, int> _propertyCache = new Dictionary<string, int>();

		public static bool TrySetTexture(this Material material, string property, Texture value)
		{
			return InternalSet(material, property, value, material.SetTexture);
		}

		public static bool TrySetColor(this Material material, string property, Color value)
		{
			return InternalSet(material, property, value, material.SetColor);
		}

		public static bool TrySetInt(this Material material, string property, int value)
		{
			return InternalSet(material, property, value, material.SetInt);
		}

		public static bool TrySetFloat(this Material material, string property, float value)
		{
			return InternalSet(material, property, value, material.SetFloat);
		}

		private static bool InternalSet<T>(Material material, string property, T value, Action<int, T> setter)
		{
			int propertyId = GetPropertyId(property);

			if (!material.HasProperty(propertyId))
				return false;

			setter(propertyId, value);
			return true;
		}

		private static int GetPropertyId(string property)
		{
			int propertyId;
			if (_propertyCache.TryGetValue(property, out propertyId))
				return propertyId;

			propertyId = Shader.PropertyToID(property);
			_propertyCache.Add(property, propertyId);

			return propertyId;
		}
	}
}