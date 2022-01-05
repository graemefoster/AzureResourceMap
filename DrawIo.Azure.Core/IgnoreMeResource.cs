using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources;

namespace DrawIo.Azure.Core;

internal class IgnoreMeResource : AzureResource
{
    public override IDiagramResourceBuilder CreateNodeBuilder()
    {
        return new IgnoreNodeBuilder();
    }
}