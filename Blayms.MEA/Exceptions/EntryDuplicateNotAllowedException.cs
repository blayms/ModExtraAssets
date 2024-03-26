using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class EntryDuplicateNotAllowedException : Exception
    {
        public EntryDuplicateNotAllowedException(AssetEntryMEA assetEntry)
            : base($"You cannot add a duplicated entry into the database. The asset entry that caused this exception is {$"AssetEntryMEA of type ({assetEntry} from {assetEntry.EntryDirectory.ZipFilePath} & {assetEntry.EntryDirectory.InZipPath}"}")
        {

        }
    }
}
