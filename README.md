# ModExtraAssets
A tool made specificly to make a proccess of loading custom assets way easier
Highly recommended to be used for Unity Engine BepInEx plugins

 - Requires *.zip files (they act as a main file storage)

# Documentation

This tool has a documentation outside GitHub, check it [here](https://sites.google.com/view/mea-docs/main)



# Supported file types

ModExtraAssets can recognize some file extension to create a specific asset of type

|        Extension        |Condition                          |Return type                         |
|----------------|-------------------------------|-----------------------------|
|*.png           |None                           |UnityEngine.Texture2D                    |
|*.png           |File name must contain !number at the end to define Sprite.pixelsPerUnit Value                           |UnityEngine.Sprite                    |
|*.png           |File name must contain !c at the end to differentiate it from other *.png files                        |UnityEngine.Cubemap                    |
|*.wav          |None                            |UnityEngine.AudioClip           |
|*.obj          |None|UnityEngine.Mesh|
|*.json          |[Referencing DLLs](https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial)|Any *deserializable*|
|*.txt or *.text          |None|System.String|
|*.dll          |[*.zip configuration](https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration)                            |System.Reflection.Assembly           |

# Couple of code snippets
Example: Loading database with using [Newtonsoft.Json](https://www.newtonsoft.com/json) library

    Blayms.MEA.ModExtraAssets.LoadAllZipAssets(@"D:\mycoolpluginfolder\myCoolZip.zip", this,
    (string json, Type type, object[] extraData) =>
    {
    return  JsonConvert.DeserializeObject(json, type, (JsonSerializerSettings)extraData[0]);
    }, new  object[]{MyCoolClass.GetJsonSerializerSettingsFor<LocalizationData>()});
Example: Do stuff after all asset are finished to load from *.zip

    // CREATE THE PROCEDURE
    MEAZipLoadingProcedure meazipLoadingProcedure = ModExtraAssets.CreateLoadingProcedure
    (Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    "zip.zip"), BasePlugin.Instance, (string json, Type type, object[] extraData) 
    => JsonConvert.DeserializeObject(json, type, Class.MyJsonSerializerSettings(type)), null);
    
    meazipLoadingProcedure.onLoadingResultDefined += 
    delegate (MEAZipLoadingProcedure.LoadingResult loadingResult) // SUB TO EVENT
    {
       if(loadingResult == MEAZipLoadingProcedure.LoadingResult.Success)
       {
            //CODE
       }
    };
    meazipLoadingProcedure.Initiate(); // START LOADING ASSETS

# Credits
This tool was made by [Blayms](https://blayms.github.io/about-me/) and uses some utils found online, such as:

-   [ICSharpCode.SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
-   [deadlyfingers/UnityWav](https://github.com/deadlyfingers/UnityWav)
-   [Dummiesman/ObjLoader](https://github.com/PhalanxHead/UnityRuntimeOBJLoaderDocs)
