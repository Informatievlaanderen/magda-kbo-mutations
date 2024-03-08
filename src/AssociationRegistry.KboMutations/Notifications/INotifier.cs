namespace AssocationRegistry.KboMutations.Notifications;

public interface INotifier
{
    Task Notify(IMessage message);
}