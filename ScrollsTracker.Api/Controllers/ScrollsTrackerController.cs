using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ScrollsTracker.Api.Config;
using ScrollsTracker.Api.Services;
using ScrollsTracker.Infra.Repository.Context;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Application.Commands;
using ScrollsTracker.Application.HostedServices;
using ScrollsTracker.Domain.Interfaces.Facade;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Utils;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ScrollsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ScrollsTrackerController : ControllerBase
    {
		private readonly IObraFacade _obraFacade;
        private readonly IMediator _mediator;
		private readonly IMapper _mapper;
		private readonly AppDbContext _db;
		private readonly LeituraService _leituras;

		private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

		public ScrollsTrackerController(IObraFacade obraFacade, IMediator mediator, IMapper mapper, AppDbContext db, LeituraService leituras)
		{
			_obraFacade = obraFacade;
			_mediator = mediator;
			_mapper = mapper;
			_db = db;
			_leituras = leituras;
		}

		[HttpGet("Obras")]
        public async Task<IActionResult> GetAllObrasAsync()
        {
			return Ok(await _leituras.ComLeituraAsync(await _obraFacade.ObterTodasObrasAsync(), UsuarioId));
        }

		[HttpGet("ProcurarObras")]
		public async Task<IActionResult> ProcurarObrasAsync(string titulo)
		{
			return Ok(await _leituras.ComLeituraAsync(await _obraFacade.ProcurarObrasAsync(titulo), UsuarioId));
		}

		[HttpGet("Biblioteca")]
		public async Task<ActionResult<PaginaBiblioteca>> BuscarBibliotecaAsync(
			[FromServices] BibliotecaService biblioteca, CancellationToken cancellationToken,
			[FromQuery, StringLength(300)] string? titulo = null, [FromQuery, StringLength(40)] string? statusObra = null,
			[FromQuery, StringLength(40)] string? statusLeitura = null,
			[FromQuery, Range(1, int.MaxValue)] int pagina = 1, [FromQuery, Range(1, 60)] int tamanho = 18)
		{
			return Ok(await biblioteca.BuscarAsync(UsuarioId, titulo, statusObra, statusLeitura, pagina, tamanho, cancellationToken));
		}

		[HttpGet("ProcurarObraNasApisExternas")]
		public async Task<IActionResult> ProcurarObraApisExternasAsync(string titulo)
		{
			return Ok(await _obraFacade.BuscarObraAgregadaAsync(titulo));
		}

		[HttpGet("CandidatosExternos")]
		public async Task<ActionResult<BuscaExternaResult>> BuscarCandidatosAsync(
			[FromQuery, StringLength(300, MinimumLength = 1)] string titulo,
			[FromQuery] string[]? titulosAlternativos,
			CancellationToken cancellationToken, [FromQuery] bool resumir = false)
		{
			if (string.IsNullOrWhiteSpace(titulo))
				return BadRequest("Informe um título.");

			return Ok(await _obraFacade.BuscarCandidatosAsync(titulo, titulosAlternativos ?? [], cancellationToken,
				resumir ? UsuarioId : null));
		}

		[HttpGet("CandidatosExternos/{chave:guid}")]
		public async Task<ActionResult<Obra>> SelecionarCandidatoAsync(Guid chave, CancellationToken cancellationToken)
		{
			var obra = await _obraFacade.SelecionarCandidatoAsync(chave, UsuarioId, cancellationToken);
			return obra is null ? NotFound() : Ok(obra);
		}

		[HttpGet("ObterObra/{id}")]
		public async Task<IActionResult> GetObraByIdAsync(int id)
		{
			var obra = await _obraFacade.GetObraByIdAsync(id);
			if (obra == null)
				return NotFound();

			var resposta = await _leituras.ComLeituraAsync([obra], UsuarioId);
			return Ok(resposta[0]);
		}

		[HttpGet("ObterLancamentos")]
		public async Task<IActionResult> GetLancamentosAsync(CancellationToken cancellationToken)
		{
			var lancamentos = (await _obraFacade.ObterLancamentosRecentesAsync(cancellationToken)).DistinctBy(item => item.ObraId).ToList();
			var porObra = lancamentos.ToDictionary(item => item.ObraId);
			var obras = await _leituras.ComLeituraAsync(lancamentos.Select(item => item.Obra).ToList(),
				UsuarioId, cancellationToken);
			var novidades = new List<ObraResponse>();
			foreach (var obra in obras)
			{
				var lancamento = porObra[obra.Id];
				if (Obra.InterpretarCapitulo(lancamento.Numero) is not decimal numero
					|| numero <= (Obra.InterpretarCapitulo(obra.UltimoCapituloLido) ?? 0))
					continue;

				obra.LancamentoRecente = MapearLancamento(lancamento);
				novidades.Add(obra);
			}

			return Ok(novidades);
		}

		[HttpGet("Obras/{id:int}/lancamentos")]
		public async Task<ActionResult<List<LancamentoResponse>>> BuscarHistoricoAsync(int id, CancellationToken cancellationToken)
		{
			if (!await _db.Obras.AnyAsync(obra => obra.Id == id, cancellationToken))
				return NotFound();
			var historico = await _obraFacade.ObterHistoricoAsync(id, cancellationToken);
			return Ok(historico.Select(MapearLancamento).ToList());
		}

		private static LancamentoResponse MapearLancamento(LancamentoCapitulo lancamento) =>
			new(lancamento.Numero, lancamento.Fonte.ToString(), lancamento.DetectadoEm);

		[Authorize(Policy = ContaConfig.PodeCadastrarObras)]
		[HttpPost("ProcurarECadastrarObra")]
		public async Task<IActionResult> ProcurarECadastrarObraAsync([FromBody] ProcurarECadastrarObraCommand command)
		{
			try
			{
				var obraId = await _mediator.Send(command);

				return Created(nameof(ProcurarECadastrarObraAsync), obraId);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize(Policy = ContaConfig.PodeCadastrarObras)]
		[HttpPost("CadastrarObra")]
        public async Task<IActionResult> CadastrarObraAsync([FromBody] ObraRequest obraRequest)
        {
            try
            {
				await using var transacao = await _db.Database.BeginTransactionAsync();
				var obra = _mapper.Map<Obra>(obraRequest);
				AplicarIdentidade(obra, obraRequest);
				AplicarOrigemDoCapitulo(obra, obraRequest);
				var result = await _obraFacade.CadastrarObraAsync(obra);
				if (obraRequest.UltimoCapituloLido != null || obraRequest.StatusLeitura != null)
					await _leituras.AtualizarAsync(obra.Id, UsuarioId, obraRequest);
				await _db.SaveChangesAsync();
				await transacao.CommitAsync();

				return Created(nameof(CadastrarObraAsync), result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

		[Authorize(Policy = ContaConfig.PodeGerenciarObras)]
		[HttpPut("AtualizarObra/{id}")]
		public async Task<IActionResult> UpdateObraAsync(int id, [FromBody] ObraRequest obraRequest)
		{
			var obra = await _obraFacade.GetObraByIdAsync(id);
			if (obra == null)
				return NotFound();

			var capituloAnterior = Obra.InterpretarCapitulo(obra.TotalCapitulos);
			_mapper.Map(obraRequest, obra);
			AplicarIdentidade(obra, obraRequest);
			AplicarOrigemDoCapitulo(obra, obraRequest, capituloAnterior);
			await _leituras.AtualizarAsync(id, UsuarioId, obraRequest);
			try
			{
				return Ok(await _db.SaveChangesAsync());
			}
			catch (DbUpdateException)
			{
				return Conflict("Não foi possível salvar as alterações. Recarregue a obra e tente novamente.");
			}
		}

		[HttpPut("Obras/{id:int}/leitura")]
		public async Task<IActionResult> AtualizarLeituraAsync(int id, LeituraRequest progresso)
		{
			if (!await _db.Obras.AnyAsync(obra => obra.Id == id))
				return NotFound();

			await _leituras.AtualizarAsync(id, UsuarioId, progresso);
			try
			{
				await _db.SaveChangesAsync();
				return NoContent();
			}
			catch (DbUpdateException)
			{
				return Conflict("Não foi possível salvar o progresso. Recarregue a obra e tente novamente.");
			}
		}

		[Authorize(Policy = ContaConfig.PodeGerenciarObras)]
		[HttpDelete("DeletarObra/{id}")]
		public async Task<IActionResult> DeletarObraAsync(int id)
		{
			try
			{
				var result = await _obraFacade.DeleteObraById(id);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("exportar-obras")]
		public async Task<IActionResult> DownloadJsonFileAsync()
		{
			var obras = await _obraFacade.ObterTodasObrasAsync();
			var obrasComLeitura = await _leituras.ComLeituraAsync(obras, UsuarioId);
			string jsonContent = System.Text.Json.JsonSerializer.Serialize(obrasComLeitura);

			byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);

			string fileName = "obras.json";

			return File(fileBytes, "application/json", fileName);
		}

		[Authorize(Policy = ContaConfig.PodeCadastrarObras)]
		[HttpPost("importar-obras")]
		public async Task<IActionResult> ImportJsonFileAsync([FromBody] List<ObraRequest> obraRequest)
		{
			var entradas = obraRequest.DistinctBy(obra => obra.Titulo!.Trim()).ToList();
			await using var transacao = await _db.Database.BeginTransactionAsync();
			foreach (var entrada in entradas)
			{
				var titulo = entrada.Titulo!.Trim();
				var obra = await _db.Obras.SingleOrDefaultAsync(item => item.Titulo == titulo);
				if (obra == null)
				{
					obra = _mapper.Map<Obra>(entrada);
					AplicarIdentidade(obra, entrada);
					AplicarOrigemDoCapitulo(obra, entrada);
					_db.Obras.Add(obra);
					await _db.SaveChangesAsync();
				}

				if (entrada.UltimoCapituloLido != null || entrada.StatusLeitura != null)
					await _leituras.AtualizarAsync(obra.Id, UsuarioId, entrada);
			}

			await _db.SaveChangesAsync();
			await transacao.CommitAsync();
			return Ok(entradas.Count);
		}

		private static void AplicarIdentidade(Obra obra, ObraRequest entrada)
		{
			obra.Titulo = entrada.Titulo!.Trim();
			obra.TitulosAlternativos = StringUtils.LimparTitulosAlternativos(
				obra.Titulo, entrada.TitulosAlternativos ?? obra.TitulosAlternativos);

			if (entrada.VinculosExternos is not { } vinculos)
				return;

			obra.VinculosExternos = new();
			if (vinculos.TryGetValue(EnumSources.MangaDex, out var mangaDex)
				&& Guid.TryParseExact(mangaDex, "D", out var uuid))
			{
				obra.VinculosExternos[EnumSources.MangaDex] = uuid.ToString("D");
			}

			foreach (var fonte in new[] { EnumSources.MangaUpdate, EnumSources.Kitsu })
			{
				if (vinculos.TryGetValue(fonte, out var id)
					&& long.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) && numero > 0)
				{
					obra.VinculosExternos[fonte] = numero.ToString(CultureInfo.InvariantCulture);
				}
			}
		}

		private static void AplicarOrigemDoCapitulo(Obra obra, ObraRequest entrada, decimal? anterior = null)
		{
			var numero = Obra.InterpretarCapitulo(obra.TotalCapitulos);
			if (numero != anterior)
			{
				obra.FonteUltimoCapitulo = null;
				obra.UltimoCapituloDetectadoEm = null;
			}

			if (numero.HasValue && entrada.FonteUltimoCapitulo is (EnumSources.MangaDex or EnumSources.MangaUpdate
				or EnumSources.MangaKatana or EnumSources.MangaHere)
				&& obra.FonteUltimoCapitulo != entrada.FonteUltimoCapitulo)
			{
				obra.FonteUltimoCapitulo = entrada.FonteUltimoCapitulo;
				obra.UltimoCapituloDetectadoEm = DateTime.UtcNow;
			}
		}

		[Authorize(Policy = ContaConfig.PodeGerenciarObras)]
		[HttpPost("atualizar-obras")]
		public async Task<IActionResult> AtualizarObrasAsync([FromServices] AtualizacaoDeObras atualizacao,
			CancellationToken cancellationToken)
		{
			if (!await atualizacao.TentarExecutarAsync(cancellationToken))
				return Conflict("Já há uma rodada de atualização em execução.");
			return Ok();
		}

		// TODO: Refatorar?
		[Obsolete]
		[HttpGet("imagens/{nomeArquivo}")]
        public IActionResult GetImagem(string nomeArquivo)
        {
            try
            {
                string pastaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens");
                string caminhoArquivo = Path.Combine(pastaDestino, nomeArquivo);

                if (!System.IO.File.Exists(caminhoArquivo))
                {
                    return NotFound(new { message = "Imagem não encontrada" });
                }

                string contentType = "image/png";

                return PhysicalFile(caminhoArquivo, contentType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
