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

public static class CommonResources
{
    public const string AAD = "c8be31af-ab7c-4759-862d-9b9344a4a54b";
    public const string CoreServices = "516f3ff1-d065-4ce2-806b-160eb431bae5";
    public const string Diagnostics = "813867a2-b7e6-4bef-9d78-d36e02e8b533";
}