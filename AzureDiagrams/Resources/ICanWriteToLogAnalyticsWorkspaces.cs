namespace AzureDiagrams.Resources;

internal interface ICanWriteToLogAnalyticsWorkspaces
{
    bool DoYouWriteTo(string customerId);
    void CreateFlowBackToMe(LogAnalyticsWorkspace workspace);
}