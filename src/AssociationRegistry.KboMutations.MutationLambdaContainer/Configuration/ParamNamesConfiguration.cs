namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;

public class ParamNamesConfiguration
{
    public static string Section = "ParamNames";

    public DateTime Created => DateTime.Now;

    public string Cert { get; set; } = null!;
    public string CaCert { get; set; } = null!;
    public string Key { get; set; } = null!;
}