using System.ComponentModel.DataAnnotations;

namespace ScrollsTracker.Api.Requests;

public class LeituraRequest
{
    [StringLength(24)]
    [RegularExpression(@"^\d+(?:[.,]\d+)?$", ErrorMessage = "Informe um capítulo válido.")]
    public string? UltimoCapituloLido { get; set; }

    [AllowedValues(null, "Lendo", "Completo", "Pausado", "Planejo Ler", "Abandonado", ErrorMessage = "Status de leitura inválido.")]
    public string? StatusLeitura { get; set; }
}
