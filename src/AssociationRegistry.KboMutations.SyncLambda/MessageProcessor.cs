using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.SQS;
using AssocationRegistry.KboMutations;
using AssocationRegistry.KboMutations.Configuration;
using AssocationRegistry.KboMutations.Messages;
using AssociationRegistry.Acties.SyncKbo;
using AssociationRegistry.Framework;
using AssociationRegistry.Kbo;
using AssociationRegistry.Magda;
using AssociationRegistry.Vereniging;
using NodaTime;
using ResultNet;

namespace AssociationRegistry.KboMutations.SyncLambda;

public class MessageProcessor
{
    private readonly KboSyncConfiguration _kboSyncConfiguration;
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;

    public MessageProcessor(IAmazonS3 s3Client, IAmazonSQS sqsClient, KboSyncConfiguration kboSyncConfiguration)
    {
        _s3Client = s3Client;
        _sqsClient = sqsClient;
        _kboSyncConfiguration = kboSyncConfiguration;
    }

    public async Task ProcessMessage(SQSEvent sqsEvent, 
        ILambdaLogger contextLogger,
        MagdaGeefVerenigingService geefOndernemingService,
        IVerenigingsRepository repository,
        CancellationToken cancellationToken)
    {
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.MutationFileBucketName)}:{_kboSyncConfiguration.MutationFileBucketName}");
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.MutationFileQueueUrl)}:{_kboSyncConfiguration.MutationFileQueueUrl}");
        contextLogger.LogInformation($"{nameof(_kboSyncConfiguration.SyncQueueUrl)}:{_kboSyncConfiguration.SyncQueueUrl}");

        var handler = new SyncKboCommandHandler(geefOndernemingService);
        
        foreach (var record in sqsEvent.Records)
        {
            var message = JsonSerializer.Deserialize<TeSynchroniserenKboNummerMessage>(record.Body);
            
            contextLogger.LogInformation($"Processing record: {message.KboNummer} from file '{message.Filename}'");

            var syncKboCommand = new SyncKboCommand(KboNummer.Create(message.KboNummer));
            var commandMetadata = new CommandMetadata("KboSync", SystemClock.Instance.GetCurrentInstant(), Guid.NewGuid(), null);
            var commandEnvelope = new CommandEnvelope<SyncKboCommand>(syncKboCommand, commandMetadata);
            
            var commandResult = await handler.Handle(commandEnvelope, repository, cancellationToken);

            contextLogger.LogInformation($"Sync resulted in sequence '{commandResult.Sequence}'. HasChanges? {commandResult.HasChanges()}");
        }
    }
}