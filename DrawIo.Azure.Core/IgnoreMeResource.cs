using System;
using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core
{
    internal class IgnoreMeResource : AzureResource
    {
        public override IAzureNodeBuilder CreateNodeBuilder()
        {
            return new IgnoreNodeBuilder();
        }

        internal class IgnoreNodeBuilder : IAzureNodeBuilder
        {
            public IEnumerable<Node> CreateNodes(AzureResource resource)
            {
                yield break;
            }
        }
    }
}