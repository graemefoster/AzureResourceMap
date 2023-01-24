using AzureDiagrams.Resources;
using Shouldly;

namespace AzureDiagramsTests.VirtualWans;

public class VWanWithVHub
{
    [Fact]
    public async Task CanDrawDiagram()
    {
        var resources = BuildScenario();

        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            resources,
            false,
            false,
            false,
            true,
            false);

        diagram.ShouldMatchApproved();
    }

    private static AzureResource[] BuildScenario()
    {
        var vwanId = new Guid("6d4ced93-d694-4e0c-b3a0-6cef2c095061").ToString();
        var vnetId = "/subscriptions/123/resourceGroups/rg1/providers/microsoft.network/virtualnetworks/vnet123";

        var vwan = new VWan(vwanId, "vwan");

        var vnet = new VNet(vnetId, "test-vnet", Array.Empty<VNet.Subnet>());

        var vhub = new VHub(
            vwanId, 
            new Guid("318e3768-dacb-4e2c-b87e-288107259a6c").ToString(), 
            "Hub-1",
            new[] { vnetId }
            );

        // var appServicePlanId =
        //     "/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/serverFarms/TestAppServicePlan";
        //
        // var asp = new AppServicePlan(appServicePlanId, "TestAppServicePlan");
        //
        // var peId = new Guid("36a566f9-f6b8-439c-9371-204842160e2b").ToString();
        // var pe2Id = new Guid("36a566f9-f6b8-439c-9371-204842160e2d").ToString();
        //
        // var app1HostName = "TestApp1.azurewebsites.net";
        // var app2HostName = "TestApp2.azurewebsites.net";
        //
        // var app1 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp1",
        //     appServicePlanId,
        //     "TestApp1",
        //     false,
        //     Array.Empty<string>(),
        //     new[] { app1HostName },
        //     virtualNetworkSubnetId: $"{vnetId}/subnets/vnet-integration",
        //     new PrivateEndpointExtensions(new[] { peId }));
        //
        // var app2 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp2",
        //     appServicePlanId,
        //     "TestApp2",
        //     false,
        //     new[] { $"https://{app1HostName}" },
        //     new[] { app2HostName },
        //     virtualNetworkSubnetId: $"{vnetId}/subnets/vnet-integration",
        //     new PrivateEndpointExtensions(new[] { pe2Id }));
        //
        // var nicId = new Guid("36a566f9-f6b8-439c-9371-204842160e2c").ToString();
        // var nic = new Nic(nicId,
        //     IpConfigurations.ForPrivateEndpoint("10.1.1.1", $"{vnetId}/subnets/private-endpoints", app1HostName));
        //
        // var privateEndpoint = new PrivateEndpoint(peId, new[] { $"{vnetId}/subnets/private-endpoints" },
        //     new[] { nic.Id }, new[] { $"https://{app1HostName}" });
        //
        // var nic2Id = new Guid("36a566f9-f6b8-439c-9371-204842160e2e").ToString();
        // var nic2 = new Nic(nic2Id,
        //     IpConfigurations.ForPrivateEndpoint("10.1.1.2", $"{vnetId}/subnets/private-endpoints", app2HostName));
        //
        // var privateEndpoint2 = new PrivateEndpoint(pe2Id, new[] { $"{vnetId}/subnets/private-endpoints" },
        //     new[] { nic2.Id }, new[] { $"https://{app2HostName}" });

        var azureResources = new AzureResource[]
        {
            vwan,
            vhub,
            vnet
        };

        var resources = azureResources.Process();
        return resources;
    }
}