using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Siccity.GLTFUtility.Converters {
	public class ExtensionsConverter : JsonConverter {
		public static Dictionary<string, Type> extensionTypes {
			get {
				if (_extensionTypes == null) Initialize();
				return _extensionTypes;
			}
		}
		private static Dictionary<string, Type> _extensionTypes;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			Dictionary<string, object> extensions = (Dictionary<string, object>) value;
			writer.WriteStartArray();
			foreach (var kvp in extensions) {
				writer.WritePropertyName(kvp.Key);
				writer.WriteStartObject();
				writer.WriteValue(kvp.Value);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			Dictionary<string, IExtension> extensions = new Dictionary<string, IExtension>();

			Dictionary<string, JObject> dict = serializer.Deserialize<Dictionary<string, JObject>>(reader);
			foreach (var kvp in dict) {
				string extensionId = kvp.Key;
				JObject extensionJObject = kvp.Value;

				Type extensionType;
				if (extensionTypes.TryGetValue(extensionId, out extensionType)) {
					IExtension extension = JsonConvert.DeserializeObject(extensionJObject.ToString(), extensionType) as IExtension;
					extensions.Add(extensionId, extension);
				}
			}
			return extensions;
		}

		public override bool CanConvert(Type objectType) {
			return true;
			//return objectType == typeof(Dictionary<string, object>);
		}

		/// <summary> Get all classes deriving from baseType via reflection </summary>
		public static void Initialize() {
			_extensionTypes = new Dictionary<string, Type>();

			Type extensionInterface = typeof(IExtension);
			List<System.Type> types = new List<System.Type>();
			System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies) {
				try {
					types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && extensionInterface.IsAssignableFrom(t)).ToArray());
				} catch (ReflectionTypeLoadException) { }
			}
			foreach (System.Type type in types) {
				ExtensionAttribute attrib = type.GetCustomAttribute<ExtensionAttribute>();
				if (attrib != null) {
					_extensionTypes.Add(attrib.extension, type);
				}
			}
		}
	}
}

namespace Siccity.GLTFUtility {
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	class ExtensionAttribute : System.Attribute {
		public string extension;

		public ExtensionAttribute(string extension) {
			this.extension = extension;
		}
	}

	public interface IExtension {
		void TaskedWork(params object[] parms);
		void MainThreadWork<T>(ref T Result) where T : class;
	}
}