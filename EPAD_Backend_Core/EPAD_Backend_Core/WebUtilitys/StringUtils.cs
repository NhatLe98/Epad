namespace EPAD_Backend_Core.WebUtilitys
{
    public static class StringExtensions
    {
        public static string TextOverflowEllipsis(this string text, int maxLength)
        {
            if (text.Length - 3 < maxLength)
            {
                return text;
            }
            return text[..(maxLength - 3)] + "...";
        }
    }
}
