using System;
using System.Collections.Generic;

namespace DrawIo.Azure.Core.Resources
{
    class VMExtension : AzureResource
    {
        public override bool IsSpecific => true;

        public override IEnumerable<string> ToDrawIo()
        {
            return Array.Empty<string>();
        }
    }
}