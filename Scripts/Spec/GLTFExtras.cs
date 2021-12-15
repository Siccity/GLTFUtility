using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class GLTFExtrasProcessor
	{
		public virtual void ProcessExtras(IDictionary<string, JToken> extras)
		{
		}
	}
}