namespace CollabTool.Web
{
	public static class StringExtensions
	{
		public static string IfNullThen(this string str, string replacement)
		{
			return !string.IsNullOrEmpty(str) ? str : replacement;
		}
	}
}