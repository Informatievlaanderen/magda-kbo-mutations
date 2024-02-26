using AssociationRegistry.Framework;
using AssociationRegistry.Kbo;
using AssociationRegistry.Magda;
using AssociationRegistry.Magda.Configuration;
using AssociationRegistry.Magda.Constants;
using AssociationRegistry.Magda.Exceptions;
using AssociationRegistry.Magda.Extensions;
using AssociationRegistry.Magda.Models;
using AssociationRegistry.Vereniging;
using Microsoft.Extensions.Logging;
using ResultNet;

namespace AssociationRegistry.KboMutations.SyncLambda.Framework;

public class MagdaGeefVerenigingService : Magda.MagdaGeefVerenigingService, IMagdaGeefVerenigingService
{
    public MagdaGeefVerenigingService(
        IMagdaCallReferenceRepository magdaCallReferenceRepository,
        IMagdaFacade magdaFacade,
        TemporaryMagdaVertegenwoordigersSection temporaryMagdaVertegenwoordigersSection,
        ILogger<MagdaGeefVerenigingService> logger)
        : base(magdaCallReferenceRepository, magdaFacade, temporaryMagdaVertegenwoordigersSection, logger)
    {
    }

    public async Task<GeefVerenigingDetail> GeefVerenigingDetail(KboNummer kboNummer, MagdaCallReference callReference)
    {
        try
        {
            var magdaDetail = new GeefVerenigingDetail();
            
            var magdaResponse = await _magdaFacade.GeefOnderneming(kboNummer, callReference);

            if (MagdaResponseValidator.HasBlokkerendeUitzonderingen(magdaResponse))
                return magdaDetail with { Result = HandleUitzonderingen(kboNummer, magdaResponse) };

            magdaDetail = magdaDetail with
            {
                OndernemingType = magdaResponse?.Body?.GeefOndernemingResponse?.Repliek.Antwoorden.Antwoord.Inhoud.Onderneming ?? null,
            };
            
            if (magdaDetail.OndernemingType is null ||
                !HeeftToegestaneActieveRechtsvorm(magdaDetail.OndernemingType) ||
                !IsOnderneming(magdaDetail.OndernemingType) ||
                !IsActiefOfInOprichting(magdaDetail.OndernemingType) ||
                !IsRechtspersoon(magdaDetail.OndernemingType))
                return magdaDetail with { Result = VerenigingVolgensKboResult.GeenGeldigeVereniging };

            magdaDetail = magdaDetail with
            {
                MaatschappelijkeNamen = GetBestMatchingNaam(magdaDetail.OndernemingType.Namen.MaatschappelijkeNamen),
                CommercieleNamen = GetBestMatchingNaam(magdaDetail.OndernemingType.Namen.CommercieleNamen)
            };

            if (magdaDetail.MaatschappelijkeNamen is null)
                return magdaDetail with { Result = VerenigingVolgensKboResult.GeenGeldigeVereniging };

            magdaDetail = magdaDetail with
            {
                MaatschappelijkeZetel = magdaDetail.OndernemingType.Adressen.SingleOrDefault(a => a.Type.Code.Value == AdresCodes.MaatschappelijkeZetel && IsActiveToday(a.DatumBegin, a.DatumEinde))
                // MaatschappelijkeZetelDetail = new MaatschappelijkeZetelDetail
                // {
                //     LocatieId = 0,
                //     Naam = "",
                //     true
                // }
            };
            
            return magdaDetail with {
                Result = VerenigingVolgensKboResult.GeldigeVereniging(
                    new VerenigingVolgensKbo
                    {
                        KboNummer = KboNummer.Create(kboNummer),
                        Type = rechtsvormMap[GetActiveRechtsvorm(magdaDetail.OndernemingType)!.Code.Value],
                        Naam = magdaDetail.MaatschappelijkeNamen.Naam,
                        KorteNaam = GetBestMatchingNaam(magdaDetail.OndernemingType.Namen.AfgekorteNamen)?.Naam,
                        Startdatum = DateOnlyHelper.ParseOrNull(magdaDetail.OndernemingType.Start.Datum, Formats.DateOnly),
                        Adres = GetAdresFrom(magdaDetail.MaatschappelijkeZetel),
                        Contactgegevens = GetContactgegevensFrom(magdaDetail.MaatschappelijkeZetel),
                        Vertegenwoordigers = GetVertegenwoordigers(),
                    })
            };
        }
        catch (Exception e)
        {
            throw new MagdaException(message: "Er heeft zich een fout voorgedaan bij het aanroepen van de Magda GeefOndernemingDienst.", e);
        }
    }

    public override async Task<Result<VerenigingVolgensKbo>> GeefVereniging(
        KboNummer kboNummer,
        CommandMetadata metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            var callReference = await CreateReference(_magdaCallReferenceRepository, metadata.Initiator, metadata.CorrelationId, kboNummer, cancellationToken);
            var magdaResponse = await GeefVerenigingDetail(kboNummer, callReference);

            return magdaResponse.Result;
        }
        catch (Exception e)
        {
            throw new MagdaException(message: "Er heeft zich een fout voorgedaan bij het aanroepen van de Magda GeefOndernemingDienst.", e);
        }
    }
}