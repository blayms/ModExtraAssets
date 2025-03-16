# ModExtraAssets

A tool made specifically to make the process of loading custom assets much easier.
Highly recommended for use with Unity Engine BepInEx plugins.

- Supports \*.zip files (Stable.)
- Supports \*.vfp files (Recently released and highly untested but has been proven to work for the most part.)

# Documentation

This tool has documentation outside GitHub. Check it [here](https://sites.google.com/view/mea-docs/main).

# Supported file types

ModExtraAssets can recognize some file extensions to create a specific type of asset.

| Extension       | Zip Condition                                                                                                                                                                             | Vfp Condition | Return type                    |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------------------------ |
| \*.png         | None                                                                                                                                                                                    | None          | UnityEngine.Texture2D         |
| \*.png         | File name must contain !number at the end to define Sprite.pixelsPerUnit value.                                                                                                          | File must contain a metadata pair **(Key: meaTexType, Value: sprite!{number to define Sprite.pixelsPerUnit value})**.          | UnityEngine.Sprite            |
| \*.png         | File name must contain !c at the end to differentiate it from other \*.png files.                                                                                                        | File must contain a metadata pair **(Key: meaTexType, Value: cubemap)**.          | UnityEngine.Cubemap           |
| \*.png         | File name must contain !sheet at the end to differentiate it from other \*.png files and must also include a [JSON file](https://sites.google.com/view/mea-docs/main/useful-information/loading-sprite-sheets). | File must contain a metadata pair **(Key: meaTexType, Value: sheet)**. It can use a separate JSON file or another metadata pair. Please [visit docs (step 5)](https://sites.google.com/view/mea-docs/main/useful-information/loading-sprite-sheets) for more information.             | Blayms.MEA.SpriteSheetMEA     |
| \*.wav         | None                                                                                                                                                                                    | None          | UnityEngine.AudioClip         |
| \*.obj         | None                                                                                                                                                                                    | None             | UnityEngine.Mesh              |
| \*.json        | [Referencing DLLs](https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial)                                                                                        | [Referencing DLLs](https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial)          | Any deserializable type          |
| \*.txt or \*.text | None                                                                                                                                                                                | None             | System.String                 |
| \*.dll         | [\*.zip configuration](https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration)                                         | [\*.vfp configuration](https://sites.google.com/view/mea-docs/main/useful-information/vfp-configuration) | System.Reflection.Assembly |

# A couple of code snippets

Example: Loading database using a zip file with [Newtonsoft.Json](https://www.newtonsoft.com/json) library

    MEAZipLoadingProcedure proc = Blayms.MEA.ModExtraAssets.CreateLoadingProcedure<MEAZipLoadingProcedure>(@"D:\mycoolpluginfolder\myCoolZip.zip", this,
    (string json, Type type, object[] extraData) =>
    {
        return JsonConvert.DeserializeObject(json, type, (JsonSerializerSettings)extraData[0]);
    }, new object[]{MyCoolClass.GetJsonSerializerSettingsFor<LocalizationData>()});
    proc.Initiate();

Example: Perform actions after all assets have finished loading from a \*.zip file

    // CREATE THE PROCEDURE
    MEAZipLoadingProcedure meaZipLoadingProcedure = ModExtraAssets.CreateLoadingProcedure<MEAZipLoadingProcedure>
    (Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    "zip.zip"), BasePlugin.Instance, (string json, Type type, object[] extraData)
    => JsonConvert.DeserializeObject(json, type, Class.MyJsonSerializerSettings(type)), null);

    meaZipLoadingProcedure.onLoadingResultDefined +=
    delegate (MEAZipLoadingProcedure.LoadingResult loadingResult) // Subscribe to the event
    {
       if(loadingResult == MEAZipLoadingProcedure.LoadingResult.Success)
       {
            // CODE
       }
    };
    meaZipLoadingProcedure.Initiate(); // START LOADING ASSETS

# Credits

This tool was made by [Blayms](https://blayms.github.io/about-me/) and uses some utilities found online, such as:

- [ICSharpCode.SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
- [deadlyfingers/UnityWav](https://github.com/deadlyfingers/UnityWav)
- [Dummiesman/ObjLoader](https://github.com/PhalanxHead/UnityRuntimeOBJLoaderDocs)

