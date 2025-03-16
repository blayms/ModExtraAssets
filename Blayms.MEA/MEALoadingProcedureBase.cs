using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Blayms.MEA
{
    /// <summary>
    /// 
    /// </summary>
    public class MEALoadingProcedureBase
    {
        public delegate void LoadingResultDelegate(LoadingResult result);
        public delegate void OnEntryLoaded(AssetEntryMEA entry);
        protected MonoBehaviour monoBehaviour;
        private string name;
        private LoadingResult result;
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
        /// <summary>
        /// A loading state enum for loading procedures
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
            /// Deserializes all *.json files from *.zip (along with sprite sheet data files)
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
        internal void SetResult(LoadingResult result)
        {
            this.result = result;
            onLoadingResultDefined?.Invoke(result);
        }
        /// <summary>
        /// Loading IEnumerator
        /// </summary>
        protected virtual IEnumerator LoadIEnumerator()
        {
            yield return null;
        }
        protected void Invoke_onEntryLoaded(AssetEntryMEA entryMEA)
        {
            onEntryLoaded?.Invoke(entryMEA);
        }
        protected void Invoke_onLoadingResultDefined(LoadingResult result)
        {
            onLoadingResultDefined?.Invoke(result);
        }

        private string filePath = null;
        private byte[] fileBytes = null;
        private bool usesBytes;

        public MEALoadingProcedureBase(string filePath, MonoBehaviour monoBehaviour)
        {
            this.filePath = filePath;
            name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            this.monoBehaviour = monoBehaviour;
        }
        public MEALoadingProcedureBase(byte[] fileBytes, MonoBehaviour monoBehaviour)
        {
            this.fileBytes = fileBytes;
            this.monoBehaviour = monoBehaviour;
            name = $"Bytes-Based {GetType().FullName}";
            usesBytes = true;
        }
        /// <summary>
        /// Copies the name of the file used for this procedure<para><b>If your procedure uses file bytes, the name will be "Bytes-Based {Type}",</b><br><b>because it's impossible to grab a file name straight up from byte array</b></br></para>
        /// </summary>
        public string Name => name;
        /// <summary>
        /// Path of the .zip file
        /// </summary>
        public string Path
        {
            get
            {
                if (usesBytes)
                {
                    throw new DataMisalignedException("Current MEAZipLoadingProcedure does not use path for populating the database. Use \"Bytes\" property instead.");
                }
                return filePath;
            }
        }
        /// <summary>
        /// Bytes of the .zip file
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                if (!usesBytes)
                {
                    throw new DataMisalignedException("Current MEAZipLoadingProcedure does not use bytes for populating the database. Use \"Path\" property instead.");
                }
                return fileBytes;
            }
        }
        /// <summary>
        /// True if this procedure was created with file bytes
        /// </summary>
        public bool UsesFileBytes => usesBytes;
    }
}
