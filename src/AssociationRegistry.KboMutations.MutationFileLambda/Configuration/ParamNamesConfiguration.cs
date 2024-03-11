using AssocationRegistry.KboMutations.Configuration;

namespace AssociationRegistry.KboMutations.MutationFileLambda.Configuration;

public class ParamNamesConfiguration: ISlackConfiguration
{
    public static string Section = "ParamNames";
    
    public string SlackWebhook { get; set; } = null;
}