using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AssocationRegistry.KboMutations.Messages;
using AssociationRegistry.Framework;
using AssociationRegistry.Kbo;
using AssociationRegistry.Vereniging;
using NodaTime.Extensions;
using ResultNet;

namespace AssociationRegistry.KboMutations.SyncLambda;

public class MessageProcessor
{
    private readonly IMagdaGeefVerenigingService _magdaGeefVerenigingService;
    private readonly IVerenigingsRepository _verenigingsRepository;

    public MessageProcessor(IMagdaGeefVerenigingService magdaGeefVerenigingService, IVerenigingsRepository verenigingsRepository)
    {
        _magdaGeefVerenigingService = magdaGeefVerenigingService;
        _verenigingsRepository = verenigingsRepository;
    }

    public async Task ProcessMessage(SQSEvent sqsEvent,
        ILambdaLogger contextLogger,
        CancellationToken cancellationToken)
    {
        foreach (var record in sqsEvent.Records)
        {
            contextLogger.LogInformation("Processing record body: " + record.Body);

            var message = JsonSerializer.Deserialize<TeSynchroniserenKboNummerMessage>(record.Body);

            await Handle(contextLogger, message, Guid.NewGuid(), cancellationToken);
        }
    }

    private async Task Handle(ILambdaLogger logger,
        TeSynchroniserenKboNummerMessage? message,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var kboNummer = KboNummer.Create(message.KboNummer!);

        var (verenigingVolgensKbo, verenigingMetRechtspersoonlijkheid) = GetVerenigingData(logger, kboNummer, correlationId, cancellationToken);
        
        // TODO : Implementatie voor het berekenen van de diff tussen beiden
    }

    private (VerenigingVolgensKbo, VerenigingMetRechtspersoonlijkheid) GetVerenigingData(ILambdaLogger logger, KboNummer kboNummer, Guid correlationId, CancellationToken cancellationToken)
    {
        var verenigingFromMagdaTask = GetVerenigingFromMagda(logger, kboNummer, correlationId, cancellationToken);
        var verenigingFromRepositoryTask = GetVerenigingFromRepository(logger, kboNummer, cancellationToken);

        // Load simultaneous from both MAGDA and our repository
        Task.WaitAll(new Task[]
        {
            verenigingFromMagdaTask,
            verenigingFromRepositoryTask
        }, cancellationToken);

        var verenigingFromMagda = verenigingFromMagdaTask.Result.Data;
        var verenigingFromRepository = verenigingFromRepositoryTask.Result;

        return (verenigingFromMagda, verenigingFromRepository);
    }

    private async Task<Result<VerenigingVolgensKbo>> GetVerenigingFromMagda(ILambdaLogger logger, KboNummer kboNummer, Guid correlationId, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Calling {nameof(IMagdaGeefVerenigingService)}.{nameof(IMagdaGeefVerenigingService.GeefVereniging)} for KBO '{kboNummer}'");
        var response = await _magdaGeefVerenigingService.GeefVereniging(kboNummer, new CommandMetadata("KBO", DateTime.UtcNow.ToInstant(), correlationId), cancellationToken);

        return response;
    }

    private async Task<VerenigingMetRechtspersoonlijkheid> GetVerenigingFromRepository(ILambdaLogger logger, KboNummer kboNummer, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Calling {nameof(IVerenigingsRepository)}.{nameof(IVerenigingsRepository.GetVCodeAndNaam)} for KBO '{kboNummer}'");
        var vCodeAndNaamResponse = await _verenigingsRepository.GetVCodeAndNaam(kboNummer);

        if (vCodeAndNaamResponse is null || vCodeAndNaamResponse.VCode is null) throw new ApplicationException($"Could not find VCode and Name for KBO number {kboNummer}");

        var vCode = VCode.Create(vCodeAndNaamResponse.VCode);

        logger.LogInformation($"Calling {nameof(IVerenigingsRepository)}.{nameof(IVerenigingsRepository.Load)} for VCode '{vCodeAndNaamResponse.VCode}' known as '{vCodeAndNaamResponse.VerenigingsNaam}'");
        var response = await _verenigingsRepository.Load<VerenigingMetRechtspersoonlijkheid>(vCode, null);

        return response;
    }
}