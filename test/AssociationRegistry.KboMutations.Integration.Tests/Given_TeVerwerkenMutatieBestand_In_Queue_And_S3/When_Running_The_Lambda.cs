using System.Text.Json;
using AssocationRegistry.KboMutations.Messages;
using AssociationRegistry.Kbo;
using AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;
using FluentAssertions;
using Xunit;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3;

public class When_Running_The_Lambda : IClassFixture<With_TeVerwerkenMutatieBestand_In_Queue_And_S3_Fixture>
{
    private readonly With_TeVerwerkenMutatieBestand_In_Queue_And_S3_Fixture _fixture;

    public When_Running_The_Lambda(With_TeVerwerkenMutatieBestand_In_Queue_And_S3_Fixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(Skip = "Not yet finished")]
    public async Task It_Splits_The_TeVerwerkenMutatieBestandMessage_In_TeVerwerkenKboNummer_Messages()
    {

        _fixture.ReceivedMessages.Should().HaveCount(_fixture.MutatieLijnen.Length);
    }
    
    [Fact(Skip = "Not yet finished")]
    public async Task It_Sends_TeSynchroniserenKboNummerMessage_For_Each_MutatieLijn()
    {
        var expectedMutationFiles = _fixture.MutatieLijnen
            .Select(x => new TeSynchroniserenKboNummerMessage(
                x.Ondernemingsnummer))
            .ToList();

        _fixture.ReceivedMessages.Select(x => JsonSerializer.Deserialize<TeSynchroniserenKboNummerMessage>(x.Body))
            .Should().BeEquivalentTo(expectedMutationFiles);
    }
}