
# ModExtraAssets

A tool made specificly to make a proccess of loading custom assets way easier
Highly recommended to be used for Unity Engine BepInEx plugins

- Supports \*.zip files (Stable)
- Supports *.vfp files (Recently released and highly untested, but proven to be mainly working)

# Documentation

This tool has a documentation outside GitHub, check it [here](https://sites.google.com/view/mea-docs/main)

# Supported file types

ModExtraAssets can recognize some file extension to create a specific asset of type

| Extension       | Zip Condition                                                                                                                                                                             | Vfp Condition | Return type                    |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------------------------ |
| \*.png         | None                                                                                                                                                                                    | None          | UnityEngine.Texture2D         |
| \*.png         | File name must contain !number at the end to define Sprite.pixelsPerUnit Value                                                                                                          | File must contains a metadata pair **(Key: meaTexType, Value: sprite!{number to define Sprite.pixelsPerUnit Value})**.          | UnityEngine.Sprite            |
| \*.png         | File name must contain !c at the end to differentiate it from other \*.png files                                                                                                        | File must contains a metadata pair **(Key: meaTexType, Value: cubemap)**          | UnityEngine.Cubemap           |
| \*.png         | File name must contain !sheet at the end to differentiate it from other \*.png files and also requires a [json file](https://sites.google.com/view/mea-docs/main/useful-information/loading-sprite-sheets) | File must contains a metadata pair **(Key: meaTexType, Value: sheet)**. Could use a separate json file or another metadata pair. Please [visit docs (step 5)](https://sites.google.com/view/mea-docs/main/useful-information/loading-sprite-sheets) for more information             | Blayms.MEA.SpriteSheetMEA     |
| \*.wav         | None                                                                                                                                                                                    | None          | UnityEngine.AudioClip         |
| \*.obj         | None                                                                                                                                                                                    | None             | UnityEngine.Mesh              |
| \*.json        | [Referencing DLLs](https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial)                                                                                        | [Referencing DLLs](https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial)          | Any _deserializable_          |
| \*.txt or \*.text | None                                                                                                                                                                                | None             | System.String                 |
| \*.dll         | [\*.zip configuration](https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration)                                         | [\*.vfp configuration](https://sites.google.com/view/mea-docs/main/useful-information/vfp-configuration) | System.Reflection.Assembly |


# Couple of code snippets

Example: Loading database with using a zip file [Newtonsoft.Json](https://www.newtonsoft.com/json) library

    MEAZipLoadingProcedure proc = Blayms.MEA.ModExtraAssets.CreateLoadingProcedure<MEAZipLoadingProcedure>(@"D:\mycoolpluginfolder\myCoolZip.zip", this,
    (string json, Type type, object[] extraData) =>
    {
    return  JsonConvert.DeserializeObject(json, type, (JsonSerializerSettings)extraData[0]);
    }, new  object[]{MyCoolClass.GetJsonSerializerSettingsFor<LocalizationData>()});
    proc.Initiate();

Example: Do stuff after all asset are finished to load from \*.zip

    // CREATE THE PROCEDURE
    MEAZipLoadingProcedure meaZipLoadingProcedure = ModExtraAssets.CreateLoadingProcedure<MEAZipLoadingProcedure>
    (Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    "zip.zip"), BasePlugin.Instance, (string json, Type type, object[] extraData)
    => JsonConvert.DeserializeObject(json, type, Class.MyJsonSerializerSettings(type)), null);

    meaZipLoadingProcedure.onLoadingResultDefined +=
    delegate (MEAZipLoadingProcedure.LoadingResult loadingResult) // SUB TO EVENT
    {
       if(loadingResult == MEAZipLoadingProcedure.LoadingResult.Success)
       {
            //CODE
       }
    };
    meaZipLoadingProcedure.Initiate(); // START LOADING ASSETS

# Credits

This tool was made by [Blayms](https://blayms.github.io/about-me/) and uses some utils found online, such as:

- [ICSharpCode.SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
- [deadlyfingers/UnityWav](https://github.com/deadlyfingers/UnityWav)
- [Dummiesman/ObjLoader](https://github.com/PhalanxHead/UnityRuntimeOBJLoaderDocs)
