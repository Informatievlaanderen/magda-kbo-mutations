using AssocationRegistry.KboMutations.Notifications;

namespace AssociationRegistry.KboMutations.MutationFileLambda;

public readonly record struct KboMutationFileLambdaGestart : IMessage
{
    private readonly int _messageReceiveCount;

    public KboMutationFileLambdaGestart(int messageReceiveCount) => _messageReceiveCount = messageReceiveCount;
    public string Value => $"KBO mutation file lambda gestart. Aantal berichten te verwerken: {_messageReceiveCount}";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationFileLambdaVoltooid : IMessage
{
    private readonly int _messageProcessCount;

    public KboMutationFileLambdaVoltooid(int messageProcessCount) => _messageProcessCount = messageProcessCount;
    public string Value => $"KBO mutation file lambda voltooid. Aantal berichten verwerkt: {_messageProcessCount}";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationFileLambdaGefaald : IMessage
{
    private readonly Exception _exception;

    public KboMutationFileLambdaGefaald(Exception exception) => _exception = exception;
    public string Value => $"KBO mutation file lambda gefaald. {_exception.Message}";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationFileLambdaKonSqsBerichtBatchNietVersturen : IMessage
{
    private readonly int _failedMessageCount;

    public KboMutationFileLambdaKonSqsBerichtBatchNietVersturen(int failedMessageCount) => _failedMessageCount = failedMessageCount;
    public string Value => $"KBO mutation file lambda kon {_failedMessageCount} berichten niet naar SQS versturen.";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationFileLambdaMessageProcessorGefaald : IMessage
{
    private readonly Exception _exception;

    public KboMutationFileLambdaMessageProcessorGefaald(Exception exception) => _exception = exception;
    public string Value => $"KBO mutation file lambda heeft onverwachte fout in de processor! {_exception.Message}";
    public NotifyType Type => NotifyType.Failure;
}
