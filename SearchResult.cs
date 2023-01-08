using Wox.Plugin.Interfaces;

namespace Plugin.Everything
{
    internal class SearchResult : IFileDropResult
    {
        public string Path { get; set; }

        public SearchResult(string path)
        {
            Path = path;
        }
    }
}
