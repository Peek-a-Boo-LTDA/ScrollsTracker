using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Interfaces;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Application.Services.Filter
{
	public class ObraFilter : IObraFilter
	{
		private readonly Obra _obra;

		public ObraFilter(string titulo)
		{
			_obra = new Obra { Titulo = titulo };
		}

		public ObraFilter(Obra obra)
		{
			_obra = obra;
		}

		public void Filtrar(Obra obra, EnumSources origem, bool obraNova = false)
		{
			if (obraNova)
			{
				//FiltrarTituloObraNova(obra, origem);
				FiltrarDescricao(obra, origem);
				FiltrarTotalCapitulos(obra, origem);
				FiltrarImagem(obra, origem);
				FiltrarStatus(obra, origem);
			}
			else
			{
				//FiltrarTitulo(obra, origem);
				FiltrarDescricao(obra, origem);
				FiltrarTotalCapitulos(obra, origem);
				FiltrarImagem(obra, origem);
				FiltrarStatus(obra, origem);
			}
		}

		public Obra ObraFiltrada => _obra;

		private void FiltrarImagem(Obra obra, EnumSources origem)
		{
			if (string.IsNullOrEmpty(_obra.Imagem))
			{
				_obra.Imagem = obra.Imagem;
			}

			if (origem == EnumSources.MangaDex && !string.IsNullOrEmpty(obra.Imagem))
			{
				_obra.Imagem = obra.Imagem;
			}
		}

		private void FiltrarStatus(Obra obra, EnumSources origem)
		{
			if (string.IsNullOrEmpty(_obra.Status))
			{
				_obra.Status = obra.Status;
			}

			if (origem == EnumSources.MangaDex && !string.IsNullOrEmpty(obra.Status))
			{
				_obra.Status = obra.Status;
			}
		}

		private void FiltrarTotalCapitulos(Obra obra, EnumSources origem)
		{
			if (_obra.GetTotalCapitulosAsDouble() == 0 || obra.GetTotalCapitulosAsDouble() > _obra.GetTotalCapitulosAsDouble())
			{
				_obra.TotalCapitulos = obra.TotalCapitulos;
				_obra.DataAtualizacao = DateTime.Now;
			}
		}

		private void FiltrarDescricao(Obra obra, EnumSources origem)
		{
			if (string.IsNullOrEmpty(_obra.Descricao))
			{
				_obra.Descricao = obra.Descricao;
			}

			//if (origem == EnumSources.MangaUpdate && !string.IsNullOrEmpty(obra.Descricao))
			//{
			//	_obra.Descricao = obra.Descricao;
			//}
		}

		private void FiltrarTitulo(Obra obra, EnumSources origem)
		{
			if (string.IsNullOrEmpty(_obra.Titulo))
			{
				_obra.Titulo = obra.Titulo;
			}
		}

		private void FiltrarTituloObraNova(Obra obra, EnumSources origem)
		{
			if (string.IsNullOrEmpty(_obra.Titulo) || origem == EnumSources.MangaUpdate)
			{
				_obra.Titulo = obra.Titulo;
			}
		}
	}
}
