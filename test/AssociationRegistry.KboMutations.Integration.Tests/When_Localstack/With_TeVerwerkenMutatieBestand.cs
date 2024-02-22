using FluentAssertions;
using Xunit;

namespace AssociationRegistry.KboMutations.Integration.Tests.When_Localstack;

public class With_TeVerwerkenMutatieBestand : IClassFixture<Fixtures.With_TeVerwerkenMutatieBestand_Fixture>
{
    private readonly Fixtures.With_TeVerwerkenMutatieBestand_Fixture _fixture;

    public With_TeVerwerkenMutatieBestand(Fixtures.With_TeVerwerkenMutatieBestand_Fixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SyncQueue_Has_Messages()
    {
        _fixture.ReceivedMessages.Count.Should().Be(2);
    }
}