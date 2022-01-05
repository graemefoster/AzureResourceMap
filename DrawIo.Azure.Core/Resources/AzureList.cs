namespace DrawIo.Azure.Core.Resources;

internal class AzureList<T> where T : AzureResource
{
    public string NextLink { get; set; }
    public T[] Value { get; set; }
}