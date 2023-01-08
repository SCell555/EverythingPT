namespace Plugin.Everything
{
    [Flags]
    internal enum SearchFlags : uint
    {
        None                = 0,
        MatchCase           = 0x001,
        MatchWord           = 0x002,
        MatchPath           = 0x004,
        Regex               = 0x008,
        MatchDiacritics     = 0x010,
        MatchPrefix         = 0x020,
        MatchSuffix         = 0x040,
        IgnorePunctuation   = 0x080,
        IgnoreWhitespace    = 0x100
    }

    [Flags]
    internal enum ItemFlags : uint
    {
        Folder  = 0x1,
        Drive   = 0x2,
        Root    = Drive
    }

    internal enum SortMode : uint
    {
        NameAscending                   = 1,
        NameDescending                  = 2,
        PathAscending                   = 3,
        PathDescending                  = 4,
        SizeAscending                   = 5,
        SizeDescending                  = 6,
        ExtensionAscending              = 7,
        ExtensionDescending             = 8,
        TypeNameAscending               = 9,
        TypeNameDescending              = 10,
        DateCreatedAscending            = 11,
        DateCreatedDescending           = 12,
        DateModifiedAscending           = 13,
        DateModifiedDescending          = 14,
        AttributesAscending             = 15,
        AttributesDescending            = 16,
        FileListFilenameAscending       = 17,
        FileListFilenameDescending      = 18,
        RunCountAscending               = 19,
        RunCountDescending              = 20,
        DateRecentlyChangedAscending    = 21,
        DateRecentlyChangedDescending   = 22,
        DateAccessedAscending           = 23,
        DateAccessedDescending          = 24,
        DateRunAscending                = 25,
        DateRunDescending               = 26
    }
}
