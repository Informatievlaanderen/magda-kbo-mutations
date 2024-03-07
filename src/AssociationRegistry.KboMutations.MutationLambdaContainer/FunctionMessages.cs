using System.Text;
using AssocationRegistry.KboMutations.Notifications;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public readonly record struct KboMutationLambdaGestart : IMessage
{
    public string Value => "KBO mutation lambda gestart.";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationLambdaBestandenOpgehaald : IMessage
{
    private readonly int _aantalBestanden;

    public KboMutationLambdaBestandenOpgehaald(int aantalBestanden)
    {
        _aantalBestanden = aantalBestanden;
    }

    public string Value => $"KBO mutation lambda heeft {_aantalBestanden} bestanden opgehaald.";
    public NotifyType Type => NotifyType.None;
}

public readonly record struct KboMutationLambdaGefaald : IMessage
{
    private readonly Exception _exception;

    public KboMutationLambdaGefaald(Exception exception)
    {
        _exception = exception;
    }
    public string Value => $"KBO mutation lambda gefaald. {_exception.Message}";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationLambdaVoltooid : IMessage
{
    public string Value => "KBO mutation lambda voltooid.";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationLambdaQueueStatus : IMessage
{
    private readonly string _queueArn;
    private readonly int _approximateMessageCount;

    public KboMutationLambdaQueueStatus(string queueArn, int approximateMessageCount)
    {
        _queueArn = queueArn;
        _approximateMessageCount = approximateMessageCount;
    }

    public string Value
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine($"KBO mutation file queue statistieken:");
            sb.AppendLine($"- Queue : {_queueArn[(_queueArn.LastIndexOf(':') + 1)..]}");
            sb.AppendLine($"- Aantal berichten : {(_approximateMessageCount.Equals(0) ? "Geen" : _approximateMessageCount)} resterende berichten.");
            return sb.ToString();
        }
    }

    public NotifyType Type => _approximateMessageCount > 0 
        ? NotifyType.Failure 
        : NotifyType.Success;
}
