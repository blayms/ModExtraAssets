using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Blayms.MEA.Exceptions
{
    internal class JsonDeserializationFailException : Exception
    {
        public JsonDeserializationFailException(string filePath, Exception innerException)
            : base($"Failed to deserialize ({filePath}),\nsee <https://sites.google.com/view/mea-docs/main/useful-information/json-tutorial> for a JSON tutorial. Throwing this following exception:\n{innerException}")
        {
        }
    }
}
