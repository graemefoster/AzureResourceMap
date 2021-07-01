using System;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core
{
    internal class AzureResourceConverter : JsonConverter
    {
        Func<AzureResource, string, Task<JObject>> _enricher;

        public AzureResourceConverter(Func<AzureResource, string, Task<JObject>> enricher)
        {
            _enricher = enricher;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(AzureResource).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            static AzureResource GetAzureResource(string type) => type switch
            {
                "Microsoft.Network/virtualNetworks" => new VNet(),
                "Microsoft.Network/privateEndpoints" => new PrivateEndpoint(),
                "Microsoft.Network/privateDnsZones" => new PrivateDnsZone(),
                "Microsoft.Network/privateDnsZones/virtualNetworkLinks" => new PrivateDnsZoneVirtualNetworkLink(),
                "Microsoft.Network/networkInterfaces" => new Nic(),
                "Microsoft.ContainerService/managedClusters" => new AKS(),
                "Microsoft.ContainerRegistry/registries" => new ACR(),
                "Microsoft.Web/serverFarms" => new ASP(),
                "Microsoft.Web/sites" => new App(),
                "Microsoft.ApiManagement/service" => new APIm(),
                "Microsoft.Compute/virtualMachines" => new VM(),
                "Microsoft.Compute/disks" => new Disk(),
                "Microsoft.OperationalInsights/workspaces" => new LogAnalyticsWorkspace(),
                "microsoft.insights/components" => new AppInsights(),
                "Microsoft.Storage/storageAccounts" => new StorageAccount(),
                "Microsoft.Network/networkSecurityGroups" => new NSG(),
                "Microsoft.Network/publicIPAddress" => new PIP(),
                "Microsoft.Compute/virtualMachines/extensions" => new VMExtension(),
                "Microsoft.ManagedIdentity/userAssignedIdentities" => new UserAssignedManagedIdentity(),
                _ => new AzureResource()
            };

            JObject jo = JObject.Load(reader);

            // Using a nullable bool here in case "is_album" is not present on an item
            var item = GetAzureResource((string) jo["type"]);
            serializer.Populate(jo.CreateReader(), item);

            if (item.FetchFull)
            {
                var fullItem = _enricher(item, item.ApiVersion).GetAwaiter().GetResult();
                item.Enrich(fullItem);
            }

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}