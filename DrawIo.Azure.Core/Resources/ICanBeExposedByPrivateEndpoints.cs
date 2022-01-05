namespace DrawIo.Azure.Core.Resources;

internal interface ICanBeExposedByPrivateEndpoints
{
    bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint);
}