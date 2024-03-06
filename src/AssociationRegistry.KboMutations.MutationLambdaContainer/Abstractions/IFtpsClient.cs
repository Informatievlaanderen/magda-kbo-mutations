namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Abstractions;

public interface IFtpsClient
{
    string GetListing(string sourceDirectory);
    bool Download(Stream stream, string ftpSourceFilePath, string localDestinationFilePath);
    void MoveFile(string baseUri, string ftpSourceFilePath, string ftpDestinationFilePath);
}