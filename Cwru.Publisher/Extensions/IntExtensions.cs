namespace Cwru.Publisher.Extensions
{
    public static class IntExtensions
    {
        public static string Select(this int count, string value, string pluralizeValue)
        {
            return count == 1 ? value : pluralizeValue;
        }
    }
}
