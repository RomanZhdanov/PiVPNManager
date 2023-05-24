namespace PiVPNManager.Infrastructure.Bot
{
    public static class StringExtensions
    {
        static char[] escapeChars = new char[] { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        public static string ToEscapeMarkDown(this string text)
        {
            foreach (char c in escapeChars)
            {
                text = text.Replace($"{c}", $"\\{c}");
            }

            return text;
        }
    }
}
