namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;

public class ParamNamesConfiguration
{
    public static string Section = "ParamNames";

    public DateTime Created => DateTime.Now;
    
    public string SlackWebhook { get; set; } = null!;
}