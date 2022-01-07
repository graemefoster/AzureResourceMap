namespace DrawIo.Azure.Core.Resources;

internal interface ICanExposePublicIPAddresses
{
    string[] PublicIpAddresses { get; }
}