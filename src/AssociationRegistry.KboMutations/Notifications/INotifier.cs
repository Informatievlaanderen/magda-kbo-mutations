namespace AssociationRegistry.KboMutations.Notifications;

public interface INotifier
{
    Task NotifySuccess(int numberOfFiles);
    Task NotifyFailure(string reason);
}