using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class DllsMissingException : Exception
    {
        public DllsMissingException(string nameType)
            : base($"Failed to find a type ({nameType}) because ModExtraAssets doesn't know where it needs to try grabbing types. You have to add some setting to your .zip to link the dll with ModExtraAssets, learn more on that here - <https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration>")
        {

        }
    }
}
