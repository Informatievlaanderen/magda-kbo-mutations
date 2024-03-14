using System.Text;
using AssociationRegistry.Notifications;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Logging;

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

    public string Value => $"KBO mutation lambda heeft {_aantalBestanden} bestanden gevonden.";
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
    private readonly string _queueOmschrijving;
    private readonly int _approximateMessageCount;

    public KboMutationLambdaQueueStatus(string queueOmschrijving, int approximateMessageCount)
    {
        _queueOmschrijving = queueOmschrijving;
        _approximateMessageCount = approximateMessageCount;
    }

    public string Value
        => $"{_queueOmschrijving}: {_approximateMessageCount}";
    
    public NotifyType Type => _approximateMessageCount > 0 
        ? NotifyType.Failure 
        : NotifyType.Success;
}
