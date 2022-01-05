using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources;

namespace DrawIo.Azure.Core;

internal class IgnoreMeResource : AzureResource
{
    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new IgnoreNodeBuilder(this);
    }
}