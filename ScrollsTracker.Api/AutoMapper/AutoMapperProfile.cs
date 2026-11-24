using AutoMapper;
using ScrollsTracker.Api.Requests;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Api.AutoMapperProfile
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<ObraRequest, Obra>();
			CreateMap<Obra, ObraResponse>()
				.ForMember(destino => destino.StatusLeitura, membro => membro.Ignore())
				.ForMember(destino => destino.UltimoCapituloLido, membro => membro.Ignore());
		}
	}
}
