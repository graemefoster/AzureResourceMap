namespace AzureDiagrams.Resources;

public class VNetIntegration : AzureResource, ICanInjectIntoASubnet
{
    public App LinkedApp { get; }
    private readonly string _vnetIntegratedInto;

    public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public VNetIntegration(string id, string vnetIntegratedInto, App linkedApp)
    {
        LinkedApp = linkedApp;
        Id = id;
        _vnetIntegratedInto = vnetIntegratedInto;
    }

    public string[] SubnetIdsIAmInjectedInto => new[] { _vnetIntegratedInto };
}