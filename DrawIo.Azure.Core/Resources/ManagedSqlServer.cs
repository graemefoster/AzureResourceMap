namespace DrawIo.Azure.Core.Resources;

internal class ManagedSqlServer : AzureResource
{
    public override string Image => "img/lib/azure2/databases/SQL_Server.svg";

    public void DiscoveredDatabase(ManagedSqlDatabase managedSqlDatabase)
    {
        OwnsResource(managedSqlDatabase);
    }
}