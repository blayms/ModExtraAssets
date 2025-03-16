using Blayms.MEA.Utils;
using Blayms.MEA.Utils.ICSharpCode.SharpZipLib.Zip;
using Blayms.VersatileFilePackage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static Blayms.MEA.AssetEntryMEA;

namespace Blayms.MEA
{
    /// <summary>
    /// Class that allows to track the loading progress from Versatile File Packages
    /// </summary>
    public class MEAVfpLoadingProcedure : MEALoadingProcedureBase
    {

        public MEAVfpLoadingProcedure(string vfpPath, MonoBehaviour monoBehaviour) : base(vfpPath, monoBehaviour)
        {
        }
        public MEAVfpLoadingProcedure(byte[] vfpBytes, MonoBehaviour monoBehaviour) : base(vfpBytes, monoBehaviour)
        {
        }

        private void TryReadingPackageMetadata(FilePackage.PackageMetadata metadata)
        {
            try
            {
                if (metadata.AdditionalMetadata.GetAllValuesSafe("pluginDllPath", out string[] paths))
                {
                    string dllPath = paths[0].Replace("{VFP_DIR}", System.IO.Path.GetDirectoryName(Path));
                    Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                    ModExtraAssets.TryAddingReferenceAssembly(pluginAssembly);
                }
            }
            catch
            {
                throw new Exceptions.FailedVFPMetadataParseException(this);
            }
        }

        protected override IEnumerator LoadIEnumerator()
        {
            SetResult(LoadingResult.FilesInProgress);
            Task<FilePackage> filePackageTask = UsesFileBytes ? FilePackage.FromBytesAsync(Bytes) : FilePackage.FromFileAsync(Path);
            yield return new WaitUntil(() => filePackageTask.IsCompleted);

            if (filePackageTask.IsFaulted)
            {
                throw new Exception("Failed to load FilePackage...");
            }
            else
            {
                FilePackage filePackage = filePackageTask.Result;

                Dictionary<string, AssetEntryMEA.Directory> dirs = new Dictionary<string, AssetEntryMEA.Directory>();
                Dictionary<string, AssetEntryMEA.Folder> createdFolders = new Dictionary<string, AssetEntryMEA.Folder>();
                AssetEntryMEA.Directory coreDir = new AssetEntryMEA.Directory(filePackage.Metadata.Name);
                List<Action<AssetFile>> jsonObjectsCreation = new List<Action<AssetFile>>();
                List<Action<AssetFile>> jsonObjectsDirectoryFinishing = new List<Action<AssetFile>>();
                List<Action<AssetFile>> spriteSheetCreationActions = new List<Action<AssetFile>>();
                List<Action<AssetFile>> spriteSheetCreationActions_Metadata = new List<Action<AssetFile>>();
                List<AssetFile> jsonAssetFiles = new List<AssetFile>();
                Dictionary<string, AssetEntryMEA> spriteSheetGraphicTemp = new Dictionary<string, AssetEntryMEA>();
                List<AssetFile> spriteSheetEntriesTemp = new List<AssetFile>();
                List<AssetFile> spriteSheetEntriesTemp_Metadata = new List<AssetFile>();
                List<AssetEntryMEA> sheetObjectEntries = new List<AssetEntryMEA>();
                coreDir.folders = new Folder[] { new Folder(coreDir) };
                coreDir.isCoreDir = true;
                dirs.Add(coreDir.InPath, coreDir);

                TryReadingPackageMetadata(filePackage.Metadata);

                for (int i = 0; i < filePackage.Files.Length; i++)
                {
                    AssetFile assetFile = filePackage.Files[i];

                    if (assetFile.Metadata.GetAllValuesSafe("meaDir", out string[] paths))
                    {
                        string dir = paths[0];
                        if (!dirs.ContainsKey(dir))
                        {
                            AssetEntryMEA.Directory directory = new AssetEntryMEA.Directory(System.IO.Path.Combine(assetFile.Name + "." + assetFile.Extension, dir));
                            if (coreDir.InPath != directory.InPath)
                            {
                                directory.filePath = System.IO.Path.Combine(dir, assetFile.Name + "." + assetFile.Extension);
                                string[] folders = dir.Split(System.IO.Path.DirectorySeparatorChar);
                                List<AssetEntryMEA.Folder> foldersList = new List<AssetEntryMEA.Folder>();
                                foreach (string folderPath in folders)
                                {
                                    AssetEntryMEA.Directory newDir = new AssetEntryMEA.Directory(directory.InPath.Substring(0, directory.InPath.LastIndexOf(folderPath)) + folderPath);
                                    newDir.filePath = System.IO.Path.Combine(dir, assetFile.Name + "." + assetFile.Extension);
                                    if (!dirs.ContainsKey(newDir.InPath))
                                    {
                                        dirs.Add(newDir.InPath, newDir);
                                    }
                                    AssetEntryMEA.Folder folder = null;
                                    if (createdFolders.TryGetValue(newDir.InPath, out Folder folder1))
                                    {
                                        folder = folder1;
                                    }
                                    else
                                    {
                                        folder = new AssetEntryMEA.Folder(newDir, folderPath);
                                        createdFolders.Add(newDir.InPath, folder);
                                    }
                                    foldersList.Add(folder);
                                }
                                directory.folders = foldersList.ToArray();
                                dirs.AddOrReplace(dir, directory);
                            }
                        }
                    }
                    string fileExt = assetFile.Extension;
                    byte[] bytes = assetFile.Bytes;
                    Type type = Internal.GetTypeFromVFPAssetFile(assetFile);
                    AssetEntryMEA assetEntry = null;
                    switch (fileExt)
                    {
                        case "png":
                        case "jpeg":
                        case "jpg":
                            object obj = null;
                            switch (type.Name)
                            {
                                case nameof(Texture2D):
                                    obj = Internal.Texture2DFromAssetFile(assetFile);
                                    break;
                                case nameof(Sprite):
                                    obj = Internal.SpriteFromAssetFile(assetFile);
                                    break;
                                case nameof(Cubemap):
                                    obj = Internal.CubemapFromAssetFile(assetFile);
                                    break;
                            }
                            string name = assetFile.Name;
                            if (obj.GetType() == typeof(Sprite))
                            {
                                name = ((Sprite)obj).name.Replace("MEA_", "");
                            }
                            if (type == typeof(Cubemap))
                            {
                                name = ((Cubemap)obj).name.Replace("MEA_", "");
                            }
                            assetEntry = new AssetEntryMEA(bytes, typeof(Texture2D), obj, name);
                            ModExtraAssets.PopulateDictionary(this, type, assetEntry);
                            if(assetFile.Metadata.GetAllValuesSafe("meaTexType", out string[] values))
                            {
                                string value = values[0];

                                if(value == "sheet")
                                {
                                    spriteSheetGraphicTemp.Add(name, assetEntry);

                                    if(assetFile.Metadata.Contains("meaSheetJson"))
                                    {
                                        Action<AssetFile> spriteSheetCreationFromMetadata = (AssetFile actionAssetFile) =>
                                        {
                                            SpriteSheetMEA spriteSheet = ScriptableObject.CreateInstance<SpriteSheetMEA>();
                                            string jsonString = actionAssetFile.Metadata.GetAllValues("meaSheetJson")[0];
                                            spriteSheet.rawJson = jsonString;
                                            if (spriteSheetGraphicTemp.TryGetValue(Internal.AssetNameOf(actionAssetFile, false), out AssetEntryMEA asset))
                                            {
                                                spriteSheet.textureEntry = asset;
                                                spriteSheet.texture = asset.ValueAs<Texture2D>();
                                                spriteSheet.name = spriteSheet.texture.name;
                                            }
                                            else
                                            {
                                                spriteSheet.name = "MEA_FAILED_TO_LOAD_SPRITE_SHEET_ASSET";
                                                Debug.LogWarning($"ModExtraAssets failed to find a texture for sprite sheet ({Internal.AssetNameOf(actionAssetFile, false)})");
                                            }
                                            Type jsonConvert = ModExtraAssets.GetTypeFromRefDlls("Newtonsoft.Json.JsonConvert");
                                            if (jsonConvert != null)
                                            {
                                                spriteSheet.internalData = jsonConvert.GetMethodExtended("DeserializeObject",
                                                    new Type[] { typeof(string), typeof(Type) }, typeof(System.Object)).Invoke(null, new object[] { jsonString, typeof(Internal.SpriteSheetData) }) as Internal.SpriteSheetData;
                                                spriteSheet.Internal_BakeAll();
                                                AssetEntryMEA assetEntryOfSheet = new AssetEntryMEA(bytes, typeof(SpriteSheetMEA), spriteSheet,
                                                    System.IO.Path.GetFileNameWithoutExtension(actionAssetFile.Name).Replace("!sheetdata", ""));
                                                sheetObjectEntries.Add(assetEntryOfSheet);
                                                spriteSheet.assetEntry = assetEntryOfSheet;
                                                ModExtraAssets.PopulateDictionary(this, typeof(SpriteSheetMEA), assetEntryOfSheet);
                                            }
                                            else
                                            {
                                                throw new Exceptions.JsonNetMissingException("Sprite sheet creation requires a reference to Newtonsoft.Json", false);
                                            }
                                        };
                                        spriteSheetCreationActions_Metadata.Add(spriteSheetCreationFromMetadata);
                                        spriteSheetEntriesTemp_Metadata.Add(assetFile);
                                    }
                                }
                            }
                            break;
                        case "wav":
                            assetEntry = new AssetEntryMEA(bytes, typeof(AudioClip), Internal.AudioClipFromAssetFile(assetFile),
                            assetFile.Name);
                            ModExtraAssets.PopulateDictionary(this, type, assetEntry);
                            break;
                        case "obj":
                            string data = Encoding.Default.GetString(bytes);
                            assetEntry = new AssetEntryMEA(bytes, typeof(Mesh), Internal.MeshFromAssetFile(assetFile), assetFile.Name);
                            ModExtraAssets.PopulateDictionary(this, type, assetEntry);
                            break;
                        case "json":
                            if (!Internal.AssetNameOf(assetFile, false).StartsWith("sheetdata!"))
                            {
                                Action<AssetFile> action = (AssetFile actionAssetFile) =>
                                {
                                    Type jsonType = typeof(object);
                                    if (actionAssetFile.Metadata.GetAllValuesSafe("meaJsonType", out string[] possibleTypes))
                                    {
                                        jsonType = ModExtraAssets.GetTypeFromRefDlls(possibleTypes[0]);
                                    }
                                    else
                                    {
                                        throw new Exceptions.JsonDeserializationFailException(actionAssetFile, "Asset File Metadata is missing meaJsonType key, it defines the type of the serialized object and must be included!");
                                    }
                                    string jsonString = Encoding.Default.GetString(bytes);
                                    assetEntry = new AssetEntryMEA(bytes, jsonType, Internal.TryDeserializingJsonWithAssetFile(actionAssetFile, this, jsonString, jsonType, JsonDeserializationArgs), System.IO.Path.GetFileNameWithoutExtension(actionAssetFile.Name));
                                    ModExtraAssets.PopulateDictionary(this, jsonType, assetEntry);
                                };
                                jsonAssetFiles.Add(assetFile);
                                jsonObjectsCreation.Add(action);
                            }
                            else
                            {
                                Action<AssetFile> spriteSheetCreation = (AssetFile actionAssetFile) =>
                                {
                                    SpriteSheetMEA spriteSheet = ScriptableObject.CreateInstance<SpriteSheetMEA>();
                                    string jsonString = Encoding.Default.GetString(bytes);
                                    spriteSheet.rawJson = jsonString;
                                    if (spriteSheetGraphicTemp.TryGetValue(Internal.AssetNameOf(actionAssetFile, false).Replace("sheetdata!", ""), out AssetEntryMEA asset))
                                    {
                                        spriteSheet.textureEntry = asset;
                                        spriteSheet.texture = asset.ValueAs<Texture2D>();
                                        spriteSheet.name = spriteSheet.texture.name.Replace("sheetdata!", "");
                                    }
                                    else
                                    {
                                        spriteSheet.name = "MEA_FAILED_TO_LOAD_SPRITE_SHEET_ASSET";
                                        Debug.LogWarning($"ModExtraAssets failed to find a texture for sprite sheet ({Internal.AssetNameOf(actionAssetFile, false)})");
                                    }
                                    Type jsonConvert = ModExtraAssets.GetTypeFromRefDlls("Newtonsoft.Json.JsonConvert");
                                    if (jsonConvert != null)
                                    {
                                        spriteSheet.internalData = jsonConvert.GetMethodExtended("DeserializeObject",
                                            new Type[] { typeof(string), typeof(Type) }, typeof(System.Object)).Invoke(null, new object[] { jsonString, typeof(Internal.SpriteSheetData) }) as Internal.SpriteSheetData;
                                        spriteSheet.Internal_BakeAll();
                                        AssetEntryMEA assetEntryOfSheet = new AssetEntryMEA(bytes, typeof(SpriteSheetMEA), spriteSheet,
                                            actionAssetFile.Name.Replace("sheetdata!", ""));
                                        sheetObjectEntries.Add(assetEntryOfSheet);
                                        spriteSheet.assetEntry = assetEntryOfSheet;
                                        ModExtraAssets.PopulateDictionary(this, typeof(SpriteSheetMEA), assetEntryOfSheet);
                                    }
                                    else
                                    {
                                        throw new Exceptions.JsonNetMissingException("Sprite sheet creation requires a reference to Newtonsoft.Json", false);
                                    }
                                };
                                spriteSheetEntriesTemp.Add(assetFile);
                                spriteSheetCreationActions.Add(spriteSheetCreation);
                            }
                            break;
                        case "txt":
                        case "text":
                            string text = Encoding.Default.GetString(bytes);
                            assetEntry = new AssetEntryMEA(bytes, typeof(string), text, assetFile.Name);
                            ModExtraAssets.PopulateDictionary(this, typeof(string), assetEntry);
                            break;
                        default:
                            assetEntry = new AssetEntryMEA(bytes, typeof(byte[]), bytes, assetFile.Name);
                            ModExtraAssets.PopulateDictionary(this, typeof(byte[]), assetEntry);
                            break;
                    }
                    Action<AssetFile> dirFinishingAction = (AssetFile actionAssetFile) =>
                    {
                        if (assetFile.Metadata.GetAllValuesSafe("meaDir", out string[] meaDirs))
                        {
                            string entryDir = meaDirs[0];
                            if (dirs.ContainsKey(entryDir))
                            {
                                string lastFolder = entryDir.Split(System.IO.Path.DirectorySeparatorChar).Last();
                                int indexOfFolderInDir = Array.IndexOf(dirs[entryDir].folders, dirs[entryDir].folders.Where(x => x.Name == lastFolder).FirstOrDefault());

                                if (!dirs[entryDir].folders[indexOfFolderInDir].assets_list.Contains(assetEntry))
                                {
                                    assetEntry.directory = dirs[entryDir];
                                    dirs[entryDir].folders[indexOfFolderInDir].assets_list.Add(assetEntry);
                                }
                            }
                            if (assetEntry.directory == null)
                            {
                                if (!coreDir.folders[0].assets_list.Contains(assetEntry))
                                {
                                    coreDir.folders[0].assets_list.Add(assetEntry);
                                    assetEntry.directory = coreDir;
                                }
                            }
                        }
                    };

                    if (fileExt == "json")
                    {
                        jsonObjectsDirectoryFinishing.Add(dirFinishingAction);
                    }
                    else
                    {
                        dirFinishingAction?.Invoke(assetFile);
                    }

                    yield return new WaitForEndOfFrame();
                    Invoke_onEntryLoaded(assetEntry);
                }

                SetResult(LoadingResult.JsonDeserialization);
                Tuple<Action<AssetFile>, Action<AssetFile>, AssetFile>[] tuple = new Tuple<Action<AssetFile>, Action<AssetFile>, AssetFile>[jsonObjectsCreation.Count];
                for (int i = 0; i < jsonObjectsCreation.Count; i++)
                {
                    tuple[i] = new Tuple<Action<AssetFile>, Action<AssetFile>, AssetFile>(jsonObjectsCreation[i], jsonObjectsDirectoryFinishing[i], jsonAssetFiles[i]);
                }
                tuple = tuple.OrderBy(x => $"{x.Item3.Name}.{x.Item3.Extension}").ToArray();
                for (int i = 0; i < tuple.Length; i++)
                {
                    tuple[i].Item1?.Invoke(tuple[i].Item3);
                    tuple[i].Item2?.Invoke(tuple[i].Item3);
                }
                for (int i = 0; i < spriteSheetEntriesTemp.Count; i++)
                {
                    spriteSheetCreationActions[i].Invoke(spriteSheetEntriesTemp[i]);
                }
                for (int i = 0; i < spriteSheetEntriesTemp_Metadata.Count; i++)
                {
                    spriteSheetCreationActions_Metadata[i].Invoke(spriteSheetEntriesTemp_Metadata[i]);
                }
                Dictionary<string, AssetEntryMEA>.Enumerator enumerator = spriteSheetGraphicTemp.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    AssetEntryMEA sheetTextureEntry = enumerator.Current.Value;
                    AssetEntryMEA sheet = sheetObjectEntries.Where(x => x.Name == sheetTextureEntry.Name.Replace("!sheet", "")).FirstOrDefault();
                    if (sheetTextureEntry.directory != null)
                    {
                        sheetTextureEntry.directory.folders.Last().assets_list.Add(sheet);
                        sheet.directory = sheetTextureEntry.directory;
                    }
                    yield return new WaitForEndOfFrame();
                }
                SetResult(LoadingResult.DirsInProgress);
                AssetEntryMEA.Directory[] allDirsCooked = dirs.Values.ToArray();
                Folder[] cookedFolders = createdFolders.Values.ToArray();
                List<Folder> coreFolders = new List<Folder>
            {
                coreDir.folders[0]
            };
                for (int i = 0; i < allDirsCooked.Length; i++)
                {
                    for (int j = 0; j < cookedFolders.Length; j++)
                    {
                        string[] splits = allDirsCooked[i].InPath.Split(System.IO.Path.DirectorySeparatorChar);
                        if (splits.Contains(cookedFolders[j].Name))
                        {
                            if (!allDirsCooked[i].folders.Contains(cookedFolders[j]))
                            {
                                allDirsCooked[i].folders = allDirsCooked[i].folders.Concat(new Folder[] { cookedFolders[j] }).ToArray();
                            }
                        }
                    }
                }
                for (int i = 0; i < allDirsCooked.Length; i++)
                {
                    for (int j = 0; j < allDirsCooked[i].folders.Length; j++)
                    {
                        allDirsCooked[i].folders[j].assets_array = allDirsCooked[i].folders[j].assets_list.ToArray();
                        if (allDirsCooked[i].InPath.Split(System.IO.Path.DirectorySeparatorChar).Length == 2)
                        {
                            coreFolders.Add(allDirsCooked[i].folders[j]);
                        }
                    }
                }
                coreDir.folders = coreFolders.ToArray();

                SetResult(LoadingResult.Success);
            }
        }

    }
}
