using AssociationRegistry.KboMutations.SyncLambda.Configuration;

namespace AssociationRegistry.KboMutations.SyncLambda.Extensions;

public static class OptionExtensions
{
    public static string GetConnectionString(this PostgreSqlOptionsSection postgreSqlOptions)
        => $"host={postgreSqlOptions.Host};" +
           $"database={postgreSqlOptions.Database};" +
           $"password={postgreSqlOptions.Password};" +
           $"username={postgreSqlOptions.Username}";

}