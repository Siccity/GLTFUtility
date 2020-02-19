[![Discord](https://img.shields.io/discord/361769369404964864.svg)](https://discord.gg/qgPrHv4)
[![GitHub issues](https://img.shields.io/github/issues/Siccity/GLTFUtility.svg)](https://github.com/Siccity/GLTFUtility/issues)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/Siccity/GLTFUtility/master/LICENSE.md)

## GLTFUtility
Allows you to import and export glTF files during runtime and in editor.
glTF is a new opensource 3d model transmission format which supports everything you'll ever need from a format in Unity.
[Read more about glTF here](https://www.khronos.org/gltf/)

![2019-04-01_00-46-27](https://user-images.githubusercontent.com/6402525/55296304-b2aa5880-5417-11e9-89a8-78ab540dc126.gif)
![image](https://user-images.githubusercontent.com/6402525/55296353-7297a580-5418-11e9-8e76-5078680ee0d3.png)
![image](https://user-images.githubusercontent.com/6402525/55296436-bd65ed00-5419-11e9-9723-31225b99450b.png)


### What makes GLTFUtility different?
Focusing on simplicity and ease of use, GLTFUtility aims to be an import-and-forget solution, keeping consistency with built-in functionality.

### Installation
1. GLTFUtility requires Newtonsoft Json to be installed in your project.
    * Official upm package: `"com.unity.nuget.newtonsoft-json": "2.0.0-preview",`
    * Unofficial Asset Store: [link](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)
    * Unofficial git repo: [link](https://github.com/jilleJr/Newtonsoft.Json-for-Unity)

2. Choose *one* of the following:
    * Download [.zip](https://github.com/Siccity/GLTFUtility/archive/master.zip) and extract to your project assets
    * Download latest [.unitypackage](https://github.com/Siccity/GLTFUtility/releases) and unpack to your projects assets
    * (git) Clone into your assets folder `git clone git@github.com:Siccity/GLTFUtility.git`
    * (git) Add repo as submodule `git submodule add git@github.com:Siccity/GLTFUtility.git Assets/Submodules/GLTFUtility`
    * (package) If using Unity 2018.3 or later, you can add a new entry to the manifest.json file in your Packages folder
  `"com.siccity.gltfutility": "https://github.com/siccity/gltfutility.git"` [Manual](https://docs.unity3d.com/Manual/upm-git.html)

**NOTICE** This is a work in progress. Expect bugs. Current features can be tracked below:

### Features
- [x] Editor import
- [ ] Editor export
- [x] Runtime import API
- [ ] Runtime export API
- [x] Static mesh (with submeshes)
- [x] UVs (up to 8 channels)
- [x] Normals
- [x] Tangents
- [x] Vertex colors
- [x] Materials (metallic/specular, opaque/mask/blend)
- [x] Textures (embedded/external)
- [x] Remote textures (during async only)
- [x] Rig
- [x] Animations (multiple)
- [x] Morph targets
- [x] Cameras
- [ ] Lights
- [ ] Draco compression
- [x] Multithreading
- [x] GLTF format
- [x] GLB format

### Runtime import API
```cs
// Single thread
using Siccity.GLTFUtility;

void ImportGLTF(string filepath) {
   GameObject result = Importer.LoadFromFile(filepath);
}
```
```cs
// Multithreaded
using Siccity.GLTFUtility;

void ImportGLTFAsync(string filepath) {
   Importer.ImportGLTFAsync(filepath, new ImportSettings(), OnFinishAsync);
}

void OnFinishAsync(GameObject result) {
   Debug.Log("Finished importing " + result.name);
}
```
