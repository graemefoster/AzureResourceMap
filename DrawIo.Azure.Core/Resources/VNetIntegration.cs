namespace DrawIo.Azure.Core.Resources;

internal class VNetIntegration : AzureResource, ICanInjectIntoASubnet
{
    private readonly string _vnetIntegratedInto;

    public override string? Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public VNetIntegration(string id, string vnetIntegratedInto)
    {
        Id = id;
        _vnetIntegratedInto = vnetIntegratedInto;
    }

    public string[] SubnetIdsIAmInjectedInto => new[] { _vnetIntegratedInto };
}