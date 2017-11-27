namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal static class StringExtensions
    {
        public static string FirstCharToUpper(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? value : value.Substring(0, 1).ToUpper() + value.Substring(1);
        }
    }
}
