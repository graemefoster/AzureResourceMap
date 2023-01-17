namespace AzureDiagrams.Resources;

public class VNetIntegration : AzureResource, ICanInjectIntoASubnet
{
    public AppServiceApp LinkedApp { get; }
    private readonly string _vnetIntegratedInto;

    public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public VNetIntegration(string id, string vnetIntegratedInto, AppServiceApp linkedApp)
    {
        LinkedApp = linkedApp;
        Id = id;
        _vnetIntegratedInto = vnetIntegratedInto;
    }

    public string[] SubnetIdsIAmInjectedInto => new[] { _vnetIntegratedInto };
}