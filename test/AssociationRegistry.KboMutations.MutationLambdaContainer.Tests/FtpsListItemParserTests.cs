using Xunit;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Abstractions;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;
using FluentAssertions;
using Moq;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Tests;

public class FtpsListItemParserTests
{
    [Fact]
    public void ParsesString()
    {
        var result = $"-rw-rw-r--   1 kbbj.vlaanderen.be-dv-verenigingsregister-ftp kbbj.vlaanderen.be-dv-verenigingsregister-ftp        0 Mar 22 05:41 pub_mut_klanten-functies0200_20240322043924000.csv{Environment.NewLine}-rw-rw-r--   1 kbbj.vlaanderen.be-dv-verenigingsregister-ftp kbbj.vlaanderen.be-dv-verenigingsregister-ftp     2260 Apr 17 05:18 pub_mut-ondernemingVKBO0200_20240417031451000.xml";
      
        var ftpsListItems = FtpsListParser.Parse(new FtpUriBuilder("host", 21), result);

        ftpsListItems.Should().HaveCount(2);
    }
}