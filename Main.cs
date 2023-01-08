using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Plugin.Everything
{
    public partial class Main : IPlugin, IPluginI18n, IContextMenu, ISavable
    {
        private readonly string[] appExtensions = { ".exe", ".bat", ".cmd", ".com", ".appref-ms", ".lnk" };

        private PluginInitContext? _context;
        private readonly PluginJsonStorage<EverythingSettings> _storage = new();
        private EverythingSettings? _settings;

        public string Name => "Everything";
        public string Description => "Search with Everything";

        public string GetTranslatedPluginDescription() => Description;
        public string GetTranslatedPluginTitle() => Name;

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = _storage.Load();
        }

        public void Save() => _storage.Save();

        struct SettingContextData
        {
            public string search;
            public bool count;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData == null)
                return new();
            var pluginName = Assembly.GetExecutingAssembly().GetName().Name;
            if (selectedResult.ContextData is SettingContextData ctx)
            {
                if (!ctx.count)
                {
                    bool isAscend = ((uint)_settings!.SortMode & 1) == 1;
                    return new List<ContextMenuResult>
                    {
                        new ContextMenuResult
                        {
                            PluginName = pluginName,
                            Title = isAscend ? "Descending" : "Ascending",
                            Glyph = isAscend ? "\xE96E" : "\xE96D",
                            FontFamily = "Segoe MDL2 Assets",
                            Action = _ =>
                            {
                                if (isAscend)
                                    _settings.SortMode = (SortMode)((uint)_settings.SortMode + 1);
                                else
                                    _settings.SortMode = (SortMode)((uint)_settings.SortMode - 1);
                                _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                                return false;
                            }
                        },
                        new ContextMenuResult
                        {
                            PluginName = pluginName,
                            Title = "Name",
                            Glyph = "\xE8A5",
                            FontFamily = "Segoe MDL2 Assets",
                            Action = _ =>
                            {
                                _settings.SortMode = isAscend ? SortMode.NameAscending : SortMode.NameDescending;
                                _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                                return false;
                            }
                        },
                        new ContextMenuResult
                        {
                            PluginName = pluginName,
                            Title = "Path",
                            Glyph = "\xE8B7",
                            FontFamily = "Segoe MDL2 Assets",
                            Action = _ =>
                            {
                                _settings.SortMode = isAscend ? SortMode.PathAscending : SortMode.PathDescending;
                                _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                                return false;
                            }
                        },
                        new ContextMenuResult
                        {
                            PluginName = pluginName,
                            Title = "Size",
                            Glyph = "\xE8EC",
                            FontFamily = "Segoe MDL2 Assets",
                            Action = _ =>
                            {
                                _settings.SortMode = isAscend ? SortMode.SizeAscending : SortMode.SizeDescending;
                                _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                                return false;
                            }
                        },
                        new ContextMenuResult
                        {
                            PluginName = pluginName,
                            Title = "Extension",
                            Glyph = "\xE7C3",
                            FontFamily = "Segoe MDL2 Assets",
                            Action = _ =>
                            {
                                _settings.SortMode = isAscend ? SortMode.ExtensionAscending : SortMode.ExtensionDescending;
                                _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                                return false;
                            }
                        }
                    };
                }

                return new List<ContextMenuResult>
                {
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "-10",
                        Glyph = "\xE96E",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            if (_settings!.MaxSearchCount < 10)
                                _settings.MaxSearchCount = 10;
                            else
                                _settings.MaxSearchCount -= 10;
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "-1",
                        Glyph = "\xE949",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            _settings!.MaxSearchCount = Math.Max(1, _settings.MaxSearchCount - 1);
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "Infinite",
                        Glyph = "\xE249",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            _settings!.MaxSearchCount = uint.MaxValue;
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "Default",
                        Glyph = "\xE24A",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            _settings!.MaxSearchCount = 30;
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "+1",
                        Glyph = "\xE948",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            _settings!.MaxSearchCount = Math.Max(1, _settings.MaxSearchCount + 1);
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                    new ContextMenuResult
                    {
                        PluginName = pluginName,
                        Title = "+10",
                        Glyph = "\xE96D",
                        FontFamily = "Segoe MDL2 Assets",
                        Action = _ =>
                        {
                            _settings!.MaxSearchCount = Math.Max(1, _settings.MaxSearchCount + 10);
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + ctx.search, true);
                            return false;
                        }
                    },
                };
            }

            var res = new List<ContextMenuResult>();
            if (appExtensions.Contains(Path.GetExtension(selectedResult.Title)))
            {
                res.Add(new ContextMenuResult
                {
                    PluginName = pluginName,
                    Title = "Run as administrator (Ctrl + Shift + Enter)",
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = _ =>
                    {
                        try
                        {
                            Task.Run(() => Helper.RunAsAdmin(((SearchResult)selectedResult.ContextData).Path));
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    },
                });
                res.Add(new ContextMenuResult
                {
                    PluginName = pluginName,
                    Title = "Run as user (Ctrl + Shift + U)",
                    Glyph = "\xE7EE",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.U,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = _ =>
                    {
                        try
                        {
                            Task.Run(() => Helper.RunAsUser(((SearchResult)selectedResult.ContextData).Path));
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    },
                });
            }

            res.Add(new ContextMenuResult
            {
                PluginName = pluginName,
                Title = "Open containing folder (Ctrl + Shift + E)",
                Glyph = "\xE838",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.E,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ =>
                {
                    if (!Helper.OpenInShell("explorer.exe", $"/select,\"{((SearchResult)selectedResult.ContextData).Path}\""))
                    {
                        var name = $"Plugin: {_context!.CurrentPluginMetadata.Name}";
                        _context.API.ShowMsg(name, "Open directory failed");
                        return false;
                    }

                    return true;
                }
            });

            res.Add(new ContextMenuResult
            {
                PluginName = pluginName,
                Title = "Open folder in console (Ctrl + Shift + C)",
                Glyph = "\xE756",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ =>
                {
                    try
                    {
                        Helper.OpenInConsole(Path.GetDirectoryName(((SearchResult)selectedResult.ContextData).Path));
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                },
            });

            return res;
        }

        private static List<int> BuildHighlightData(string baseString, string stringWithHighlights)
        {
            if (baseString.Length == stringWithHighlights.Replace("**", "*").Length)
                return new();
            var result = new List<int>();
            var highlight = false;
            for (int i = 0, c = 0; i < baseString.Length; ++i)
            {
                if (stringWithHighlights[c++] != '*')
                {
                    if (highlight)
                        result.Add(i);
                    continue;
                }
                if (c == stringWithHighlights.Length)
                    break;
                if (stringWithHighlights[c++] == '*')
                    continue;
                highlight = !highlight;
                if (highlight)
                    result.Add(i);
            }
            return result;
        }

        private static int CalculateScore(string baseString, string stringWithHighlights)
        {
            if (baseString.Length == stringWithHighlights.Replace("**", "*").Length)
                return 100;
            var matchLen = 0;
            var firstIndex = -1;
            var highlight = false;
            for (int i = 0, c = 0; i < baseString.Length; ++i)
            {
                if (stringWithHighlights[c++] != '*')
                {
                    if (highlight)
                        ++matchLen;
                    continue;
                }
                if (c == stringWithHighlights.Length)
                    break;
                if (stringWithHighlights[c++] == '*')
                    continue;
                highlight = !highlight;
                if (firstIndex == -1)
                    firstIndex = i;
                if (highlight)
                    ++matchLen;
            }

            var score = 100 * (float)matchLen / baseString.Length;
            score += 10 * (float)firstIndex / baseString.Length;

            return (int)score;
        }

        public List<Result> Query(Query query)
        {
            var search = query?.Search ?? string.Empty;
            var result = new List<Result>();

            if (search.StartsWith("!>ES:"))
            {
                foreach (var flag in Enum.GetValues<SearchFlags>())
                {
                    if (flag == SearchFlags.None) continue;
                    var name = string.Join(' ', MatchCapital().Split(flag.ToString())).Trim();
                    var nameSearch = $"!>ES: Search flag: {name}";
                    var match = StringMatcher.FuzzySearch(search, nameSearch);
                    if (!match.Success)
                        continue;
                    result.Add(new Result
                    {
                        Title = name,
                        QueryTextDisplay = nameSearch,
                        SubTitle = (_settings!.SearchFlags & flag) == flag ? "On" : "Off",
                        Score = match.Score,
                        IcoPath = _context!.CurrentPluginMetadata.IcoPathDark,
                        Action = _ =>
                        {
                            _settings!.SearchFlags ^= flag;
                            _context!.API.ChangeQuery(_context.CurrentPluginMetadata.ActionKeyword + search, true);
                            return false;
                        }
                    });
                }

                var countMatch = StringMatcher.FuzzySearch(search, "!>ES: Max result count");
                if (countMatch.Success)
                    result.Add(new Result
                    {
                        Title = "Max result count",
                        SubTitle = _settings!.MaxSearchCount.ToString(),
                        QueryTextDisplay = "!>ES: Max result count",
                        IcoPath = _context!.CurrentPluginMetadata.IcoPathDark,
                        Score = countMatch.Score,
                        ContextData = new SettingContextData { search = search, count = true }
                    });

                var sortScore = StringMatcher.FuzzySearch(search, "!>ES: Sort mode");
                if (sortScore.Success)
                    result.Add(new Result
                    {
                        Title = "Sort mode",
                        SubTitle = string.Join(' ', MatchCapital().Split(_settings!.SortMode.ToString())).Trim(),
                        QueryTextDisplay = "!>ES: Sort mode",
                        IcoPath = _context!.CurrentPluginMetadata.IcoPathDark,
                        Score = sortScore.Score,
                        ContextData = new SettingContextData { search = search, count = false }
                    });

                return result;
            }

            var res = External.RunEverythingQuery(search, _settings!.SearchFlags, 0, _settings.MaxSearchCount, _settings.SortMode);
            if (res == null)
            {
                result.Add(new Result
                {
                    Title = "Everything is not running!",
                    SubTitle = "Please make sure to start it."
                });
                return result;
            }

            foreach (var r in res)
            {
                var fullPath = Path.Combine(r.Path, r.Name);
                var score = Math.Max(CalculateScore(r.Name, r.HighlighName), CalculateScore(r.Path, r.HighlighPath) / 2);
                if (r.Folder)
                    score /= 8;
                result.Add(new Result
                {
                    Title = r.Name,
                    SubTitle = r.Path,
                    QueryTextDisplay = search,
                    IcoPath = fullPath,
                    ContextData = new SearchResult(fullPath),
                    ToolTipData = new ToolTipData(r.Name, $"Path: {fullPath}"),
                    TitleHighlightData = BuildHighlightData(r.Name, r.HighlighName),
                    SubTitleHighlightData = BuildHighlightData(r.Path, r.HighlighPath),
                    Score = score,
                    Action = _ =>
                    {
                        bool hide = true;
                        if (!Helper.OpenInShell(fullPath, null, r.Path))
                        {
                            hide = false;
                            var name = $"Plugin: {_context!.CurrentPluginMetadata.Name}";
                            _context.API.ShowMsg(name, "Open failed", string.Empty);
                        }
                        return hide;
                    }
                });
            }

            return result;
        }

        [GeneratedRegex("(?=[A-Z])", RegexOptions.CultureInvariant)]
        private static partial Regex MatchCapital();
    }
}