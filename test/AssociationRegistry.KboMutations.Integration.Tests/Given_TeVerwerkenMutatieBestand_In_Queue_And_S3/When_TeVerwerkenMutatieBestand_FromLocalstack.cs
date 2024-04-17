using AssociationRegistry.Events;
using AssociationRegistry.EventStore;
using AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3.Fixtures;
using AssociationRegistry.Vereniging;
using FluentAssertions;
using Polly;
using Xunit;
using Xunit.Abstractions;

namespace AssociationRegistry.KboMutations.Integration.Tests.Given_TeVerwerkenMutatieBestand_In_Queue_And_S3;

public class When_TeVerwerkenMutatieBestand_FromLocalstack : IClassFixture<With_TeVerwerkenMutatieBestand_FromLocalstack>
{
    private readonly ITestOutputHelper _helper;
    private readonly With_TeVerwerkenMutatieBestand_FromLocalstack _fixture;

    public When_TeVerwerkenMutatieBestand_FromLocalstack(
        ITestOutputHelper helper,
        With_TeVerwerkenMutatieBestand_FromLocalstack fixture)
    {
        _helper = helper;
        _fixture = fixture;
    }

    [Fact(Skip = "Not yet finished")]
    public async Task SyncQueue_Has_Messages()
    {
        var retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i), (context, ts, i, x) =>
            {
                _helper.WriteLine($"No matching events found in run {i}");
            });

        await retryPolicy.ExecuteAsync(async () => await VerifyKboEventsWereAdded(_helper));
    }

    private static async Task VerifyKboEventsWereAdded(ITestOutputHelper helper)
    {
        var documentStore = With_TeVerwerkenMutatieBestand_FromLocalstack.CreateDocumentStore();
        await using var session = documentStore.LightweightSession();
        var bekendeVerenigingEvents =
            await session.Events.FetchStreamAsync(With_TeVerwerkenMutatieBestand_FromLocalstack
                .KboNummersToSeed[With_TeVerwerkenMutatieBestand_FromLocalstack.KboNummerBekendeVereniging]);
        
        bekendeVerenigingEvents.Should().NotBeNull();
        var actualEvents = bekendeVerenigingEvents.Select(x => x.EventType)
            .ToArray();
        helper.WriteLine($"Actual events: {string.Join(", ", actualEvents.Select(x => x.Name).ToList())}");
        helper.WriteLine($"Actual events: {string.Join(", ", actualEvents.Select(x => x.Name).ToList())}");


        actualEvents
            .Should()
            .ContainInOrder(
                typeof(VerenigingMetRechtspersoonlijkheidWerdGeregistreerd),
                typeof(NaamWerdGewijzigdInKbo),
                typeof(KorteNaamWerdGewijzigdInKbo),
                typeof(SynchronisatieMetKboWasSuccesvol));
    }
}