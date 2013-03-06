using System.Text.RegularExpressions;

namespace CollabTool.Web
{
	public static class StringExtensions
	{
		public static string IfNullThen(this string str, string replacement)
		{
			return !string.IsNullOrEmpty(str) ? str : replacement;
		}

		/// <summary>
		/// Splits a string at capital letters e.g. "HelloWorld" becomes "Hello World"
		/// </summary>
		public static string SplitAtCapitalLetters(this string str)
		{
			var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
			return r.Replace(str, " ");
		}
	}
}