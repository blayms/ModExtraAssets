using System;
using System.Collections.Generic;
using System.Text;

namespace Blayms.MEA.Exceptions
{
    internal class EntryFailGrabException : Exception
    {
        public EntryFailGrabException(object msg, Exception innerException)
            : base($"{msg}\nFollowing exception:\n{innerException}")
        {

        }
    }
}
