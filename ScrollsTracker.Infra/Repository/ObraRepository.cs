using Microsoft.EntityFrameworkCore;
using ScrollsTracker.Domain.Interfaces.Repository;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.Repository.Context;

namespace ScrollsTracker.Api.Repository
{
	public class ObraRepository : IObraRepository
	{
		private readonly AppDbContext _context;

		public ObraRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<int> AddAsync(Obra obra)
		{
			_context.Obras.Add(obra);
			return await _context.SaveChangesAsync();
		}

		public async Task<Obra?> GetObraByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _context.Obras.FindAsync([id], cancellationToken);
		}

		public async Task<int> DeleteObraByIdAsync(int id)
		{
			var obraParaDeletar = await _context.Obras.FindAsync(id);

			if (obraParaDeletar == null)
			{
				return 0;
			}

			_context.Obras.Remove(obraParaDeletar);
			return await _context.SaveChangesAsync();
		}

		public async Task<int> UpdateObraAsync(Obra obra)
		{
			if (_context.Entry(obra).State == EntityState.Detached)
				_context.Obras.Update(obra);

			try
			{
				return await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException ex)
			{
				foreach (var entrada in ex.Entries)
					await entrada.ReloadAsync();
				return 0;
			}
		}

		public async Task<int> CadastrarObraAsync(Obra obra)
		{
			_context.Obras.Add(obra);

			return await _context.SaveChangesAsync();
		}

		public async Task<int> CadastrarObrasAsync(List<Obra> obras)
		{
			await _context.Obras.AddRangeAsync(obras);

			return await _context.SaveChangesAsync();
		}

		public async Task<bool> SalvarAtualizacaoAsync(AtualizacaoResult atualizacao, CancellationToken cancellationToken)
		{
			if (atualizacao.Lancamento is { } lancamento && !await _context.Lancamentos.AsNoTracking()
				.AnyAsync(item => item.ObraId == lancamento.ObraId && item.Numero == lancamento.Numero, cancellationToken))
			{
				_context.Lancamentos.Add(lancamento);
			}

			try
			{
				await _context.SaveChangesAsync(cancellationToken);
				return true;
			}
			catch (DbUpdateConcurrencyException)
			{
				return false;
			}
		}

		public async Task<List<Obra>> ObterTodasObrasAsync()
		{
			return await _context.Obras.ToListAsync();
		}

		public async Task<List<Obra>> ProcurarObrasAsync(string titulo)
		{
			if (string.IsNullOrWhiteSpace(titulo))
			{
				return new List<Obra>();
			}

			return await _context.Obras.AsNoTracking().ComTitulo(titulo).ToListAsync();
		}

		public Task<List<int>> ObterIdsParaAtualizarAsync(CancellationToken cancellationToken)
		{
			var limite = DateTime.Now.AddHours(-1);
			return _context.Obras.Where(obra => obra.DataVerificacao <= limite)
				.OrderBy(obra => obra.DataVerificacao).Select(obra => obra.Id).ToListAsync(cancellationToken);
		}

		public Task<List<LancamentoCapitulo>> ObterLancamentosRecentesAsync(CancellationToken cancellationToken)
		{
			var desde = DateTime.UtcNow.AddDays(-3);
			return _context.Lancamentos.AsNoTracking().Include(item => item.Obra)
				.Where(item => item.DetectadoEm >= desde)
				.OrderByDescending(item => item.DetectadoEm).ThenBy(item => item.ObraId)
				.ToListAsync(cancellationToken);
		}

		public Task<List<LancamentoCapitulo>> ObterHistoricoAsync(int obraId, CancellationToken cancellationToken) =>
			_context.Lancamentos.AsNoTracking().Where(item => item.ObraId == obraId)
				.OrderByDescending(item => item.DetectadoEm).Take(30).ToListAsync(cancellationToken);
	}
}
