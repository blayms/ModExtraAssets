using Blayms.MEA.Utils;
using Blayms.MEA.Utils.ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static Blayms.MEA.AssetEntryMEA;

namespace Blayms.MEA
{
    /// <summary>
    /// Class that allows to track the loading progress
    /// </summary>
    public class MEAZipLoadingProcedure
    {
        private LoadingResult result;
        private string zipPath = null;
        private byte[] zipBytes = null;
        private bool usesZipBytes;
        private MonoBehaviour monoBehaviour;
        public delegate void LoadingResultDelegate(LoadingResult result);
        public delegate void OnEntryLoaded(AssetEntryMEA entry);
        /// <summary>
        /// Invokes when "Result" of this MEAZipLoadingProcedure changes
        /// </summary>
        public event LoadingResultDelegate onLoadingResultDefined;
        /// <summary>
        /// Invokes when an entry finishes to load from *.zip
        /// </summary>
        public event OnEntryLoaded onEntryLoaded;
        /// <summary>
        /// If you're using some different JSON library, rather than unity's JsonUtility, you can link your link your library with ModExtraAssets by modifing this field<para>It uses JsonUtility.FromJson(json, type); by default</para>
        /// </summary>
        public Func<string, Type, object[], object> JsonDeserializeFunction = (string json, Type type, object[] args) =>
        {
            return JsonUtility.FromJson(json, type);
        };
        public object[] JsonDeserializationArgs = null;

        public MEAZipLoadingProcedure(string zipPath, MonoBehaviour monoBehaviour)
        {
            this.zipPath = zipPath;
            this.monoBehaviour = monoBehaviour;
        }
        public MEAZipLoadingProcedure(byte[] zipBytes, MonoBehaviour monoBehaviour)
        {
            this.zipBytes = zipBytes;
            this.monoBehaviour = monoBehaviour;
            usesZipBytes = true;
        }
        /// <summary>
        /// Shows the zip loading procedure status
        /// </summary>
        public enum LoadingResult
        {
            /// <summary>
            /// Not yet initialized
            /// </summary>
            None,
            /// <summary>
            /// Currently loading
            /// </summary>
            FilesInProgress,
            /// <summary>
            /// Loading failed
            /// </summary>
            Failure,
            /// <summary>
            /// Searches each directory
            /// </summary>
            DirsInProgress,
            /// <summary>
            /// Deserializes all *.json files from *.zip
            /// </summary>
            JsonDeserialization,
            /// <summary>
            /// Loading succeeded
            /// </summary>
            Success
        }
        /// <summary>
        /// Current status of the loading process
        /// </summary>
        public LoadingResult Result
        {
            get
            {
                return result;
            }
        }
        /// <summary>
        /// Path of the .zip file
        /// </summary>
        public string Path
        {
            get
            {
                if (usesZipBytes)
                {
                    throw new DataMisalignedException("Current MEAZipLoadingProcedure does not use path for populating the database. Use \"Bytes\" property instead.");
                }
                return zipPath;
            }
        }
        /// <summary>
        /// Bytes of the .zip file
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                if (!usesZipBytes)
                {
                    throw new DataMisalignedException("Current MEAZipLoadingProcedure does not use bytes for populating the database. Use \"Path\" property instead.");
                }
                return zipBytes;
            }
        }
        /// <summary>
        /// Manually boots up the loading procedure
        /// </summary>
        public void Initiate()
        {
            if (result == LoadingResult.Success)
            {
                return;
            }
            monoBehaviour.StartCoroutine(LoadIEnumerator());
        }
        private IEnumerator LoadIEnumerator()
        {
            SetResult(LoadingResult.FilesInProgress);

            ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(zipPath));
            Stream stream = null;
            if (usesZipBytes)
            {
                stream = new MemoryStream(zipBytes);
            }
            else
            {
                stream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
            }
            ZipFile zipFile = new ZipFile(stream);
            ZipEntry zipEntry;
            Dictionary<string, AssetEntryMEA.Directory> dirs = new Dictionary<string, AssetEntryMEA.Directory>();
            Dictionary<string, AssetEntryMEA.Folder> createdFolders = new Dictionary<string, AssetEntryMEA.Folder>();
            AssetEntryMEA.Directory coreDir = new AssetEntryMEA.Directory(System.IO.Path.Combine(System.IO.Path.GetFileName(zipFile.Name)));
            List<Action<ZipEntry>> jsonObjectsCreation = new List<Action<ZipEntry>>();
            List<Action<ZipEntry>> jsonObjectsDirectoryFinishing = new List<Action<ZipEntry>>();
            List<ZipEntry> jsonZipEntries = new List<ZipEntry>();
            coreDir.folders = new Folder[] { new Folder(coreDir) };
            coreDir.isCoreDir = true;
            dirs.Add(coreDir.InZipPath, coreDir);
            if (zipFile.ZipFileComment != string.Empty)
            {
                ReadZipComment(zipFile.ZipFileComment);
            }
            while ((zipEntry = zipInputStream.GetNextEntry()) != null)
            {
                Stream zipStream = zipFile.GetInputStream(zipEntry);
                using (StreamReader s = new StreamReader(zipStream))
                {
                    if (zipEntry.IsDirectory) { continue; }

                    string dir = System.IO.Path.GetDirectoryName(zipEntry.Name);
                    if (!dirs.ContainsKey(dir))
                    {
                        AssetEntryMEA.Directory directory = new AssetEntryMEA.Directory(System.IO.Path.Combine(System.IO.Path.GetFileName(zipFile.Name), dir));
                        if (coreDir.InZipPath != directory.InZipPath)
                        {
                            directory.zipPath = zipFile.Name;
                            string[] folders = dir.Split(System.IO.Path.DirectorySeparatorChar);
                            List<AssetEntryMEA.Folder> foldersList = new List<AssetEntryMEA.Folder>();
                            foreach (string folderPath in folders)
                            {
                                AssetEntryMEA.Directory newDir = new AssetEntryMEA.Directory(directory.InZipPath.Substring(0, directory.InZipPath.LastIndexOf(folderPath)) + folderPath);
                                newDir.zipPath = zipFile.Name;
                                if (!dirs.ContainsKey(newDir.InZipPath))
                                {
                                    dirs.Add(newDir.InZipPath, newDir);
                                }
                                AssetEntryMEA.Folder folder = null;
                                if (createdFolders.TryGetValue(newDir.InZipPath, out Folder folder1))
                                {
                                    folder = folder1;
                                }
                                else
                                {
                                    folder = new AssetEntryMEA.Folder(newDir, folderPath);
                                    createdFolders.Add(newDir.InZipPath, folder);
                                }
                                foldersList.Add(folder);
                            }
                            directory.folders = foldersList.ToArray();
                            dirs.Add(dir, directory);
                        }
                    }

                    string fileExt = System.IO.Path.GetExtension(zipEntry.Name);
                    byte[] bytes = zipEntry.ReadBytesFromZipStream(zipStream);
                    Type type = Internal.GetTypeFromFileEnd(zipEntry.Name);
                    AssetEntryMEA assetEntry = null;
                    switch (fileExt)
                    {
                        case ".png":
                            object obj = null;
                            switch (type.Name)
                            {
                                case nameof(Texture2D):
                                    obj = Internal.Texture2DFromZip(zipEntry, bytes);
                                    break;
                                case nameof(Sprite):
                                    obj = Internal.SpriteFromZip(zipEntry, bytes);
                                    break;
                                case nameof(Cubemap):
                                    obj = Internal.CubemapFromZip(zipEntry, bytes);
                                    break;
                            }
                            string name = System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name);
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
                            break;
                        case ".wav":
                            assetEntry = new AssetEntryMEA(bytes, typeof(AudioClip), Internal.AudioClipFromZip(zipEntry, bytes),
                            System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name));
                            ModExtraAssets.PopulateDictionary(this, type, assetEntry);
                            break;
                        case ".obj":
                            string data = Encoding.Default.GetString(bytes);
                            assetEntry = new AssetEntryMEA(bytes, typeof(Mesh), Internal.MeshFromZip(zipEntry, data), System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name));
                            ModExtraAssets.PopulateDictionary(this, type, assetEntry);
                            break;
                        case ".json":
                            Action<ZipEntry> action = (ZipEntry actionZipEntry) =>
                            {
                                Type jsonType = ModExtraAssets.GetTypeFromRefDlls(Internal.GetFileSubDirectory(actionZipEntry.Name));
                                string jsonString = Encoding.Default.GetString(bytes);
                                assetEntry = new AssetEntryMEA(bytes, jsonType, Internal.TryDeserializingJson(actionZipEntry.Name, this, jsonString, jsonType, JsonDeserializationArgs), System.IO.Path.GetFileNameWithoutExtension(actionZipEntry.Name));
                                ModExtraAssets.PopulateDictionary(this, jsonType, assetEntry);
                            };
                            jsonZipEntries.Add(zipEntry);
                            jsonObjectsCreation.Add(action);
                            break;
                        case ".txt":
                        case ".text":
                            string text = Encoding.Default.GetString(bytes);
                            assetEntry = new AssetEntryMEA(bytes, typeof(string), text, System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name));
                            ModExtraAssets.PopulateDictionary(this, typeof(string), assetEntry);
                            break;
                        default:
                            assetEntry = new AssetEntryMEA(bytes, typeof(byte[]), bytes, System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name));
                            ModExtraAssets.PopulateDictionary(this, typeof(byte[]), assetEntry);
                            break;
                    }
                    Action<ZipEntry> dirFinishingAction = (ZipEntry actionZipEntry) =>
                    {
                        string entryDir = System.IO.Path.GetDirectoryName(actionZipEntry.Name);
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
                    };

                    if(fileExt == ".json")
                    {
                        jsonObjectsDirectoryFinishing.Add(dirFinishingAction);
                    }
                    else
                    {
                        dirFinishingAction?.Invoke(zipEntry);
                    }

                    yield return new WaitForEndOfFrame();
                    onEntryLoaded?.Invoke(assetEntry);
                }
            }

            SetResult(LoadingResult.JsonDeserialization);
            Tuple<Action<ZipEntry>, Action<ZipEntry>, ZipEntry>[] tuple = new Tuple<Action<ZipEntry>, Action<ZipEntry>, ZipEntry>[jsonObjectsCreation.Count];
            for (int i = 0; i < jsonObjectsCreation.Count; i++)
            {
                tuple[i] = new Tuple<Action<ZipEntry>, Action<ZipEntry>, ZipEntry>(jsonObjectsCreation[i], jsonObjectsDirectoryFinishing[i], jsonZipEntries[i]);
            }
            tuple = tuple.OrderBy(x => System.IO.Path.GetFileNameWithoutExtension(x.Item3.Name)).ToArray();
            //for (int i = 0; i < jsonObjectsCreation.Count; i++)
            //{
            //    jsonObjectsCreation[i]?.Invoke(jsonZipEntries[i]);
            //    jsonObjectsDirectoryFinishing[i]?.Invoke(jsonZipEntries[i]);
            //}
            for (int i = 0; i < tuple.Length; i++)
            {
                tuple[i].Item1?.Invoke(tuple[i].Item3);
                tuple[i].Item2?.Invoke(tuple[i].Item3);
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
                    string[] splits = allDirsCooked[i].InZipPath.Split(System.IO.Path.DirectorySeparatorChar);
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
                    if (allDirsCooked[i].InZipPath.Split(System.IO.Path.DirectorySeparatorChar).Length == 2)
                    {
                        coreFolders.Add(allDirsCooked[i].folders[j]);
                    }
                }
            }
            coreDir.folders = coreFolders.ToArray();

            SetResult(LoadingResult.Success);
        }
        internal void SetResult(LoadingResult result)
        {
            this.result = result;
            onLoadingResultDefined?.Invoke(result);
        }
        private void ReadZipComment(string comment)
        {
            string[] lines = comment.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    if (!lines[i].StartsWith("~"))
                    {
                        string[] splits = lines[i].Split('=');
                        switch (splits[0])
                        {
                            case "pluginDllPath":
                                string path = splits[1].Replace("\"", "").Replace("{ZIP_DIR}", System.IO.Path.GetDirectoryName(zipPath));
                                Assembly pluginAssembly = Assembly.LoadFrom(path);
                                if (!ModExtraAssets.referenceAssemblies.Contains(pluginAssembly))
                                {
                                    ModExtraAssets.referenceAssemblies.Add(pluginAssembly);
                                    AssemblyName[] refAssemblies = pluginAssembly.GetReferencedAssemblies();
                                    for (int j = 0; j < refAssemblies.Length; j++)
                                    {
                                        Assembly assembly = Assembly.Load(refAssemblies[j]);

                                        if (!ModExtraAssets.referenceAssemblies.Contains(assembly))
                                        {
                                            ModExtraAssets.referenceAssemblies.Add(assembly);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                catch
                {
                    throw new Exceptions.FailedZipCommentParseException(this);
                }
            }
        }
    }
}
