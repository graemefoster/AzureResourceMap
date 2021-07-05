using System;
using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;

namespace DrawIo.Azure.Core
{
    internal class IgnoreMeResource : AzureResource
    {
        public override IEnumerable<string> ToDrawIo()
        {
            return Array.Empty<string>();
        }
    }
}