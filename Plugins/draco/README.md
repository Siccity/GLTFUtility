Description
===========

This folder contains resources for building a simple demo decompressing and rendering Draco within Unity.

If you are looking for more information on using Draco within Unity, this site [https://gitlab.com/atteneder/DracoUnity](https://gitlab.com/atteneder/DracoUnity) is a much better resource. There are more samples as well as support for more platforms.

In this folder we currently support two types of usages:
* Import Draco compressed mesh as assets during design time.
* Load/decode Draco files in runtime.

Prerequisite
============

To start, you need to have the Draco unity plugin. You can either use the
prebuilt libraries provided in this folder or build from source.
Note that the plugin library for different platforms has different file extension.

| Platform | Library name |
| -------- | ------------ |
| Mac OS | dracodec_unity.bundle |
| Android | libdracodec_unity.so |
| Windows | dracodec_unity.dll |

Prebuilt Library
----------------

We have built library for several platforms:

| Platform | Tested Environment |
| -------- | ------------------ |
| .bundle  | macOS Sierra + Xcode 8.3.3 |
| armeabi-v7a(.so) | Android 8.1.0 |
| .dll | Win10 + Visual Studio 2017 |

Build From Source
-----------------
1. To build the source code first you need to download the repo from [Here](https://github.com/google/draco).
2. Next to get a cleaner toolchain that generates an XCode project I recommend downloading the cmake file from [this repo.](https://github.com/leetal/ios-cmake)
3. Install CMake if not already installed on you mac from [Here](https://cmake.org/download/). Make sure to add CMake to your path.
4. Open a terminal and navigate to the directory where you downloaded and/or unzipped the Draco repo.
5. Create a new build directory and navigate into that folder.
6. To create an XCode project with the downloaded toolchain enter the following command:
~~~~ bash
cmake ../ -G Xcode -DCMAKE_TOOLCHAIN_FILE={PATH_TO_DOWNLOADED_TOOLCHAIN} -DPLATFORM=OS64 -DDRACO_UNITY_PLUGIN=1
~~~~
7. This should generate an XCode project in the build folder you created. Open this XCode Project.
8. To build in release mode, first you want to go to Product->Scheme->Edit Scheme. And under Run switch Build Configuration to Release. Close this window.
9. Next your going to want to build the `dracodec_unity`. Select this from the available build targets and press build.
10. After this is complete search for where the `libdracodec_unity.a` was placed by opening the Products drop down and right-clicking the `libdracodec_unity.a` and selecting Show in Finder.
11. There should be 2 files in the output directory, `libdracodec_unity.a` and `libdraco.a`. These are the new plugin files to replace the old ones in Plugins/draco/Plugin/iOS/
12. Your done!


Create Draco Demo Unity Project
===============================

Create a new 3D project in Unity.

Copy Library to Your Project
----------------------------
Copy the plugin library to your Unity project in `Assets/Plugins/`.
For Android Arm7:

~~~~ bash
cp path/to/your/libdracodec_unity.so path/to/your/Unity/Project/Assets/Plugins/Android/libs/armeabi-v7a/
~~~~

For Android Arm8:

~~~~ bash
cp path/to/your/libdracodec_unity.so path/to/your/Unity/Project/Assets/Plugins/Android/libs/arm64-v8a/
~~~~

For Mac:

~~~~ bash
cp path/to/your/dracodec_unity.bundle path/to/your/Unity/Project/Assets/Plugins/
~~~~

For Win:

~~~~ bash
cp path/to/your/dracodec_unity.dll path/to/your/Unity/Project/Assets/Plugins/
~~~~


Copy Unity Scripts to Your Project
----------------------------------

~~~~ bash
cp unity/DracoDecodingObject.cs path/to/your/Unity/Project/Assets/
cp unity/DracoMeshLoader.cs path/to/your/Unity/Project/Assets/
~~~~

Player Settings Change
-------------------------------
Open player settings. Make sure `Allow unsafe code` is checked, so Unity can load our plug-ins.

Copy Draco Mesh to Your Project
-------------------------------
Because Unity can only recognize file extensions known to Unity, you need to change your compressed .drc file to .drc.bytes so that Unity will recognize it as a binary file. For example, if you have file `bunny.drc` then change the file name to `bunny.drc.bytes`.

~~~~ bash
cp path/to/your/bunny.drc path/to/your/Unity/Project/Assets/Resources/bunny.drc.bytes
~~~~


---

Load Draco Assets in Runtime
============================
For example, please see [DracoDecodingObject.cs](DracoDecodingObject.cs) for usage. To start, you can create an empty GameObject and attach this script to it. [DracoDecodingObject.cs](DracoDecodingObject.cs) will load `bunny.drc.bytes` by default.

Enable Library in Script Debugging
----------------------------------
If you have library for the platform you are working on, e.g. `dracodec_unity.bundle` for Mac or `dracodec_unity.dll` for  Windows. You should be able to use the plugin in debugging mode.

---

Import Compressed Draco Assets
==============================
In this section we will describe how to import Draco files (.drc) to Unity as
other 3D formats at design time, e.g. obj, fbx.
Note that importing Draco files doesn't mean the Unity project will export models as Draco files.

Copy [DracoFileImporter.cs](Editor/DracoFileImporter.cs) which enables loading (This file is only used for import Draco files):

~~~~ bash
cp DracoFileImporter.cs path/to/your/Unity/Project/Assets/Editor/
~~~~

If you have followed the previous steps, you just need to copy your asset, e.g. `bunny.drc.bytes`, to `Your/Unity/Project/Assets/Resources`, the project will automatically load the file and add the models to the project.

---
