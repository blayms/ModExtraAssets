using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class FailedZipCommentParseException : Exception
    {
        public FailedZipCommentParseException(MEAZipLoadingProcedure procedure)
            : base($"Failed to parse {procedure.Path} comments. Check your comments for any syntax errors!")
        {

        }
    }
}
