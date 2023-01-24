using AzureDiagrams.Resources;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Shouldly;

namespace AzureDiagramsTests.WebApps;

public class WebAppWithPrivateEndpoint
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

    
    [Fact]
    public async Task CanDrawCondensedDiagram()
    {
        var resources = BuildScenario();

        var diagram = await AzureDiagramGenerator.DrawIoDiagramGenerator.DrawDiagram(
            resources,
            true,
            false,
            false,
            true,
            false);

        diagram.ShouldMatchApproved();
    }

    private static AzureResource[] BuildScenario()
    {
        var vnetId = "/subscriptions/123/resourceGroups/rg1/providers/microsoft.network/virtualnetworks/vnet123";

        var vnet = new VNet(vnetId, "test-vnet", new[]
        {
            new VNet.Subnet("private-endpoints", "10.1.1.0/24"),
            new VNet.Subnet("vnet-integration", "10.1.2.0/24"),
        });

        var appServicePlanId =
            "/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/serverFarms/TestAppServicePlan";

        var asp = new AppServicePlan(appServicePlanId, "TestAppServicePlan");

        var peId = new Guid("36a566f9-f6b8-439c-9371-204842160e2b").ToString();
        var pe2Id = new Guid("36a566f9-f6b8-439c-9371-204842160e2d").ToString();

        var app1HostName = "TestApp1.azurewebsites.net";
        var app2HostName = "TestApp2.azurewebsites.net";

        var app1 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp1",
            appServicePlanId,
            "TestApp1",
            false,
            Array.Empty<string>(),
            new[] { app1HostName },
            virtualNetworkSubnetId: $"{vnetId}/subnets/vnet-integration",
            new PrivateEndpointExtensions(new[] { peId }));

        var app2 = new AppServiceApp("/subscriptions/123/resourceGroups/rg1/providers/Microsoft.Web/sites/TestApp2",
            appServicePlanId,
            "TestApp2",
            false,
            new[] { $"https://{app1HostName}" },
            new[] { app2HostName },
            virtualNetworkSubnetId: $"{vnetId}/subnets/vnet-integration",
            new PrivateEndpointExtensions(new[] { pe2Id }));

        var nicId = new Guid("36a566f9-f6b8-439c-9371-204842160e2c").ToString();
        var nic = new Nic(nicId,
            IpConfigurations.ForPrivateEndpoint("10.1.1.1", $"{vnetId}/subnets/private-endpoints", app1HostName));

        var privateEndpoint = new PrivateEndpoint(peId, new[] { $"{vnetId}/subnets/private-endpoints" },
            new[] { nic.Id }, new[] { $"https://{app1HostName}" });

        var nic2Id = new Guid("36a566f9-f6b8-439c-9371-204842160e2e").ToString();
        var nic2 = new Nic(nic2Id,
            IpConfigurations.ForPrivateEndpoint("10.1.1.2", $"{vnetId}/subnets/private-endpoints", app2HostName));

        var privateEndpoint2 = new PrivateEndpoint(pe2Id, new[] { $"{vnetId}/subnets/private-endpoints" },
            new[] { nic2.Id }, new[] { $"https://{app2HostName}" });

        var azureResources = new AzureResource[]
        {
            vnet,
            asp,
            app1,
            app2,
            nic,
            nic2,
            privateEndpoint,
            privateEndpoint2
        };

        var resources = azureResources.Process();
        return resources;
    }
}