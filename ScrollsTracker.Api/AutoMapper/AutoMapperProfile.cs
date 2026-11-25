using AutoMapper;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Api.AutoMapperProfile
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<ObraRequest, Obra>()
				.ForMember(destino => destino.TitulosAlternativos, membro => membro.Ignore())
				.ForMember(destino => destino.VinculosExternos, membro => membro.Ignore())
				.ForMember(destino => destino.FonteUltimoCapitulo, membro => membro.Ignore())
				.ForMember(destino => destino.UltimoCapituloDetectadoEm, membro => membro.Ignore());
			CreateMap<Obra, ObraResponse>()
				.ForMember(destino => destino.StatusLeitura, membro => membro.Ignore())
				.ForMember(destino => destino.UltimoCapituloLido, membro => membro.Ignore())
				.ForMember(destino => destino.LancamentoRecente, membro => membro.Ignore());
		}
	}
}
