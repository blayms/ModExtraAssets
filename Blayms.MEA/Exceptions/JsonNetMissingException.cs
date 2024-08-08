using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class JsonNetMissingException : Exception
    {
        private const string msg = "\nNewtonsoft.Json.dll assembly file is missing.\nMake sure you've used .zip configuration (https://sites.google.com/view/mea-docs/main/useful-information/zip-configuration)\nAND installed dll from here (https://www.nuget.org/packages/Newtonsoft.Json/).\nUnity's default JSONUtility is made for simple json management, we prefer to use JSON.Net for stuff like deserializing sprite sheets data structures, which is probably the source of this exception!";
        public JsonNetMissingException(string additionalDetails)
            : base(additionalDetails+msg)
        {

        }
    }
}
