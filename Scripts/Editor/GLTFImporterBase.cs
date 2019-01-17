using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public abstract class GLTFImporterBase : ScriptedImporter {

        public void SaveToAsset(AssetImportContext ctx, GameObject[] roots) {
#if UNITY_2018_2_OR_NEWER
            if (roots.Length == 1) {
                ctx.AddObjectToAsset("main", roots[0]);
                ctx.SetMainObject(roots[0]);
            } else {
                GameObject root = new GameObject("Main");
                for (int i = 0; i < roots.Length; i++) {
                    roots[i].transform.parent = root.transform;
                }
                ctx.AddObjectToAsset("main", root);
                ctx.SetMainObject(root);
            }
#else
            if (roots.Length == 1) {
                ctx.SetMainAsset("main obj", roots[0]);
            } else {
                GameObject root = new GameObject("Main");
                for (int i = 0; i < roots.Length; i++) {
                    roots[i].transform.parent = root.transform;
                }
                ctx.SetMainAsset("main obj", root);
            }
#endif
        }

        public void AddMeshes(AssetImportContext ctx, GLTFObject gltfObject) {
#if UNITY_2018_2_OR_NEWER
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                ctx.AddObjectToAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetCachedMesh());
            }
#else
            for (int i = 0; i < glbObject.meshes.Count; i++) {
                ctx.AddSubAsset(glbObject.meshes[i].name, glbObject.meshes[i].GetCachedMesh());
            }
#endif
        }
    }
}