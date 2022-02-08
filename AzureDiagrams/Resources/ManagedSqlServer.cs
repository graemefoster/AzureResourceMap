namespace AzureDiagrams.Resources;

internal class ManagedSqlServer : AzureResource
{
    public override bool IsPureContainer => true;

    public void DiscoveredDatabase(ManagedSqlDatabase managedSqlDatabase)
    {
        OwnsResource(managedSqlDatabase);
    }
}