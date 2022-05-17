using System;

namespace Magnopus.GLTFUtility
{
	[Serializable]
	public class MeshSettings
	{
        public bool asyncMeshCreation = true;
        public bool asyncBoundsGeneration = true;
        public bool asyncNormalsGeneration = true;
        public bool asyncTangentsGeneration = true;
	}
}
