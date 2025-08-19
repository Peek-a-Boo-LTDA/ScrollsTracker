using System.Text.RegularExpressions;

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

			var capitulo = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
			return Regex.Replace(capitulo, "[^0-9]", "");

			//Antiga forma de limpar a string dos capitulos, caso a nova de problema...
			//return string.Concat(input.Where(char.IsDigit));
		}
	}
}
