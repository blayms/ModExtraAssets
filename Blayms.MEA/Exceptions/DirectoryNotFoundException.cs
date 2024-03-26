using System;
using System.Collections.Generic;
using System.Text;

namespace Blayms.MEA.Exceptions
{
    internal class DirectoryNotFoundException : Exception
    {
        public DirectoryNotFoundException(string path, Exception innerException)
            : base($"Directory under path ({path}) is not found.\nFollowing exception:\n{innerException}")
        {

        }
    }
}
