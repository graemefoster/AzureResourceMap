using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class VMExtension : AzureResource
{
    private string _vm;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        _vm = string.Join('/', Id.Split('/')[..^2]).ToLowerInvariant();
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<VM>().Single(x => x.Id.ToLowerInvariant() == _vm).AddExtension(this);
    }
}