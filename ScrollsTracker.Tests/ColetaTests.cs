using Microsoft.Extensions.Logging.Abstractions;
using ScrollsTracker.Application.Services;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Tests;

public class ColetaTests
{
	[Fact]
	public async Task ObrasComMesmoNomePermanecemComoOpcoes()
	{
		var coletor = Criar(new Fonte(EnumSources.MangaDex,
			[Resultado(EnumSources.MangaDex, "Amanhecer", null, id: "um", ano: 2018),
			 Resultado(EnumSources.MangaDex, "Amanhecer", null, id: "dois", ano: 2024)]));

		var busca = await coletor.BuscarCandidatosAsync("Amanhecer", [], CancellationToken.None);

		Assert.Null(busca.Obra);
		Assert.Equal(new int?[] { 2018, 2024 }, busca.Candidatos.Select(item => item.Ano));
	}

	[Fact]
	public async Task MaiorCapituloValidoRegistraNumeroEFonte()
	{
		var coletor = Criar(
			new Fonte(EnumSources.MangaDex, [Resultado(EnumSources.MangaDex, "Amanhecer", "10")]),
			new Fonte(EnumSources.MangaKatana, [Resultado(EnumSources.MangaKatana, "Amanhecer", "12,50")]),
			new Fonte(EnumSources.MangaHere, [], Falhar: true),
			new Fonte(EnumSources.Kitsu, [Resultado(EnumSources.Kitsu, "Amanhecer", "999")]));

		var obra = new Obra { Titulo = "Amanhecer", TotalCapitulos = "11" };
		var atualizado = await coletor.BuscarEAtualizaObraAsync(obra);

		Assert.Equal("12.5", obra.TotalCapitulos);
		Assert.Equal(EnumSources.MangaKatana, obra.FonteUltimoCapitulo);
		Assert.NotNull(atualizado.Lancamento);
		Assert.Equal("12.5", atualizado.Lancamento.Numero);
		Assert.Equal(EnumSources.MangaKatana, atualizado.Lancamento.Fonte);
		Assert.Equal(DateTimeKind.Utc, atualizado.Lancamento.DetectadoEm.Kind);

		var repetido = await coletor.BuscarEAtualizaObraAsync(obra);
		Assert.Null(repetido.Lancamento);
	}

	[Theory]
	[InlineData(null, "12")]
	[InlineData("15", "15")]
	public async Task PrimeiraObservacaoENumeroMenorNaoGeramLancamento(string? atual, string esperado)
	{
		var coletor = Criar(new Fonte(EnumSources.MangaDex, [Resultado(EnumSources.MangaDex, "Amanhecer", "12")]));
		var obra = new Obra { Titulo = "Amanhecer", TotalCapitulos = atual };

		var resultado = await coletor.BuscarEAtualizaObraAsync(obra);

		Assert.Equal(esperado, obra.TotalCapitulos);
		Assert.Null(resultado.Lancamento);
	}

	private static ObraAggregatorService Criar(params IObraSource[] fontes) =>
		new(fontes, NullLogger<ObraAggregatorService>.Instance);

	[Fact]
	public async Task BuscaAmbiguaAdiaDetalhesEChaveSoFuncionaParaDono()
	{
		using var cache = new CacheCandidatos();
		var fonte = new FonteComDetalhes();
		var coletor = new ObraAggregatorService([fonte], NullLogger<ObraAggregatorService>.Instance, cache);

		var busca = await coletor.BuscarCandidatosAsync("Amanhecer", [], CancellationToken.None, "leitor-a");
		Assert.Equal(2, busca.Candidatos.Count);
		Assert.Empty(fonte.Carregados);
		var escolhido = busca.Candidatos.Single(item => item.Ano == 2024);
		Assert.True(escolhido.Chave.HasValue);
		Assert.Null(await coletor.SelecionarCandidatoAsync(escolhido.Chave.Value, "leitor-b", CancellationToken.None));
		var obra = await coletor.SelecionarCandidatoAsync(escolhido.Chave.Value, "leitor-a", CancellationToken.None);

		Assert.Equal("2024", Assert.Single(fonte.Carregados));
		Assert.Equal("8", obra?.TotalCapitulos);
	}

	private sealed class FonteComDetalhes : IObraSource
	{
		public EnumSources SourceName => EnumSources.MangaDex;
		public List<string> Carregados { get; } = [];

		public Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken) =>
			Task.FromResult<IReadOnlyList<SearchResult>>(
				[Resultado(SourceName, "Amanhecer", null, "2018", 2018) with { RequerDetalhes = true },
				 Resultado(SourceName, "Amanhecer", null, "2024", 2024) with { RequerDetalhes = true }]);

		public Task<SearchResult> CompletarAsync(SearchResult resultado, CancellationToken cancellationToken)
		{
			Carregados.Add(resultado.Obra.VinculosExternos[SourceName]);
			resultado.Obra.TotalCapitulos = "8";
			return Task.FromResult(resultado);
		}
	}

	private static SearchResult Resultado(EnumSources fonte, string titulo, string? capitulo, string? id = null, int? ano = null) =>
		new(new Obra { Titulo = titulo, TotalCapitulos = capitulo, VinculosExternos = id is null ? [] : new() { [fonte] = id } }, fonte, ano);

	private sealed record Fonte(EnumSources SourceName, IReadOnlyList<SearchResult> Resultados, bool Falhar = false) : IObraSource
	{
		public Task<IReadOnlyList<SearchResult>> BuscarAsync(string titulo, CancellationToken cancellationToken) =>
			Falhar ? Task.FromException<IReadOnlyList<SearchResult>>(new HttpRequestException("Falha simulada"))
				: Task.FromResult(Resultados);
	}
}
