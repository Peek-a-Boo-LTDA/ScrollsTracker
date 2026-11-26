namespace ScrollsTracker.Infra.Repository.Context;

public class UsuarioObra
{
    public required string UsuarioId { get; set; }
    public int ObraId { get; set; }
    public string? UltimoCapituloLido { get; set; }
    public string? StatusLeitura { get; set; }
    public bool Seguindo { get; set; }
    public DateTime? LeituraAtualizadaEm { get; set; }
}
