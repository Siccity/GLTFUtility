using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    public abstract class GLTFImporterBase : ScriptedImporter {

        public static Material defaultMaterial { get { return _defaultMaterial != null ? _defaultMaterial : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"); } }
        private static Material _defaultMaterial;

        public void SaveToAsset(AssetImportContext ctx, GameObject[] roots) {
#if UNITY_2018_2_OR_NEWER
            // Add GameObjects
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
            // Add GameObjects
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

        public void ApplyDefaultMaterial(GameObject[] roots) {
            MeshRenderer[] renderers = roots.SelectMany(x => x.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
            for (int i = 0; i < renderers.Length; i++) {
                Material[] mats = renderers[i].sharedMaterials;
                for (int k = 0; k < mats.Length; k++) {
                    if (mats[k] == null) mats[k] = defaultMaterial;
                }
                renderers[i].sharedMaterials = mats;
            }
        }

        public void AddMeshes(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.meshes.Count; i++) {
                Mesh mesh = gltfObject.meshes[i].GetMesh();
                if (mesh == null) {
                    Debug.LogWarning("Mesh at index " + i + " was null");
                    continue;
                }

#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(gltfObject.meshes[i].name, gltfObject.meshes[i].GetCachedMesh());
#else
                ctx.AddSubAsset(glbObject.meshes[i].name, glbObject.meshes[i].GetCachedMesh());
#endif
            }
        }

        public void AddMaterials(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.materials.Count; i++) {
                Material mat = gltfObject.materials[i].GetMaterial();
                if (string.IsNullOrEmpty(mat.name)) mat.name = "material" + i.ToString();

#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(mat.name, mat);
#else
                ctx.AddSubAsset(mat.name, mat);
#endif
            }
        }

        public void AddAnimations(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.animations.Count; i++) {
                AnimationClip clip = gltfObject.animations[i].Clip;
#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(clip.name, clip);
#else
                ctx.AddSubAsset(clip.name, clip);
#endif
            }
        }

        public void AddTextures(AssetImportContext ctx, GLTFObject gltfObject) {
            for (int i = 0; i < gltfObject.images.Count; i++) {
                // Dont add asset textures
                if (gltfObject.images[i].imageIsAsset) continue;

                Texture2D tex = gltfObject.images[i].GetTexture();
                if (tex == null) continue;
                if (string.IsNullOrEmpty(tex.name)) tex.name = "texture" + i.ToString();
#if UNITY_2018_2_OR_NEWER
                ctx.AddObjectToAsset(i.ToString(), tex);
#else
                ctx.AddSubAsset(i.ToString(), glbObject.images[i].GetTexture());
#endif
            }
        }
    }
}