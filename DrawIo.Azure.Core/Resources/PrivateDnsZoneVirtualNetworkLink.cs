using System;
using System.Collections.Generic;

namespace DrawIo.Azure.Core.Resources
{
    class PrivateDnsZoneVirtualNetworkLink : AzureResource
    {
        public override bool IsSpecific => true;

        public override IEnumerable<string> ToDrawIo(int x, int y)
        {
            return Array.Empty<string>();
        }
    }
}