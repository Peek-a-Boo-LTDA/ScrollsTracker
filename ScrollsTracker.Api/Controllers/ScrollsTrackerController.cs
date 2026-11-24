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
using ScrollsTracker.Domain.Interfaces.Facade;
using ScrollsTracker.Domain.Models;

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

		[HttpGet("ProcurarObraNasApisExternas")]
		public async Task<IActionResult> ProcurarObraApisExternasAsync(string titulo)
		{
			return Ok(await _obraFacade.BuscarObraAgregadaAsync(titulo));
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
		public async Task<IActionResult> GetLancamentosAsync()
		{
			return Ok(await _leituras.ComLeituraAsync(await _obraFacade.ObterLancamentosAsync(), UsuarioId));
		}

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
				obra.Titulo = obra.Titulo!.Trim();
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

			_mapper.Map(obraRequest, obra);
			obra.Titulo = obra.Titulo!.Trim();
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
					obra.Titulo = titulo;
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

		[Authorize(Policy = ContaConfig.PodeGerenciarObras)]
		[HttpPost("atualizar-obras")]
		public async Task<IActionResult> AtualizarObrasAsync()
		{
			var command = new AtualizarObrasCommand();

			try
			{
				await _mediator.Send(command);

				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.ToString());
			}
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
