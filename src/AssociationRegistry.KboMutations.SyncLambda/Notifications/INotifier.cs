namespace AssociationRegistry.KboMutations.SyncLambda.Notifications;

public interface INotifier
{
    Task NotifySuccess(int numberOfFiles);
    Task NotifyFailure(string reason);
}