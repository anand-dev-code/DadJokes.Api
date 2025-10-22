using System.Text.RegularExpressions;

namespace DadJokes.Api.Utils
{
    public static class TextHighlighter
    {
        /// <summary>
        /// Wraps matches of <paramref name="term"/> in the text with << and >> (case-insensitive).
        /// Preserves the original casing of the matched text.
        /// </summary>
        public static string Highlight(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term))
                return text;

            try
            {
                var pattern = Regex.Escape(term);
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var result = regex.Replace(text, m => $"<<{m.Value}>>");
                return result;
            }
            catch
            {
                return text;
            }
        }
    }
}
