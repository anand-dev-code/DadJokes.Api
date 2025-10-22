using System.Text.RegularExpressions;

namespace DadJokes.Api.Utils
{
    public static class WordCounter
    {
        private static readonly Regex _wordSplit = new(@"\S+", RegexOptions.Compiled);

        public static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return _wordSplit.Matches(text).Count;
        }
    }
}
