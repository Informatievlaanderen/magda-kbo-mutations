using AssocationRegistry.KboMutations.Notifications;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer;

public readonly record struct KboMutationLambdaGestart : IMessage
{
    public string Value => "KBO mutation lambda gestart.";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationLambdaBestandenOpgehaald : IMessage
{
    private readonly int _aantalBestanden;

    public KboMutationLambdaBestandenOpgehaald(int aantalBestanden)
    {
        _aantalBestanden = aantalBestanden;
    }

    public string Value => $"KBO mutation lambda heeft {_aantalBestanden} opgehaald.";
    public NotifyType Type => NotifyType.None;
}

public readonly record struct KboMutationLambdaGefaald : IMessage
{
    private readonly Exception _exception;

    public KboMutationLambdaGefaald(Exception exception)
    {
        _exception = exception;
    }
    public string Value => $"KBO mutation lambda gefaald. {_exception.Message}";
    public NotifyType Type => NotifyType.Failure;
}

public readonly record struct KboMutationLambdaVoltooid : IMessage
{
    public string Value => "KBO mutation lambda voltooid.";
    public NotifyType Type => NotifyType.Success;
}

public readonly record struct KboMutationLambdaKonBestandNietVerwerken : IMessage
{
    private readonly string _fileName;
    private readonly Exception _exception;

    public KboMutationLambdaKonBestandNietVerwerken(string fileName, Exception exception)
    {
        _fileName = fileName;
        _exception = exception;
    }
    public string Value => $"KBO mutation lambda kon een bestand niet verwerken. Bestandsnaam: '{_fileName}' ({_exception.Message})";
    public NotifyType Type => NotifyType.Failure;
}
