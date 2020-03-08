## GLTFUtility
This project adds a few animation features into [Siccity](https://github.com/Siccity)'s excellent [GLTFUtility](https://github.com/Siccity/GLTFUtility):

* Animate blend shapes
* Animate with stepped and linear interpolation modes
* Force animations to use a specific interpolation mode
* Cut out redundant blend shape animation key frames
* Set a custom frame rate on imported animation clips
* Don't worry about installing package dependencies

### Installation
Open Unity's Package Manager and add the package with the git URL: <https://github.com/sprylyltd/GLTFUtility.git>.

### Limitations
* Animations that used stepped interpolation on rotation curves can only be imported in-editor (it requires an editor-only API call).

### Learn More
Visit the original [GLTFUtility](https://github.com/Siccity/GLTFUtility) repository for more information.