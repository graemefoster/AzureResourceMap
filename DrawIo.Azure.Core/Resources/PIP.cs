namespace DrawIo.Azure.Core.Resources;

internal class PIP : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Public_IP_Addresses.svg";

    // public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
    // {
    //     allResources.OfType<Nic>().Where(x => x.ExposedBy(this)).ForEach(x => Link(x, graph));
    // }
}