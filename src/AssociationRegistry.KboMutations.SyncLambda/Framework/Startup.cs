using AssociationRegistry.EventStore;
using AssociationRegistry.KboMutations.SyncLambda.Configuration;
using AssociationRegistry.KboMutations.SyncLambda.Extensions;
using AssociationRegistry.Magda;
using AssociationRegistry.Magda.Configuration;
using AssociationRegistry.Vereniging;
using Marten;
using Marten.Events;
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IEventStore = AssociationRegistry.EventStore.IEventStore;

namespace AssociationRegistry.KboMutations.SyncLambda.Framework;

public class Startup
{
    private IServiceCollection _serviceCollection;
    public IConfiguration Configuration { get; private set; }
    
    public Startup Configure(IConfiguration configuration)
    {
        Configuration = configuration;
        return this;
    }

    public Startup ConfigureServices(IServiceCollection services)
    {
        PostgreSqlOptionsSection postgresqlOptions = Configuration.GetSection(PostgreSqlOptionsSection.Name).Get<PostgreSqlOptionsSection>();
        MagdaOptionsSection magdaOptions = Configuration.GetSection(PostgreSqlOptionsSection.Name).Get<MagdaOptionsSection>();
        TemporaryMagdaVertegenwoordigersSection magdaTemporaryVertegenwoordigersOptions = Configuration.GetSection(TemporaryMagdaVertegenwoordigersSection.SectionName).Get<TemporaryMagdaVertegenwoordigersSection>();
        StoreOptions storeOptions = new StoreOptions();
        storeOptions.Connection(postgresqlOptions.GetConnectionString());
        storeOptions.Events.StreamIdentity = StreamIdentity.AsString;
        storeOptions.Serializer(CreateCustomMartenSerializer());
        storeOptions.Events.MetadataConfig.EnableAll();

        services
            .AddLogging()
            .AddSingleton(postgresqlOptions)
            .AddSingleton(magdaOptions)
            .AddSingleton(storeOptions)
            .AddSingleton<IDocumentStore>(sp => new DocumentStore(sp.GetRequiredService<StoreOptions>()))
            .AddSingleton<IEventStore>(sp => new EventStore.EventStore(sp.GetRequiredService<IDocumentStore>()))
            .AddTransient<IMagdaCallReferenceRepository>(sp => new MagdaCallReferenceRepository(sp.GetRequiredService<IDocumentStore>().LightweightSession()))
            .AddSingleton<IMagdaFacade>(sp => new MagdaFacade(magdaOptions, sp.GetRequiredService<ILogger<MagdaFacade>>()))
            .AddSingleton<IMagdaGeefVerenigingService>(sp => new MagdaGeefVerenigingService(
                sp.GetRequiredService<IMagdaCallReferenceRepository>(),
                sp.GetRequiredService<IMagdaFacade>(),
                magdaTemporaryVertegenwoordigersOptions,
                sp.GetRequiredService<ILogger<MagdaGeefVerenigingService>>()))
            .AddSingleton<IVerenigingsRepository>(sp => new VerenigingsRepository(sp.GetRequiredService<IEventStore>()))
            .AddSingleton<IMessageProcessor>(sp => new MessageProcessor(
                sp.GetRequiredService<IMagdaGeefVerenigingService>(),
                sp.GetRequiredService<IMagdaCallReferenceRepository>(),
                sp.GetRequiredService<IVerenigingsRepository>())
            );

        services.AddMarten(configure => storeOptions);
        
        ISerializer CreateCustomMartenSerializer()
        {
            var jsonNetSerializer = new JsonNetSerializer();
            jsonNetSerializer.Customize(
                s =>
                {
                    s.DateParseHandling = DateParseHandling.None;
                    // s.Converters.Add(new NullableDateOnlyJsonConvertor(WellknownFormats.DateOnly));
                    // s.Converters.Add(new DateOnlyJsonConvertor(WellknownFormats.DateOnly));
                });
            return jsonNetSerializer;
        }

        return this;
    }

    public IServiceProvider BuildServiceProvider() => _serviceCollection.BuildServiceProvider();
}