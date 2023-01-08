namespace Plugin.Everything
{
    internal class EverythingSettings
    {
        public SearchFlags SearchFlags { get; set; } = SearchFlags.None;
        public SortMode SortMode { get; set; } = SortMode.NameAscending;
        public uint MaxSearchCount { get; set; } = 30;
    }
}
