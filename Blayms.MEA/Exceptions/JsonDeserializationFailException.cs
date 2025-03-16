using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Blayms.MEA.Exceptions
{
    internal class JsonDeserializationFailException : Exception
    {
        public JsonDeserializationFailException(string filePath, Exception innerException)
            : base($"Failed to deserialize ({filePath}),\nsee <https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial> for a JSON tutorial.\nThrowing this following exception:\n{innerException}")
        {
        }
        public JsonDeserializationFailException(VersatileFilePackage.AssetFile file, string reason) : base($"Failed to deserialize JSON string from {file}\nsee <https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial>\nReason: {reason}") { }
        public JsonDeserializationFailException(VersatileFilePackage.AssetFile file, Exception innerException) : base($"Failed to deserialize JSON string from {file}\nsee <https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial>\nThrowing this following exception:\n{innerException}") { }
    }
}
