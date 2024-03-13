using System.Text;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Logging;

public class ProgramInformation
{
    public static string Build(
        KboMutationsConfiguration kboMutationsConfiguration)
    {
        var progInfo = new StringBuilder();
        progInfo.AppendLine();
        progInfo.AppendLine("Application settings:");
        progInfo.AppendLine(new string('-', 50));
        progInfo.AppendLine(new string('-', 50));
        progInfo.AppendLine();
        return progInfo.ToString();
    }

    private static void AppendKeyValue(StringBuilder progInfo, string key, string value)
    {
        progInfo.Append(key);
        progInfo.Append(": \t");
        progInfo.AppendLine(value);
    }
}