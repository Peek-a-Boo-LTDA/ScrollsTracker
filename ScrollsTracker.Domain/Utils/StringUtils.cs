namespace ScrollsTracker.Domain.Utils
{
	public static class StringUtils
	{
		public static string ManterApenasNumeros(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			return string.Concat(input.Where(char.IsDigit));
		}
	}
}
