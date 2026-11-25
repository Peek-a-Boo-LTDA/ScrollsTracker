using FuzzySharp;
using Microsoft.Extensions.Logging;
using ScrollsTracker.Application.Services.Filter;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Utils;

namespace ScrollsTracker.Application.Services;

public class ObraAggregatorService(IEnumerable<IObraSource> fontes, ILogger<ObraAggregatorService> logger,
	CacheCandidatos? cacheCandidatos = null) : IObraAggregatorService
{
	private readonly IObraSource[] _fontes = fontes.ToArray();

	public async Task<Obra> BuscarObraAgregadaAsync(string titulo)
	{
		var resultado = await BuscarCandidatosAsync(titulo, [], CancellationToken.None);
		return resultado.Obra ?? new Obra { Titulo = titulo };
	}

	public async Task<BuscaExternaResult> BuscarCandidatosAsync(
		string titulo, IReadOnlyList<string> titulosAlternativos, CancellationToken cancellationToken,
		string? usuarioParaSelecionar = null)
	{
		var nomes = StringUtils.LimparTitulosAlternativos(titulo, titulosAlternativos).Prepend(titulo.Trim()).ToArray();
		var resultados = await BuscarFontesAsync(_fontes, nomes, cancellationToken);
		var grupos = Agrupar(resultados, nomes);
		var exatos = grupos.Where(grupo => grupo.Pontuacao == 100).ToList();

		if (exatos.Count == 1)
			return new BuscaExternaResult((await MontarCandidatoAsync(exatos[0].Resultados, cancellationToken)).Obra, []);

		var opcoes = (exatos.Count > 0 ? exatos : grupos).Take(5);
		if (usuarioParaSelecionar != null && cacheCandidatos != null)
		{
			var resumos = opcoes.Select(grupo =>
			{
				var candidato = MontarCandidato(grupo.Resultados, grupo.Resultados);
				return candidato with
				{
					Obra = new Obra { Titulo = candidato.Obra.Titulo, Imagem = candidato.Obra.Imagem },
					Chave = cacheCandidatos.Guardar(usuarioParaSelecionar, grupo.Resultados)
				};
			}).ToList();
			return new BuscaExternaResult(null, resumos);
		}

		var candidatos = await Task.WhenAll(opcoes.Select(grupo => MontarCandidatoAsync(grupo.Resultados, cancellationToken)));
		return new BuscaExternaResult(null, candidatos);
	}

	public async Task<Obra?> SelecionarCandidatoAsync(Guid chave, string usuarioId, CancellationToken cancellationToken)
	{
		var resultados = cacheCandidatos?.Obter(chave, usuarioId);
		return resultados is { Length: > 0 } ? (await MontarCandidatoAsync(resultados, cancellationToken)).Obra : null;
	}

	public async Task<AtualizacaoResult> BuscarEAtualizaObraAsync(Obra obra, CancellationToken cancellationToken = default)
	{
		var nomes = ObraFilter.Nomes(obra).Where(nome => !string.IsNullOrWhiteSpace(nome)).ToArray();
		var associados = await Task.WhenAll(_fontes
			.Where(fonte => obra.VinculosExternos.ContainsKey(fonte.SourceName))
			.Select(fonte => ConsultarVinculoAsync(fonte, obra, cancellationToken)));
		var confirmados = associados.OfType<SearchResult>().ToList();

		var disponiveis = _fontes.Where(fonte => !obra.VinculosExternos.ContainsKey(fonte.SourceName));
		var descobertos = await BuscarFontesAsync(disponiveis, nomes, cancellationToken);
		var grupos = Agrupar(descobertos, nomes);

		if (confirmados.Count > 0)
		{
			var compativeis = grupos.Where(grupo => grupo.Resultados.All(candidato =>
				confirmados.Any(confirmado => MesmaObra(confirmado, candidato))
				&& confirmados.All(confirmado => !IdsConflitam(confirmado, candidato)))).ToList();
			if (compativeis.Count == 1)
				confirmados.AddRange(await CompletarGrupoAsync(compativeis[0].Resultados, cancellationToken));
		}
		else if (obra.VinculosExternos.Count == 0)
		{
			var exatos = grupos.Where(grupo => grupo.Pontuacao == 100).ToList();
			if (exatos.Count == 1)
				confirmados.AddRange(await CompletarGrupoAsync(exatos[0].Resultados, cancellationToken));
			else if (grupos.Count > 0)
				logger.LogWarning("A atualização de {TituloBuscado} precisa de uma correspondência confirmada.", obra.Titulo);
		}
		else
		{
			// Um vínculo indisponível não impede uma leitura exata por uma fonte de fallback.
			var exatos = grupos.Where(grupo => grupo.Pontuacao == 100).ToList();
			if (exatos.Count == 1)
			{
				confirmados.AddRange(exatos[0].Resultados
					.Where(item => item.Source is EnumSources.MangaKatana or EnumSources.MangaHere));
			}
		}

		var novoCapitulo = ObraFilter.Aplicar(obra, confirmados);
		obra.DataVerificacao = DateTime.Now;
		return new AtualizacaoResult(obra, novoCapitulo);
	}

	private async Task<CandidatoObra> MontarCandidatoAsync(IReadOnlyList<SearchResult> grupo, CancellationToken cancellationToken)
	{
		var resultados = await CompletarGrupoAsync(grupo, cancellationToken);
		return MontarCandidato(grupo, resultados);
	}

	private static CandidatoObra MontarCandidato(IReadOnlyList<SearchResult> grupo, IReadOnlyList<SearchResult> resultados)
	{
		var obra = new Obra { Titulo = grupo[0].Obra.Titulo };
		obra.TitulosAlternativos = StringUtils.LimparTitulosAlternativos(obra.Titulo,
			resultados.SelectMany(item => ObraFilter.Nomes(item.Obra)));
		ObraFilter.Aplicar(obra, resultados);

		var formato = resultados.Select(item => item.Formato)
			.FirstOrDefault(tipo => !string.IsNullOrWhiteSpace(tipo) && tipo != "Quadrinhos");
		return new CandidatoObra(obra, resultados.Select(item => item.Ano).FirstOrDefault(ano => ano.HasValue), formato,
			resultados.Select(item => item.Source == EnumSources.MangaUpdate ? "MangaUpdates" : item.Source.ToString()).ToArray());
	}

	private async Task<SearchResult[]> BuscarFontesAsync(IEnumerable<IObraSource> fontes,
		string[] nomes, CancellationToken cancellationToken)
	{
		var resultados = await Task.WhenAll(fontes.Select(fonte => BuscarFonteAsync(fonte, nomes, cancellationToken)));
		return resultados.SelectMany(lista => lista).ToArray();
	}

	private async Task<IReadOnlyList<SearchResult>> BuscarFonteAsync(IObraSource fonte,
		string[] nomes, CancellationToken cancellationToken)
	{
		if (nomes.Length == 0)
			return [];

		var candidatos = new List<SearchResult>();
		try
		{
			using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			prazo.CancelAfter(TimeSpan.FromSeconds(12));
			foreach (var nome in nomes)
			{
				candidatos.AddRange(await fonte.BuscarAsync(nome, prazo.Token));
				if (candidatos.Any(item => Pontuar(item, [nome]) == 100))
					break;
			}

			if (candidatos.Count == 0)
				logger.LogWarning("A fonte {Fonte} não retornou dados utilizáveis para {TituloBuscado}.", fonte.SourceName, nomes[0]);
			else
			{
				var melhor = candidatos.MaxBy(item => Pontuar(item, nomes))!;
				logger.LogInformation("A fonte {Fonte} retornou {Quantidade} resultados para {TituloBuscado}. Melhor opção: {TituloEncontrado} ({Pontuacao}).",
					fonte.SourceName, candidatos.Count, nomes[0], melhor.Obra.Titulo, Pontuar(melhor, nomes));
			}

			return candidatos;
		}
		catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Falha ao consultar a fonte {Fonte} para {TituloBuscado}.", fonte.SourceName, nomes[0]);
			return candidatos;
		}
	}

	private async Task<SearchResult?> ConsultarVinculoAsync(IObraSource fonte, Obra obra, CancellationToken cancellationToken)
	{
		var id = obra.VinculosExternos[fonte.SourceName];
		try
		{
			var resultado = await fonte.ObterPorIdAsync(id, cancellationToken);
			if (resultado is not null)
			{
				logger.LogInformation("A fonte {Fonte} retornou {TituloEncontrado} pelo vínculo {IdExterno}.",
					fonte.SourceName, resultado.Obra.Titulo, id);
				return await CompletarAsync(fonte, resultado, cancellationToken);
			}

			logger.LogWarning("A fonte {Fonte} não retornou dados para o vínculo {IdExterno} de {TituloBuscado}.",
				fonte.SourceName, id, obra.Titulo);
		}
		catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Falha ao consultar o vínculo {IdExterno} da fonte {Fonte} para {TituloBuscado}.",
				id, fonte.SourceName, obra.Titulo);
		}

		return null;
	}

	private Task<SearchResult[]> CompletarGrupoAsync(IEnumerable<SearchResult> grupo, CancellationToken cancellationToken) =>
		Task.WhenAll(grupo.Select(resultado => CompletarAsync(
			_fontes.First(fonte => fonte.SourceName == resultado.Source), resultado, cancellationToken)));

	private async Task<SearchResult> CompletarAsync(IObraSource fonte, SearchResult resultado, CancellationToken cancellationToken)
	{
		if (!resultado.RequerDetalhes)
			return resultado;

		try
		{
			return await fonte.CompletarAsync(resultado, cancellationToken);
		}
		catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Falha ao completar dados da fonte {Fonte} para {TituloEncontrado}.",
				fonte.SourceName, resultado.Obra.Titulo);
			return resultado;
		}
	}

	private static List<Grupo> Agrupar(IEnumerable<SearchResult> resultados, string[] nomes)
	{
		var grupos = new List<Grupo>();
		var candidatos = resultados.ToList();
		var ordenados = candidatos.Select(item => (Item: item, Pontuacao: Pontuar(item, nomes)))
			.Where(item => item.Pontuacao > 40 || candidatos.Any(outro => ReferenciaConfirmada(item.Item, outro)))
			.OrderByDescending(item => item.Pontuacao).ThenBy(item => item.Item.Source);

		foreach (var (item, pontuacao) in ordenados)
		{
			var correspondencias = grupos.Where(grupo =>
				grupo.Resultados.Any(outro => MesmaObra(outro, item))
				&& grupo.Resultados.All(outro => !IdsConflitam(outro, item)
					&& (MetadadosCompativeis(outro, item) || ReferenciaConfirmada(outro, item))))
				.ToList();
			var referencia = correspondencias.Find(grupo => grupo.Resultados.Any(outro => ReferenciaConfirmada(outro, item)));
			var grupo = referencia ?? (correspondencias.Count == 1 ? correspondencias[0] : null);

			if (grupo is null)
			{
				if (pontuacao > 40)
					grupos.Add(new Grupo([item], pontuacao));
			}
			else if (grupo.Resultados.All(outro => outro.Source != item.Source))
				grupo.Resultados.Add(item);
		}

		return grupos;
	}

	private static bool MesmaObra(SearchResult a, SearchResult b) =>
		!IdsConflitam(a, b) && (ReferenciaConfirmada(a, b)
			|| (MetadadosCompativeis(a, b)
				&& ObraFilter.NomesNormalizados(a.Obra).Intersect(ObraFilter.NomesNormalizados(b.Obra)).Any()));

	private static bool MetadadosCompativeis(SearchResult a, SearchResult b) =>
		(a.Ano == null || b.Ano == null || a.Ano == b.Ano)
		&& (string.IsNullOrWhiteSpace(a.Formato) || string.IsNullOrWhiteSpace(b.Formato)
			|| a.Formato.Contains("novel", StringComparison.OrdinalIgnoreCase)
				== b.Formato.Contains("novel", StringComparison.OrdinalIgnoreCase));

	private static bool IdsConflitam(SearchResult a, SearchResult b) =>
		a.Source == b.Source
		&& a.Obra.VinculosExternos.TryGetValue(a.Source, out var id)
		&& b.Obra.VinculosExternos.TryGetValue(b.Source, out var outroId)
		&& id != outroId;

	private static bool ReferenciaConfirmada(SearchResult a, SearchResult b) =>
		ReferenciaApontaPara(a, b) || ReferenciaApontaPara(b, a);

	private static bool ReferenciaApontaPara(SearchResult a, SearchResult b) =>
		a.Referencias?.TryGetValue(b.Source, out var referencia) == true
		&& b.Obra.VinculosExternos.TryGetValue(b.Source, out var id) && referencia == id;

	private static int Pontuar(SearchResult resultado, string[] nomes)
	{
		var encontrados = ObraFilter.NomesNormalizados(resultado.Obra).Distinct().ToArray();
		return nomes.Select(StringUtils.NormalizarTitulo).Where(nome => nome.Length > 0)
			.SelectMany(nome => encontrados.Select(encontrado =>
				nome == encontrado ? 100 : Math.Min(99, Fuzz.Ratio(nome, encontrado))))
			.DefaultIfEmpty().Max();
	}

	private sealed record Grupo(List<SearchResult> Resultados, int Pontuacao);
}
