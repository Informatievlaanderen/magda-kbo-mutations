using AssocationRegistry.KboMutations.Models;
using AutoBogus;

namespace AssociationRegistry.KboMutations.Tests.Fakers;

public sealed class MagdaMutatieBestandFaker : AutoFaker<MagdaMutatieBestand>
{
    public MagdaMutatieBestandFaker()
    {
        RuleFor(p => p.FtpPath, f => string.Empty);
        RuleFor(p => p.Name, f => f.Random.Guid().ToString("N"));
    }
}
