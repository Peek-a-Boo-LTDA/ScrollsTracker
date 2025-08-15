using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Application.Commands;
using ScrollsTracker.Domain.Interfaces.Facade;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrollsTrackerController : ControllerBase
    {
		private readonly IObraFacade _obraFacade;
        private readonly IMediator _mediator;
		private readonly IMapper _mapper;

		public ScrollsTrackerController(IObraFacade obraFacade, IMediator mediator, IMapper mapper)
		{
			_obraFacade = obraFacade;
			_mediator = mediator;
			_mapper = mapper;
		}

		[HttpGet("Obras")]
        public async Task<IActionResult> GetAllObrasAsync()
        {
			return Ok(await _obraFacade.ObterTodasObrasAsync());
        }

		[HttpGet("ProcurarObraNasApisExternas")]
		public async Task<IActionResult> ProcurarObraApisExternasAsync(string titulo)
		{
			return Ok(await _obraFacade.BuscarObraAgregadaAsync(titulo));
		}

		[HttpGet("ObterObra/{id}")]
		public async Task<IActionResult> GetObraByIdAsync(int id)
		{
			return Ok(await _obraFacade.GetObraByIdAsync(id));
		}

		[HttpGet("ObterLancamentos")]
		public async Task<IActionResult> GetLancamentosAsync()
		{
			return Ok(await _obraFacade.ObterLancamentosAsync());
		}

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

		[HttpPost("CadastrarObra")]
        public async Task<IActionResult> CadastrarObraAsync([FromBody] ObraRequest obraRequest)
        {
            try
            {
				var obra = _mapper.Map<Obra>(obraRequest);
				var result = await _obraFacade.CadastrarObraAsync(obra);

				return Created(nameof(CadastrarObraAsync), result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

		[HttpPut("AtualizarObra")]
		public async Task<IActionResult> AtualizarObraAsync([FromBody] ObraRequest obraRequest)
		{
			try
			{
				var obra = _mapper.Map<Obra>(obraRequest);
				var result = await _obraFacade.UpdateObraAsync(obra);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

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
			string jsonContent = System.Text.Json.JsonSerializer.Serialize(obras);

			byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);

			string fileName = "obras.json";

			return File(fileBytes, "application/json", fileName);
		}

		[HttpPost("importar-obras")]
		public async Task<IActionResult> ImportJsonFileAsync([FromBody] List<ObraRequest> obraRequest)
		{
			var obras = _mapper.Map<List<Obra>>(obraRequest);
			var result = await _obraFacade.CadastrarObrasAsync(obras);
			return Ok(result);
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
