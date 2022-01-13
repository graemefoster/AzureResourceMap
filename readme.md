# AzureDiagrams

## Generate a diagram from your Azure Resources

## Usage
```bash
az login 
az account set --subscription "<subscription-name>"
dotnet AzureDiagrams.dll --subscription-id <subscription-id> --resource-group <resource-group> --resource-group <resource-group> --output c:/temp/
```

## Example outputs
### Azure App Service with App Insights / database / Key Vault
![AzureSimple](./assets/grfsq2-platform-test-rg.drawio.png)

### More complex with VNets and private endpoints
![AzureSimple](./assets/grfsq2-platform-test-rg.drawio.png)

## How does it work?
AzureDiagrams queries the Azure Resource Management APIs to introspect resource-groups. It then uses a set of strategies to enrich the raw data, building a model that can be projected into other formats.

It's not 100% guaranteed to be correct but it should give a good first pass at fairly complex architectures/

To layout the components I use the amazing [AutomaticGraphLayout](https://github.com/microsoft/automatic-graph-layout) library.

## Todo
There are many, many Azure services not yet covered. I'll try and put a table here of what is covered, and how comprehensive it is covered.

## Visulations
The initial version supports Draw.IO diagrams. 


