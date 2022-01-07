namespace DrawIo.Azure.Core.Resources;

internal class VNetIntegration : AzureResource, ICanInjectIntoASubnet
{
    private readonly string _vnetIntegratedInto;

    public VNetIntegration(string id, string vnetIntegratedInto)
    {
        Id = id;
        _vnetIntegratedInto = vnetIntegratedInto;
    }

    public string[] SubnetIdsIAmInjectedInto => new[] { _vnetIntegratedInto };
}