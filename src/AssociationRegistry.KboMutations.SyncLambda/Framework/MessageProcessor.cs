using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AssocationRegistry.KboMutations.Messages;
using AssociationRegistry.Magda;
using AssociationRegistry.Magda.Models;
using AssociationRegistry.Vereniging;

namespace AssociationRegistry.KboMutations.SyncLambda.Framework;

public interface IMessageProcessor
{
    Task ProcessMessage(SQSEvent sqsEvent, ILambdaLogger contextLogger, CancellationToken cancellationToken);
}

public class MessageProcessor : IMessageProcessor
{
    private readonly IMagdaGeefVerenigingService _magdaGeefVerenigingService;
    private readonly IMagdaCallReferenceRepository _magdaCallReferenceRepository;
    private readonly IVerenigingsRepository _verenigingsRepository;

    public MessageProcessor(IMagdaGeefVerenigingService magdaGeefVerenigingService, IMagdaCallReferenceRepository magdaCallReferenceRepository, IVerenigingsRepository verenigingsRepository)
    {
        _magdaGeefVerenigingService = magdaGeefVerenigingService;
        _magdaCallReferenceRepository = magdaCallReferenceRepository;
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

            await Handle(contextLogger, KboNummer.Create(message.KboNummer!), Guid.NewGuid(), cancellationToken);
        }
    }

    private async Task Handle(ILambdaLogger logger,
        KboNummer kboNummer,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var verenigingFromMagda = await GetVerenigingFromMagda(logger, kboNummer, correlationId, cancellationToken);
        var verenigingFromRepository = await GetVerenigingFromRepository(logger, kboNummer, cancellationToken);

        verenigingFromRepository.WijzigNaamUitKbo(verenigingFromMagda.NaamUitKbo);
        verenigingFromRepository.WijzigKorteNaamUitKbo(verenigingFromMagda.KorteNaamUitKbo);
        verenigingFromRepository.WijzigKorteBeschrijving(verenigingFromMagda.KorteBeschrijving);
        verenigingFromRepository.WijzigRoepnaam(verenigingFromMagda.Roepnaam);

        verenigingFromRepository.WijzigMaatschappelijkeZetel(
            verenigingFromMagda.MaatschappelijkeZetelDetail.LocatieId,
            verenigingFromMagda.MaatschappelijkeZetelDetail.Naam,
            verenigingFromMagda.MaatschappelijkeZetelDetail.IsPrimair
        );

        // verenigingFromRepository.WijzigContactgegeven(contactgegevenId, beschrijving, isPrimair);
        verenigingFromRepository.WijzigDoelgroep(verenigingFromMagda.Doelgroep);
        // verenigingFromRepository.WijzigHoofdactiviteitenVerenigingsloket();
    }

    private async Task<GeefVerenigingDetail> GetVerenigingFromMagda(ILambdaLogger logger, KboNummer kboNummer, Guid correlationId, CancellationToken cancellationToken)
    {
        var callReference = await CreateReference(_magdaCallReferenceRepository, "DV", correlationId, kboNummer, cancellationToken);

        logger.LogInformation($"Calling {nameof(IMagdaGeefVerenigingService)}.{nameof(IMagdaGeefVerenigingService.GeefVerenigingDetail)} for KBO '{kboNummer}'");
        return await _magdaGeefVerenigingService.GeefVerenigingDetail(kboNummer, callReference);

        static async Task<MagdaCallReference> CreateReference(
            IMagdaCallReferenceRepository repository,
            string initiator,
            Guid correlationId,
            string opgevraagdOnderwerp,
            CancellationToken cancellationToken)
        {
            var magdaCallReference = new MagdaCallReference
            {
                Reference = Guid.NewGuid(),
                CalledAt = DateTimeOffset.UtcNow,
                Initiator = initiator,
                OpgevraagdeDienst = "GeefOndernemingDienst-02.00",
                Context = "Registreer vereniging met rechtspersoonlijkheid",
                AanroependeDienst = "Verenigingsregister Beheer Api",
                CorrelationId = correlationId,
                OpgevraagdOnderwerp = opgevraagdOnderwerp,
            };

            await repository.Save(magdaCallReference, cancellationToken);
            return magdaCallReference;
        }
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