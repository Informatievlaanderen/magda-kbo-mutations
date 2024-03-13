using AssociationRegistry.Notifications;

namespace AssociationRegistry.KboMutations.MutationFileLambda.Logging;

public readonly record struct KboMutationFileLambdaSqsBerichtBatchNietVerstuurd : IMessage
{
    private readonly int _failedMessageCount;

    public KboMutationFileLambdaSqsBerichtBatchNietVerstuurd(int failedMessageCount) => _failedMessageCount = failedMessageCount;
    public string Value => $"KBO mutation file lambda kon {_failedMessageCount} berichten niet naar SQS versturen.";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationFileLambdaSqsBerichtBatchVerstuurd : IMessage
{
    private readonly int _messageCount;

    public KboMutationFileLambdaSqsBerichtBatchVerstuurd(int successfulMessageCount) => _messageCount = successfulMessageCount;
    public string Value => $"KBO mutation file lambda kon {_messageCount} berichten naar SQS versturen.";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationFileLambdaMessageProcessorGefaald : IMessage
{
    private readonly Exception _exception;

    public KboMutationFileLambdaMessageProcessorGefaald(Exception exception) => _exception = exception;
    public string Value => $"KBO mutation file lambda heeft onverwachte fout in de processor! {_exception.Message}";
    public NotifyType Type => NotifyType.Failure;
}
