namespace ScrollsTracker.DiscordBot.Model.Interfaces
{
	public interface IScrollsTrackerHttpService
	{
		Task<Obra> ProcurarObraNasApisExternasAsync(string titulo);
	}
}
