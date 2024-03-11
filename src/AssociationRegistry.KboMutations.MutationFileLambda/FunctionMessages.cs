using Amazon.SQS.Model;
using AssocationRegistry.KboMutations.Notifications;
using AssociationRegistry.Notifications;

namespace AssociationRegistry.KboMutations.MutationFileLambda;

public readonly record struct KboMutationFileLambdaSqsBerichtBatchNietVerstuurd : IMessage
{
    private readonly int _failedMessageCount;

    public KboMutationFileLambdaSqsBerichtBatchNietVerstuurd(List<BatchResultErrorEntry> batchResponseFailed) => _failedMessageCount = batchResponseFailed.Count;
    public string Value => $"KBO mutation file lambda kon {_failedMessageCount} berichten niet naar SQS versturen.";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationFileLambdaSqsBerichtBatchVerstuurd : IMessage
{
    private readonly int _messageCount;

    public KboMutationFileLambdaSqsBerichtBatchVerstuurd(List<SendMessageBatchResultEntry> batchResponseSuccesful) => _messageCount = batchResponseSuccesful.Count;
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
