namespace AssocationRegistry.KboMutations.Notifications;

public interface IMessage
{
    public string Value { get; }    
    public NotifyType Type { get; }
}