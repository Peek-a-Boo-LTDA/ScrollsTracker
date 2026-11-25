namespace ScrollsTracker.Api.Requests;

public class ObraResponse : ObraRequest
{
    public int Id { get; set; }
    public DateTime? UltimoCapituloDetectadoEm { get; set; }
    public LancamentoResponse? LancamentoRecente { get; set; }
}

public sealed record LancamentoResponse(string Numero, string Fonte, DateTime DetectadoEm);
