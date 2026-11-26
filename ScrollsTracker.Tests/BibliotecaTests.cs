using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ScrollsTracker.Api.AutoMapperProfile;
using ScrollsTracker.Api.Repository;
using ScrollsTracker.Api.Services;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Tests;

public class BibliotecaTests
{
	[Fact]
	public async Task AliasEPaginacaoUsamApenasOProgressoDaConta()
	{
		await using var conexao = new SqliteConnection("Data Source=:memory:");
		await conexao.OpenAsync();
		await using var db = CriarContexto(conexao);
		await db.Database.MigrateAsync();
		var a = new Obra { Titulo = "Amanhecer", TitulosAlternativos = ["Hoegwijawa Maengin Seongnyeo"] };
		var b = new Obra { Titulo = "Neblina" };
		db.AddRange(a, b, new IdentityUser { Id = "leitor-a" }, new IdentityUser { Id = "leitor-b" });
		await db.SaveChangesAsync();
		db.UsuarioObras.Add(new UsuarioObra
		{
			UsuarioId = "leitor-a", ObraId = a.Id, StatusLeitura = "Lendo", UltimoCapituloLido = "4"
		});
		await db.SaveChangesAsync();
		var biblioteca = new BibliotecaService(db);

		var alias = await biblioteca.BuscarAsync("leitor-a", "maengin", null, "Lendo", 1, 1, CancellationToken.None);
		Assert.Equal("4", Assert.Single(alias.Obras).UltimoCapituloLido);
		Assert.Equal(a.Id, alias.Obras[0].Id);
		var outraConta = await biblioteca.BuscarAsync("leitor-b", "maengin", null, "Lendo", 1, 1, CancellationToken.None);
		Assert.Empty(outraConta.Obras);
		var pagina = await biblioteca.BuscarAsync("leitor-a", null, null, null, 2, 1, CancellationToken.None);
		Assert.Equal(2, pagina.TotalPaginas);
		Assert.Equal(b.Id, Assert.Single(pagina.Obras).Id);
	}

	[Fact]
	public async Task SeguirNaoCompartilhaOuApagaProgressoEContinuarMostraApenasLeituraPendente()
	{
		await using var conexao = new SqliteConnection("Data Source=:memory:");
		await conexao.OpenAsync();
		await using var db = CriarContexto(conexao);
		await db.Database.MigrateAsync();
		var obra = new Obra { Titulo = "Amanhecer", TotalCapitulos = "10" };
		db.AddRange(obra, new IdentityUser { Id = "leitor-a" }, new IdentityUser { Id = "leitor-b" });
		await db.SaveChangesAsync();
		var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()).CreateMapper();
		var leituras = new LeituraService(db, mapper);
		await leituras.AtualizarAsync(obra.Id, "leitor-a", new() { StatusLeitura = "Lendo", UltimoCapituloLido = "4" });
		await leituras.AtualizarAsync(obra.Id, "leitor-b", new() { StatusLeitura = "Lendo", UltimoCapituloLido = "10" });
		await db.SaveChangesAsync();
		await leituras.SeguirAsync(obra.Id, "leitor-a", true);
		await db.SaveChangesAsync();

		Assert.Equal("4", Assert.Single(await leituras.ContinuarAsync("leitor-a", CancellationToken.None)).UltimoCapituloLido);
		Assert.Empty(await leituras.ContinuarAsync("leitor-b", CancellationToken.None));
		Assert.False(Assert.Single(await leituras.ComLeituraAsync([obra], "leitor-b")).Seguindo);
		await leituras.SeguirAsync(obra.Id, "leitor-a", false);
		await db.SaveChangesAsync();
		var leitura = Assert.Single(await leituras.ComLeituraAsync([obra], "leitor-a"));
		Assert.False(leitura.Seguindo);
		Assert.Equal("4", leitura.UltimoCapituloLido);
	}

	[Fact]
	public async Task CadastroNaoViraLancamentoERepeticaoNaoDuplicaHistorico()
	{
		await using var conexao = new SqliteConnection("Data Source=:memory:");
		await conexao.OpenAsync();
		await using var db = CriarContexto(conexao);
		await db.Database.MigrateAsync();
		var repo = new ObraRepository(db);
		var obra = new Obra { Titulo = "Amanhecer", TotalCapitulos = "10" };
		await repo.CadastrarObraAsync(obra);
		Assert.Empty(await repo.ObterLancamentosRecentesAsync(CancellationToken.None));

		obra.TotalCapitulos = "11";
		var agora = DateTime.UtcNow;
		for (var tentativa = 0; tentativa < 2; tentativa++)
		{
			var lancamento = new LancamentoCapitulo
			{
				ObraId = obra.Id, Numero = "11", Fonte = EnumSources.MangaDex, DetectadoEm = agora
			};
			Assert.True(await repo.SalvarAtualizacaoAsync(new(obra, lancamento), CancellationToken.None));
		}

		var resultado = Assert.Single(await repo.ObterLancamentosRecentesAsync(CancellationToken.None));
		Assert.Equal("11", resultado.Numero);
		Assert.Equal(EnumSources.MangaDex, resultado.Fonte);
		Assert.Equal(DateTimeKind.Utc, resultado.DetectadoEm.Kind);
		Assert.Single(await repo.ObterHistoricoAsync(obra.Id, CancellationToken.None));
	}

	private static AppDbContext CriarContexto(SqliteConnection conexao) =>
		new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conexao).Options);
}
