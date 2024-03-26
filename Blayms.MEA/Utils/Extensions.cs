using Blayms.MEA.Utils.ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blayms.MEA.Utils
{
    internal static class Extensions
    {
        #region ZipEntry
        internal static byte[] ReadBytesFromZipStream(this ZipEntry entry, Stream stream)
        {
            byte[] ret = null;
            ret = new byte[entry.Size];
            stream.Read(ret, 0, ret.Length);

            return ret;
        }
        #endregion
        #region IList<AssetEntry>
        public static T[] EntryCollectionValues<T>(this IList<AssetEntryMEA> entries)
        {
            T[] values = new T[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                values[i] = entries[i].ValueAs<T>();
            }

            return values;
        }
        #endregion
    }
}
