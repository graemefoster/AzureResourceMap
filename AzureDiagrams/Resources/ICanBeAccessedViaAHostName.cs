namespace AzureDiagrams.Resources;

internal interface ICanBeAccessedViaAHostName
{
    bool CanIAccessYouOnThisHostName(string hostname);
}