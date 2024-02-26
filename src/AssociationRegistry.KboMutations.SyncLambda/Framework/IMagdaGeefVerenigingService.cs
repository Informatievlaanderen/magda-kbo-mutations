using AssociationRegistry.Magda.Models;
using AssociationRegistry.Vereniging;

namespace AssociationRegistry.KboMutations.SyncLambda.Framework;

public interface IMagdaGeefVerenigingService : AssociationRegistry.Kbo.IMagdaGeefVerenigingService
{
    Task<GeefVerenigingDetail> GeefVerenigingDetail(KboNummer kboNummer, MagdaCallReference callReference);
}