using AssociationRegistry.Kbo;
using AssociationRegistry.Magda.Onderneming.GeefOnderneming;
using AssociationRegistry.Vereniging;
using ResultNet;

namespace AssociationRegistry.KboMutations.SyncLambda;

public record struct GeefVerenigingDetail
{
    public VerenigingsNaam NaamUitKbo { get; set; }
    public VerenigingsNaam KorteNaamUitKbo { get; set; }
    public string KorteBeschrijving { get; set; }
    public VerenigingsNaam Roepnaam { get; set; }
    
    public Doelgroep Doelgroep { get; set; }
    
    public Result<VerenigingVolgensKbo> Result { get; set; }
    public Onderneming2_0Type OndernemingType { get; set; }
    public NaamOndernemingType MaatschappelijkeNamen { get; set; }
    public AdresOndernemingType? MaatschappelijkeZetel { get; set; }
    public MaatschappelijkeZetelDetail MaatschappelijkeZetelDetail { get; set; }
    public NaamOndernemingType CommercieleNamen { get; set; }
}

public class MaatschappelijkeZetelDetail
{
    public int LocatieId { get; set; }
    public string Naam { get; set; }
    public bool? IsPrimair { get; set; }
}