namespace AzureDiagrams.Resources;

internal interface ICanExposePublicIPAddresses
{
    string[] PublicIpAddresses { get; }
}