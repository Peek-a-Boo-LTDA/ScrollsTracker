using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;

namespace ScrollsTracker.Application.Services.Filter;

internal static class ObraFilter
{
	private static readonly EnumSources[] Catalogo = [EnumSources.MangaUpdate, EnumSources.Kitsu, EnumSources.MangaDex];
	private static readonly EnumSources[] Capas = [.. Catalogo, EnumSources.MangaKatana, EnumSources.MangaHere];
	private static readonly EnumSources[] FontesDeCapitulos =
		[EnumSources.MangaDex, EnumSources.MangaKatana, EnumSources.MangaHere, EnumSources.MangaUpdate];

	public static LancamentoCapitulo? Aplicar(Obra obra, IReadOnlyList<SearchResult> resultados)
	{
		if (!obra.FixarDescricao && EscolherCampo(resultados, Catalogo, item => item.Descricao) is { } descricao)
			(obra.Descricao, obra.FonteDescricao) = (descricao.Valor, descricao.Fonte);
		if (!obra.FixarStatus && EscolherCampo(resultados, Catalogo, item => NormalizarStatus(item.Status)) is { } status)
			(obra.Status, obra.FonteStatus) = (status.Valor, status.Fonte);
		if (!obra.FixarImagem && EscolherCampo(resultados, Capas, item => item.Imagem) is { } imagem)
			(obra.Imagem, obra.FonteImagem) = (imagem.Valor, imagem.Fonte);

		foreach (var resultado in resultados)
		{
			foreach (var vinculo in resultado.Obra.VinculosExternos)
				obra.VinculosExternos.TryAdd(vinculo.Key, vinculo.Value);
		}

		var capitulo = resultados
			.Where(item => FontesDeCapitulos.Contains(item.Source))
			.Select(item => (Numero: Obra.InterpretarCapitulo(item.Obra.TotalCapitulos), Fonte: item.Source))
			.Where(item => item.Numero.HasValue)
			.OrderByDescending(item => item.Numero)
			.ThenBy(item => Array.IndexOf(FontesDeCapitulos, item.Fonte))
			.FirstOrDefault();

		var anterior = Obra.InterpretarCapitulo(obra.TotalCapitulos);
		if (capitulo.Numero is not decimal numero || (anterior.HasValue && numero < anterior.Value))
			return null;

		var agora = DateTime.UtcNow;
		if (anterior != numero || obra.FonteUltimoCapitulo is null)
		{
			obra.TotalCapitulos = Obra.FormatarCapitulo(numero);
			obra.FonteUltimoCapitulo = capitulo.Fonte;
			obra.UltimoCapituloDetectadoEm = agora;
		}

		if (numero > 0 && anterior != numero)
			obra.DataAtualizacao = agora.ToLocalTime();

		return anterior is > 0 && numero > anterior.Value
			? new LancamentoCapitulo { ObraId = obra.Id, Numero = obra.TotalCapitulos!, Fonte = capitulo.Fonte, DetectadoEm = agora }
			: null;
	}

	public static IEnumerable<string> Nomes(Obra obra) =>
		obra.TitulosAlternativos.Prepend(obra.Titulo ?? "");

	public static IEnumerable<string> NomesNormalizados(Obra obra) =>
		Nomes(obra).Select(StringUtils.NormalizarTitulo).Where(nome => nome.Length > 0);

	private static (string Valor, EnumSources Fonte)? EscolherCampo(IReadOnlyList<SearchResult> resultados,
		IEnumerable<EnumSources> prioridade, Func<Obra, string?> campo)
	{
		foreach (var fonte in prioridade)
		{
			var resultado = resultados.FirstOrDefault(item => item.Source == fonte);
			if (resultado is not null && campo(resultado.Obra) is { } valor && !string.IsNullOrWhiteSpace(valor))
				return (valor, fonte);
		}

		return null;
	}

	private static string? NormalizarStatus(string? status)
	{
		var texto = status?.Trim().ToLowerInvariant();
		return texto switch
		{
			null or "" or "unknown" or "tba" => null,
			"current" or "publishing" => "ongoing",
			"finished" or "complete" => "completed",
			"unreleased" or "upcoming" => "upcoming",
			_ when texto.Contains("hiatus") => "hiatus",
			_ when texto.Contains("cancel") => "cancelled",
			_ when texto.Contains("completed") && !texto.Contains("ongoing") => "completed",
			_ when texto.Contains("ongoing") => "ongoing",
			_ => status?.Trim()
		};
	}
}
