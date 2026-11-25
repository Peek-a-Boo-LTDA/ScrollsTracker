using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ScrollsTracker.Domain.Utils;

public static partial class StringUtils
{
	public static string? ExtrairCapitulo(string? texto)
	{
		var numero = NumeroCapitulo().Match(texto ?? "").Groups[1].Value;
		return numero.Length == 0 ? null : numero.Replace(',', '.');
	}

	public static string NormalizarTitulo(string? titulo)
	{
		var normalizado = new StringBuilder();
		foreach (var letra in (titulo ?? "").Normalize(NormalizationForm.FormD).EnumerateRunes())
		{
			if (Rune.GetUnicodeCategory(letra) == UnicodeCategory.NonSpacingMark)
				continue;

			normalizado.Append(Rune.IsLetterOrDigit(letra) ? Rune.ToUpperInvariant(letra).ToString() : " ");
		}

		return string.Join(' ', normalizado.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
	}

	public static List<string> LimparTitulosAlternativos(string? titulo, IEnumerable<string>? aliases)
	{
		var usados = new HashSet<string>(StringComparer.Ordinal) { "", NormalizarTitulo(titulo) };
		return (aliases ?? []).Where(alias => !string.IsNullOrWhiteSpace(alias))
			.Select(alias => alias.Trim())
			.Where(alias => alias.Length <= 300 && usados.Add(NormalizarTitulo(alias)))
			.Take(50).ToList();
	}

	[GeneratedRegex(@"^\s*(?:(?:ch(?:apter)?|cap(?:ítulo)?)\.?\s*)?(\d+(?:[.,]\d+)?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex NumeroCapitulo();
}
