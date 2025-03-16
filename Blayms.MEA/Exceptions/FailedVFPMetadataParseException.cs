using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Blayms.MEA.Exceptions
{
    internal class FailedVFPMetadataParseException : Exception
    {
        public FailedVFPMetadataParseException(MEAVfpLoadingProcedure procedure)
            : base($"Failed to parse {procedure.Name} package metadata. Check your metadata values for any syntax errors!")
        {

        }
        public FailedVFPMetadataParseException(VersatileFilePackage.AssetFile assetFileVfp)
            : base($"Failed to parse {assetFileVfp.Name} file metadata. Check your metadata values for any syntax errors!")
        {

        }
    }
}
