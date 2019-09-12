using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
    /// <summary> Contains methods for saving a gameobject as an asset </summary>
    public static class GLTFAssetUtility {

        public static Material defaultMaterial { get { return _defaultMaterial != null ? _defaultMaterial : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat"); } }
        private static Material _defaultMaterial;

        public static void SaveToAsset(GameObject root, AssetImportContext ctx) {
#if UNITY_2018_2_OR_NEWER
            ctx.AddObjectToAsset("main", root);
            ctx.SetMainObject(root);
#else
            ctx.SetMainAsset("main obj", root);
#endif
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            ApplyDefaultMaterial(renderers);
            AddMeshes(filters, ctx);
            AddMaterials(renderers, ctx);
        }

        private static void ApplyDefaultMaterial(MeshRenderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++) {
                Material[] mats = renderers[i].sharedMaterials;
                for (int k = 0; k < mats.Length; k++) {
                    if (mats[k] == null) mats[k] = defaultMaterial;
                }
                renderers[i].sharedMaterials = mats;
            }
        }

        public static void AddMeshes(MeshFilter[] filters, AssetImportContext ctx) {
            HashSet<Mesh> visitedMeshes = new HashSet<Mesh>();
            for (int i = 0; i < filters.Length; i++) {
                Mesh mesh = filters[i].sharedMesh;
                if (visitedMeshes.Contains(mesh)) continue;
                ctx.AddAsset(mesh.name, mesh);
                visitedMeshes.Add(mesh);
            }
        }

        public static void AddMaterials(MeshRenderer[] renderers, AssetImportContext ctx) {
            HashSet<Material> visitedMaterials = new HashSet<Material>();
            HashSet<Texture2D> visitedTextures = new HashSet<Texture2D>();
            for (int i = 0; i < renderers.Length; i++) {
                foreach (Material mat in renderers[i].sharedMaterials) {
                    if (visitedMaterials.Contains(mat)) continue;
                    if (string.IsNullOrEmpty(mat.name)) mat.name = "material" + visitedMaterials.Count;
                    ctx.AddAsset(mat.name, mat);
                    visitedMaterials.Add(mat);

                    // Add textures
                    foreach (Texture2D tex in mat.AllTextures()) {
                        // Dont add asset textures
                        //if (images[i].isAsset) continue;
                        if (visitedTextures.Contains(tex)) continue;
                        if (AssetDatabase.Contains(tex)) continue;
                        if (string.IsNullOrEmpty(tex.name)) tex.name = "texture" + visitedTextures.Count;
                        ctx.AddAsset(tex.name, tex);
                        visitedTextures.Add(tex);
                    }
                }
            }
        }

        public static void AddAsset(this AssetImportContext ctx, string identifier, Object obj) {
#if UNITY_2018_2_OR_NEWER
            ctx.AddObjectToAsset(identifier, obj);
#else
            ctx.AddSubAsset(identifier, obj);
#endif
        }

        public static IEnumerable<Texture2D> AllTextures(this Material mat) {
            int[] ids = mat.GetTexturePropertyNameIDs();
            for (int i = 0; i < ids.Length; i++) {
                Texture2D tex = mat.GetTexture(ids[i]) as Texture2D;
                if (tex != null) yield return tex;
            }
        }
    }
}