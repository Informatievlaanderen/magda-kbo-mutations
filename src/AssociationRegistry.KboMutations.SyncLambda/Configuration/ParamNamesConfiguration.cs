namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;

public class ParamNamesConfiguration
{
    public static string Section = "ParamNames";

    public DateTime Created => DateTime.Now;
    public string MagdaCertificate { get; set; } = null!;
    public string MagdaCertificatePassword { get; set; } = null!;
    public string PostgresPassword { get; set; } = null!;
}