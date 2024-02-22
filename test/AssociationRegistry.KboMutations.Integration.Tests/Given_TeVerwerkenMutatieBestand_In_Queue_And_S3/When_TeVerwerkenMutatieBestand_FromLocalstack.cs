using AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;
using AssociationRegistry.KboMutations.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3;

public class When_TeVerwerkenMutatieBestand_FromLocalstack : IClassFixture<With_TeVerwerkenMutatieBestand_FromLocalstack>
{
    private readonly With_TeVerwerkenMutatieBestand_FromLocalstack _fixture;

    public When_TeVerwerkenMutatieBestand_FromLocalstack(With_TeVerwerkenMutatieBestand_FromLocalstack fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SyncQueue_Has_Messages()
    {
        _fixture.ReceivedMessages.Count.Should().Be(2);
    }
}