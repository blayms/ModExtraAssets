using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class DllsMissingException : Exception
    {
        private const string typeMsg = "Failed to find a type ({nameType}) because ModExtraAssets doesn't know where it needs to try grabbing types. If you are using .zip for loading assets, some setting must be included in your .zip to link the dll with ModExtraAssets, learn more on that here - <https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration>\n\nBut if you are using .vfp for loading assets, some pair must be included in your package metadata, learn more on that here - <https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration>";
        private const string defaultDLLNotFoundMsg = "Dll under name ({nameType}) not found. It doesn't exist or, simply, not referenced in ModExtraAssets database, learn more on that here:\nZip: <https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration>\nVfp: <https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration>";
        public DllsMissingException(string nameType, bool typeMissingMessage = true)
            : base((typeMissingMessage ? typeMsg : defaultDLLNotFoundMsg).Replace("{nameType}", nameType))
        {

        }
    }
}
