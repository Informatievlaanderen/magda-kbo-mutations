using Amazon.SimpleSystemsManagement;
using AssociationRegistry.KboMutations.MutationLambdaContainer.Configuration;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public class CertificatesProvider
{
    private readonly ParamNamesConfiguration _paramNamesConfiguration;
    private readonly KboMutationsConfiguration _kboMutationsConfiguration;
    private readonly SsmClientWrapper _ssmClient;

    public CertificatesProvider(AmazonSimpleSystemsManagementClient client,
    ParamNamesConfiguration paramNamesConfiguration,
        KboMutationsConfiguration kboMutationsConfiguration)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        if (kboMutationsConfiguration == null)
            throw new ArgumentNullException(nameof(kboMutationsConfiguration));
        
        if (paramNamesConfiguration == null)
            throw new ArgumentNullException(nameof(paramNamesConfiguration));
        
        if (string.IsNullOrWhiteSpace(paramNamesConfiguration.Cert))
            throw new ArgumentException($"{nameof(paramNamesConfiguration.Cert)} cannot be null or empty");
        
        if (string.IsNullOrWhiteSpace(paramNamesConfiguration.CaCert))
            throw new ArgumentException($"{nameof(paramNamesConfiguration.CaCert)} cannot be null or empty");
        
        if (string.IsNullOrWhiteSpace(paramNamesConfiguration.Key))
            throw new ArgumentException($"{nameof(paramNamesConfiguration.Key)} cannot be null or empty");

        _paramNamesConfiguration = paramNamesConfiguration;
        _kboMutationsConfiguration = kboMutationsConfiguration;
        _ssmClient = new SsmClientWrapper(client);
    }
    
    public async Task WriteCertificatesToFileSystem()
    {
        var key = await _ssmClient.GetParameterAsync(_paramNamesConfiguration.Key);
        var cert = await _ssmClient.GetParameterAsync(_paramNamesConfiguration.Cert);
        var cacert = await _ssmClient.GetParameterAsync(_paramNamesConfiguration.CaCert);

        await File.WriteAllTextAsync(_kboMutationsConfiguration.KeyPath, key);
        await File.WriteAllTextAsync(_kboMutationsConfiguration.CertPath, cert);
        await File.WriteAllTextAsync(_kboMutationsConfiguration.CaCertPath, cacert);
    }
}