using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;
using ScrollsTracker.Infra.Model;

namespace ScrollsTracker.Application.Services.Filter
{
	public class ObraFilter : IObraFilter
	{
		private readonly Obra _obra;
		private int Score = 0;

		public ObraFilter(string titulo)
		{
			_obra = new Obra { Titulo = titulo };
		}

		public ObraFilter(SearchResult search)
		{
			_obra = search.Obra;
			Score = search.Score;
		}

		public ObraFilter(Obra obra)
		{
			_obra = obra;
		}

		public void Filtrar(SearchResult search, bool obraNova = false)
		{
			if (obraNova)
			{
				//FiltrarTituloObraNova(search);
				FiltrarDescricao(search);
				FiltrarTotalCapitulos(search);
				FiltrarImagem(search);
				FiltrarStatus(search);
			}
			else
			{
				//FiltrarTitulo(search);
				FiltrarDescricao(search);
				FiltrarTotalCapitulos(search);
				FiltrarImagem(search);
				FiltrarStatus(search);
			}
		}

		public Obra ObraFiltrada => _obra;

		private void FiltrarImagem(SearchResult search)
		{
			if (string.IsNullOrEmpty(_obra.Imagem))
			{
				_obra.Imagem = search.Obra.Imagem;
			}

			if (search.Score > Score && !string.IsNullOrEmpty(search.Obra.Imagem))
			{
				_obra.Imagem = search.Obra.Imagem;
			}
		}

		private void FiltrarStatus(SearchResult search)
		{
			if (string.IsNullOrEmpty(_obra.Status))
			{
				_obra.Status = search.Obra.Status;
			}

			//TODO aqui talvez fique melhor por sources
			if (search.Score > Score && !string.IsNullOrEmpty(search.Obra.Imagem))
			{
				_obra.Status = search.Obra.Status;
			}
		}

		private void FiltrarTotalCapitulos(SearchResult search)
		{
			if (_obra.GetTotalCapitulosAsDouble() == 0 || search.Obra.GetTotalCapitulosAsDouble() > _obra.GetTotalCapitulosAsDouble())
			{
				_obra.TotalCapitulos = search.Obra.TotalCapitulos;
				_obra.DataAtualizacao = DateTime.Now;
			}
		}

		private void FiltrarDescricao(SearchResult search)
		{
			if (string.IsNullOrEmpty(_obra.Descricao))
			{
				_obra.Descricao = search.Obra.Descricao;
			}

			//TODO verificar source ou score
			if (search.Source == EnumSources.MangaUpdate && !string.IsNullOrEmpty(search.Obra.Descricao))
			{
				_obra.Descricao = search.Obra.Descricao;
			}
		}

		private void FiltrarTitulo(SearchResult search)
		{
			if (string.IsNullOrEmpty(_obra.Titulo))
			{
				_obra.Titulo = search.Obra.Titulo;
			}
		}

		private void FiltrarTituloObraNova(SearchResult search)
		{
			if (string.IsNullOrEmpty(_obra.Titulo) || search.Source == EnumSources.MangaUpdate)
			{
				_obra.Titulo = search.Obra.Titulo;
			}
		}
	}
}
