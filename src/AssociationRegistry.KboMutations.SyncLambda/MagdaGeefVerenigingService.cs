using AssociationRegistry.Framework;
using AssociationRegistry.Kbo;
using AssociationRegistry.Vereniging;
using ResultNet;

namespace AssociationRegistry.KboMutations.SyncLambda;

internal class MagdaGeefVerenigingService : IMagdaGeefVerenigingService
{
    public async Task<Result<VerenigingVolgensKbo>> GeefVereniging(KboNummer kboNummer, CommandMetadata metadata, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}