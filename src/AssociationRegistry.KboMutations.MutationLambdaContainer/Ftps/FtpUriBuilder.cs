namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Ftps;

public class FtpUriBuilder : UriBuilder
{
    private const string FtpScheme = "ftp";

    public FtpUriBuilder(string host, int port)
        : this(FtpScheme, host, port)
    {
    }

    private FtpUriBuilder(string scheme, string host, int port)
        : base(scheme, host, port)
    {
    }

    public string FileName => Uri.Segments.Last();

    public FtpUriBuilder AppendDir(string sourcePath)
    {
        return new FtpUriBuilder(
            Scheme,
            Host,
            Port)
        {
            Path = $"{Path}{sourcePath.Trim('/')}/"
        };
    }

    public FtpUriBuilder WithPath(string fullPath)
    {
        return new FtpUriBuilder(
            Scheme,
            Host,
            Port)
        {
            Path = fullPath
        };
    }

    public FtpUriBuilder AppendFileName(string fileName)
    {
        return new FtpUriBuilder(
            Scheme,
            Host,
            Port)
        {
            Path = $"{Path}{(Path.EndsWith('/') ? fileName.TrimStart('/') : fileName)}"
        };
    }
}