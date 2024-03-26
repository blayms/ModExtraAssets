using Blayms.MEA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Blayms.MEA
{
    /// <summary>
    /// Tool made by <see href="https://blayms.github.io/about-me/">Blayms</see> for loading any type of assets into your Unity game BepInEx mod<para>Read docs <see href="https://sites.google.com/view/mea-docs/main">here</see></para>
    /// </summary>
    public static class ModExtraAssets
    {
#region Internal
        internal static Dictionary<Type, List<AssetEntryMEA>> internalDatabase = new Dictionary<Type, List<AssetEntryMEA>>();
        internal const string ToolAcronym = "MEA";
        internal static List<Assembly> referenceAssemblies = new List<Assembly>();

        internal static void PopulateDictionary(MEAZipLoadingProcedure procedure, Type key, AssetEntryMEA assetEntry)
        {
            if (!internalDatabase.ContainsKey(key))
            {
                internalDatabase[key] = new List<AssetEntryMEA>();
            }
            if (internalDatabase[key].Contains(assetEntry))
            {
                procedure.SetResult(MEAZipLoadingProcedure.LoadingResult.Failure);
                throw new Exceptions.EntryDuplicateNotAllowedException(assetEntry);
            }
            internalDatabase[key].Add(assetEntry);
        }
        #endregion
        /// <summary>
        /// Function that adds content of a *.zip file into ModExtraAssets database
        /// </summary>
        /// <param name="zipPath">Path of the .zip file that contains all assets</param>
        /// <param name="monoBehaviour">Instance of UnityEngine.MonoBehaviour that allows you to run IEnumerators (required for loading assets)</param>
        /// <param name="customJsonFunction">A custom function for JSON deserialization, in case you're using different JSON library</param>
        /// <param name="customJsonDeserializationArgs">Some extra arguments for custom JSON deserialization function, if needed</param>
        /// <returns>Class that allows to track the loading progress</returns>
        public static MEAZipLoadingProcedure LoadAllZipAssets(string zipPath, MonoBehaviour monoBehaviour, Func<string, Type, object[], object> customJsonFunction = default, object[] customJsonDeserializationArgs = null)
        {
            MEAZipLoadingProcedure zipLoadingProcedure = new MEAZipLoadingProcedure(zipPath, monoBehaviour);
            if(customJsonFunction != default)
            {
                zipLoadingProcedure.JsonDeserializeFunction = customJsonFunction;
            }
            if(customJsonDeserializationArgs != null)
            {
                zipLoadingProcedure.JsonDeserializationArgs = customJsonDeserializationArgs;
            }
            zipLoadingProcedure.Initiate();

            return zipLoadingProcedure;
        }

        /// <summary>
        /// Creates MEAZipLoadingProcedure without initiating it up instantly. Use MEAZipLoadingProcedure.Initiate()
        /// </summary>
        /// <param name="zipPath">Path of the .zip file that contains all assets</param>
        /// <param name="monoBehaviour">Instance of UnityEngine.MonoBehaviour that allows you to run IEnumerators (required for loading assets)</param>
        /// <param name="customJsonFunction">A custom function for JSON deserialization, in case you're using different JSON library</param>
        /// <param name="customJsonDeserializationArgs">Some extra arguments for custom JSON deserialization function, if needed</param>
        /// <returns></returns>
        public static MEAZipLoadingProcedure CreateLoadingProcedure(string zipPath, MonoBehaviour monoBehaviour, Func<string, Type, object[], object> customJsonFunction = default, object[] customJsonDeserializationArgs = null)
        {
            MEAZipLoadingProcedure zipLoadingProcedure = new MEAZipLoadingProcedure(zipPath, monoBehaviour);
            if (customJsonFunction != default)
            {
                zipLoadingProcedure.JsonDeserializeFunction = customJsonFunction;
            }
            if (customJsonDeserializationArgs != null)
            {
                zipLoadingProcedure.JsonDeserializationArgs = customJsonDeserializationArgs;
            }
            return zipLoadingProcedure;
        }
        /// <summary>
        /// Function that adds content of a *.zip file into ModExtraAssets database
        /// </summary>
        /// <param name="zipBytes">Bytes of the .zip file that contains all assets</param>
        /// <param name="monoBehaviour">Instance of UnityEngine.MonoBehaviour that allows you to run IEnumerators (required for loading assets)</param>
        /// <param name="customJsonFunction">A custom function for JSON deserialization, in case you're using different JSON library</param>
        /// <param name="customJsonDeserializationArgs">Some extra arguments for custom JSON deserialization function, if needed</param>
        /// <returns>Class that allows to track the loading progress</returns>
        public static MEAZipLoadingProcedure LoadAllZipAssets(byte[] zipBytes, MonoBehaviour monoBehaviour, Func<string, Type, object[], object> customJsonFunction = default, object[] customJsonDeserializationArgs = null)
        {
            MEAZipLoadingProcedure zipLoadingProcedure = new MEAZipLoadingProcedure(zipBytes, monoBehaviour);
            if (customJsonFunction != default)
            {
                zipLoadingProcedure.JsonDeserializeFunction = customJsonFunction;
            }
            if (customJsonDeserializationArgs != null)
            {
                zipLoadingProcedure.JsonDeserializationArgs = customJsonDeserializationArgs;
            }
            zipLoadingProcedure.Initiate();

            return zipLoadingProcedure;
        }

        /// <summary>
        /// Creates MEAZipLoadingProcedure without initiating it up instantly. Use MEAZipLoadingProcedure.Initiate()
        /// </summary>
        /// <param name="zipBytes">Bytes of the .zip file that contains all assets</param>
        /// <param name="monoBehaviour">Instance of UnityEngine.MonoBehaviour that allows you to run IEnumerators (required for loading assets)</param>
        /// <param name="customJsonFunction">A custom function for JSON deserialization, in case you're using different JSON library</param>
        /// <param name="customJsonDeserializationArgs">Some extra arguments for custom JSON deserialization function, if needed</param>
        /// <returns></returns>
        public static MEAZipLoadingProcedure CreateLoadingProcedure(byte[] zipBytes, MonoBehaviour monoBehaviour, Func<string, Type, object[], object> customJsonFunction = default, object[] customJsonDeserializationArgs = null)
        {
            MEAZipLoadingProcedure zipLoadingProcedure = new MEAZipLoadingProcedure(zipBytes, monoBehaviour);
            if (customJsonFunction != default)
            {
                zipLoadingProcedure.JsonDeserializeFunction = customJsonFunction;
            }
            if (customJsonDeserializationArgs != null)
            {
                zipLoadingProcedure.JsonDeserializationArgs = customJsonDeserializationArgs;
            }
            return zipLoadingProcedure;
        }
        /// <summary>
        /// Get an entry class by the filename, which contains a plenty of information about it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">File name</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static AssetEntryMEA GetEntry<T>(string name)
        {
            try
            {
                return internalDatabase[typeof(T)].Where(x => x.Name == name).FirstOrDefault();
            } 
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entry with name ({name}) from the database.", ex);
            }
        }
        /// <summary>
        /// Get a value of an entry. Shortcut for ModExtraAssets.GetEntry<T>("name").ValueAs<T>();
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">File name</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static T GetEntryValue<T>(string name)
        {
            try
            {
                return internalDatabase[typeof(T)].Where(x => x.Name == name).FirstOrDefault().ValueAs<T>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entry value with name ({name}) from the database.", ex);
            }
        }
        /// <summary>
        /// Get all entry classes by the type, these contain a plenty of information about them
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static AssetEntryMEA[] GetAllEntries<T>()
        {
            try
            {
                return internalDatabase[typeof(T)].ToArray();
            }
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entries of type ({typeof(T).Name}) from the database.", ex);
            }
        }
        /// <summary>
        /// Get all entry values by the type.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static T[] GetAllEntryValues<T>()
        {
            try
            {
                return internalDatabase[typeof(T)].EntryCollectionValues<T>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entries of type ({typeof(T).Name}) from the database.", ex);
            }
        }
        /// <summary>
        /// Get all entry classes by the type and predicate, these contain a plenty of information about them
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="predicate">Predicate/condition, which filters the possible collection in return (eg. x => x.Name = "name").</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static AssetEntryMEA[] GetAllEntriesWhere<T>(Func<AssetEntryMEA, bool> predicate)
        {
            try
            {
                return internalDatabase[typeof(T)].Where(predicate).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entries of type ({typeof(T).Name}) from the database.", ex);
            }
        }
        /// <summary>
        /// Get all entry values by the type and predicate
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="predicate">Predicate/condition, which filters the possible collection in return (eg. x => x.Name = "name").</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static T[] GetAllEntryValuesWhere<T>(Func<AssetEntryMEA, bool> predicate)
        {
            try
            {
                return internalDatabase[typeof(T)].Where(predicate).ToArray().EntryCollectionValues<T>();
            }
            catch (Exception ex)
            {
                throw new Exceptions.EntryFailGrabException($"Failed to grab an entries of type ({typeof(T).Name}) from the database.", ex);
            }
        }

        /// <summary>
        /// Try getting an entry class by the filename, which contains a plenty of information about it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">File name</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static bool TryGetEntry<T>(string name, out AssetEntryMEA assetEntry)
        {
            try
            {
                assetEntry = internalDatabase[typeof(T)].Where(x => x.Name == name).FirstOrDefault();
            }
            catch (Exception ex)
            {
                assetEntry = default;
            }
            return assetEntry != null;
        }
        /// <summary>
        /// Try getting a value of an entry.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">File name</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.EntryFailGrabException">Happens if something is null or basically missing</exception>
        public static bool TryGetEntryValue<T>(string name, out T value)
        {
            try
            {
                value = internalDatabase[typeof(T)].Where(x => x.Name == name).FirstOrDefault().ValueAs<T>();
            }
            catch (Exception ex)
            {
                value = default;
            }
            return value != null;
        }
    }
}
