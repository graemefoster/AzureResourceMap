using DrawIo.Azure.Core.Diagrams;

namespace DrawIo.Azure.Core.Resources;

internal class IgnoreMeResource : AzureResource
{
    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new IgnoreNodeBuilder(this);
    }
}