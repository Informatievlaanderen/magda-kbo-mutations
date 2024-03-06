namespace AssociationRegistry.KboMutations.Notifications;

public interface INotifier
{
    Task NotifyLambdaTriggered();
    Task NotifyLambdaFinished();
    Task NotifyLambdaFailed(string exceptionMessage);
    
    Task NotifyDownloadFileSuccess(int numberOfFiles);
    Task NotifyFailure(string reason);
}