using Blayms.MEA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Blayms.MEA
{
    /// <summary>
    /// Class that contains plenty of information about the loaded asset
    /// </summary>
    public class AssetEntryMEA
    {
        private byte[] bytes;
        private Type assetType;
        private object value;
        private string name;
        internal Directory directory;
        internal AssetEntryMEA(byte[] bytes, Type assetType, object value, string name)
        {
            this.bytes = bytes;
            this.assetType = assetType;
            this.value = value;
            this.name = name;
        }
        /// <summary>
        /// Raw bytes of the entry file
        /// </summary>
        public byte[] Bytes => bytes;
        /// <summary>
        /// The type of this entry asset
        /// </summary>
        public Type AssetType => assetType;
        /// <summary>
        /// Value of the entry as object, use TryValueAs<T>() or ValueAs<T>() to get the actual value
        /// </summary>
        public object Value => value;
        /// <summary>
        /// Name of the entry file
        /// </summary>
        public string Name => name;
        /// <summary>
        /// Converts byte array to string, in case if the file is not a in .txt/.text format.<para>If this asset entry is in .txt/.text format, please convert it's value to string</para>
        /// </summary>
        public string BytesToString => Encoding.Default.GetString(bytes);

        /// <summary>
        /// Directory of this entry
        /// </summary>
        public Directory EntryDirectory => directory;
        /// <summary>
        /// Try casting object value to any type
        /// </summary>
        /// <returns></returns>
        public bool TryValueAs<T>(out T value)
        {
            T obj = (T)this.value;
            if(obj == null)
            {
                value = default;
                return false;
            }
            value = obj;
            return true;
        }
        /// <summary>
        /// Cast an object value to any type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ValueAs<T>()
        {
            return (T)value;
        }
        public override string ToString()
        {
            return $"AssetEntryMEA of type ({assetType.Name}) ({Name})";
        }
        /// <summary>
        /// File.zip directory class
        /// </summary>
        public class Directory
        {
            /// <summary>
            /// Last folder in the directory
            /// </summary>
            public Folder LastFolder => folders.Last();
            /// <summary>
            /// All directory folders
            /// </summary>
            public Folder[] Folders => folders;
            /// <summary>
            /// Path of the directory in-zip
            /// </summary>
            public string InZipPath => inZipPath;
            /// <summary>
            /// Path of the .zip file on disk
            /// </summary>
            public string ZipFilePath => zipPath;
            /// <summary>
            /// Is a root directory
            /// </summary>
            public bool IsRoot => isCoreDir;
            internal Folder[] folders = new Folder[0];
            internal bool isCoreDir;
            private string inZipPath;
            internal string zipPath;
            private static Dictionary<string, Directory> allDirsStatic = new Dictionary<string, Directory>();
            /// <summary>
            /// GetFolderByName(name);
            /// </summary>
            /// <returns></returns>
            public Folder this[string name] => GetFolderByName(name);
            /// <summary>
            /// Every time you load a .zip, all directories are getting saved for later usage
            /// </summary>
            /// <param name="path">Path of a directory</param>
            /// <returns></returns>
            public static Directory Search(string path)
            {
                try
                {
                    return allDirsStatic[path];
                }
                catch(Exception ex)
                {
                    throw new Exceptions.DirectoryNotFoundException(path, ex);
                }
            }
            /// <summary>
            /// Get all files in the directory
            /// </summary>
            /// <param name="countFolders">Decided to count folders or not</param>
            /// <param name="excludeFolders">Folders to exclude by search</param>
            /// <returns></returns>
            public int GetFileCount(bool countFolders, params string[] excludeFolders)
            {
                List<Folder> foldersFilter = new List<Folder>();
                if (excludeFolders != null)
                {
                    foldersFilter = folders.Where(x => excludeFolders.Contains(x.Name)).ToList();
                }
                int count = 0;
                for (int i = 0; i < folders.Length; i++)
                {
                    if (!foldersFilter.Contains(folders[i]))
                    {
                        count += folders[i].FileCount + (countFolders ? 1:0);
                    }
                }
                return count;
            }
            internal Directory(string inZipPath)
            {
                this.inZipPath = inZipPath;
                if (!allDirsStatic.ContainsKey(inZipPath))
                {
                    allDirsStatic.Add(inZipPath, this);
                }
            }
            ~Directory()
            {
                if (allDirsStatic.ContainsKey(inZipPath))
                {
                    allDirsStatic.Remove(inZipPath);
                }
            }
            /// <summary>
            /// Searches for a folder in directory
            /// </summary>
            /// <param name="name"></param>
            /// <exception cref="NullReferenceException">If the folder does not exist in this directory</exception>
            /// <returns></returns>
            public Folder GetFolderByName(string name)
            {
                return folders.Where(x => x.Name == name).FirstOrDefault();
            }
            /// <summary>
            /// Try searching for a folder in directory
            /// </summary>
            /// <param name="name"></param>
            /// <param name="folder"></param>
            /// <returns></returns>
            public bool TryGetFolderByName(string name, out Folder folder)
            {
                folder = GetFolderByName(name);

                return folder != null;
            }
            public override string ToString()
            {
                return $"Directory ({inZipPath})";
            }
        }
        /// <summary>
        /// File.zip folder class
        /// </summary>
        public class Folder
        {
            /// <summary>
            /// Folder name
            /// </summary>
            public string Name => name;
            /// <summary>
            /// Folder entries
            /// </summary>
            public AssetEntryMEA[] Assets => assets_array;
            /// <summary>
            /// Directory of this folder
            /// </summary>
            public Directory Directory => directory;
            /// <summary>
            /// Count of assets inside the folder
            /// </summary>
            public int FileCount => assets_array.Length;

            private string name;
            private Directory directory;
            internal AssetEntryMEA[] assets_array;
            internal List<AssetEntryMEA> assets_list = new List<AssetEntryMEA>();

            internal Folder(Directory dir, string folderName = "/")
            {
                directory = dir;
                name = folderName;
            }
            /// <summary>
            /// Get AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public AssetEntryMEA GetEntryWhere<T>(Func<AssetEntryMEA, bool> predicate)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                return assetEntries.Where(predicate).FirstOrDefault();
            }
            /// <summary>
            /// Get all entries under some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public AssetEntryMEA[] GetEntriesWhere<T>(Func<AssetEntryMEA, bool> predicate)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                return assetEntries.Where(predicate).ToArray();
            }
            /// <summary>
            /// Get values of AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public T[] GetEntryValuesWhere<T>(Func<AssetEntryMEA, bool> predicate)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                return assetEntries.Where(predicate).ToArray().EntryCollectionValues<T>();
            }
            /// <summary>
            /// Try getting all entries under some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public bool TryGetEntriesWhere<T>(Func<AssetEntryMEA, bool> predicate, out AssetEntryMEA[] entries)
            {
                entries = GetEntriesWhere<T>(predicate);

                return entries != null;
            }
            /// <summary>
            /// Try getting values of AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public bool TryGetEntryValuesWhere<T>(Func<AssetEntryMEA, bool> predicate, out T[] values)
            {
                values = GetEntryValuesWhere<T>(predicate);

                return values != null;
            }
            /// <summary>
            /// Get value of AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public T GetEntryValueWhere<T>(Func<AssetEntryMEA, bool> predicate)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                return assetEntries.Where(predicate).FirstOrDefault().ValueAs<T>();
            }
            /// <summary>
            /// Try getting AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public bool TryGetEntryWhere<T>(Func<AssetEntryMEA, bool> predicate, out AssetEntryMEA entry)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                entry = assetEntries.Where(predicate).FirstOrDefault();

                return entry != null;
            }
            /// <summary>
            /// Try getting a value of AssetEntryMEA with using some condition/predicate to filter the search
            /// </summary>
            /// <returns></returns>
            public bool TryGetEntryValueWhere<T>(Func<AssetEntryMEA, bool> predicate, out T value)
            {
                AssetEntryMEA[] assetEntries = assets_array.Where(x => x.AssetType == typeof(T)).ToArray();

                value = assetEntries.Where(predicate).FirstOrDefault().ValueAs<T>();
                return value != null;
            }
            public override string ToString()
            {
                return $"Folder ({name})";
            }
        }
    }
}
