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
    }
}
