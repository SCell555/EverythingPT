using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Everything
{
    internal partial class External
    {
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr SendMessageW(IntPtr hWnd, uint msg, nint wParam, nint lParam);
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr FindWindowW(string? className, string? windowName);
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetClassInfoExW(IntPtr hInstance, string lpClassName, IntPtr lpWndClass);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial short RegisterClassExW(IntPtr lpWndClass);
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr GetModuleHandleW(string? lpModuleHandle);
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr hWnd);
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, uint action, IntPtr changeInfo);
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool WaitMessage();
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PeekMessageW(IntPtr lpMsg, IntPtr hWnd, uint filterMin, uint filterMax, uint removeMsg);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial int GetMessageW(IntPtr lpMsg, IntPtr hWnd, uint filterMin, uint filterMax);
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool TranslateMessage(IntPtr lpMsg);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr DispatchMessageW(IntPtr lpMsg);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr DefWindowProcW(IntPtr hWnd, uint msg, nuint wParam, nint lParam);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial void PostQuitMessage(int code);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr SetWindowLongPtrW(IntPtr hWnd, int index, IntPtr dwNewLong);

        private const uint WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEX
        {
            public delegate IntPtr fnWndProc(IntPtr hWnd, uint msg, nuint wParam, nint lParam);

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public IntPtr lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public IntPtr hIconSm;

            public static WNDCLASSEX Build()
            {
                return new WNDCLASSEX
                {
                    cbSize = Marshal.SizeOf<WNDCLASSEX>()
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct IpcQuery
        {
            public int replyWnd;
            public uint replyCopyDataMsg;
            [MarshalAs(UnmanagedType.U4)]
            public SearchFlags searchFlags;
            public uint offset;
            public uint maxResults;
            public uint requestFlags;
            [MarshalAs(UnmanagedType.U4)]
            public SortMode sortMode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IpcQueryReply
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct Item
            {
                [MarshalAs(UnmanagedType.U4)]
                public ItemFlags flags;
                public int offset;
            }

            public uint totItems;
            public uint numItems;
            public uint offset;
            public uint requestFlags;
            public uint sortType;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        internal readonly record struct QueryResult
        {
            public ItemFlags Flags { get; init; }
            public string Name { get; init; }
            public string Path { get; init; }
            public string HighlighName { get; init; }
            public string HighlighPath { get; init; }
            public bool Folder => (Flags & ItemFlags.Folder) != 0;
            public bool Drive => (Flags & ItemFlags.Drive) != 0;
        }

        private static long _counter = 0;
        private readonly static Lazy<IntPtr> _moduleHandle = new(() => GetModuleHandleW(null), true);
        private readonly static Lazy<bool> _isRegistered = new(() => {
            var wcex = WNDCLASSEX.Build();
            wcex.hInstance = _moduleHandle.Value;
            wcex.lpfnWndProc = GetProcAddress(GetModuleHandleW("user32.dll"), "DefWindowProcW");
            wcex.lpszClassName = "Everything.Plugin";
            var mem = Marshal.AllocHGlobal(Marshal.SizeOf<WNDCLASSEX>());
            Marshal.StructureToPtr(wcex, mem, false);
            var res = RegisterClassExW(mem) != 0;
            Marshal.DestroyStructure<WNDCLASSEX>(mem);
            Marshal.FreeHGlobal(mem);
            return res;
        }, true);

        internal static unsafe List<QueryResult>? RunEverythingQuery(string query, SearchFlags searchFlags, uint offset, uint maxResults, SortMode sortMode)
        {
            var wnd = FindWindowW("EVERYTHING_TASKBAR_NOTIFICATION", null);
            if (wnd == IntPtr.Zero)
                return null;

            if (!_isRegistered.Value)
                return new();

            var ourWnd = CreateWindowExW(0, "Everything.Plugin", $"Everything.Plugin.{Interlocked.Increment(ref _counter)}", 0, 0, 0, 0, 0, 0, 0, _moduleHandle.Value, 0);
            ChangeWindowMessageFilterEx(ourWnd, WM_COPYDATA, 1, 0);

            var result = new List<QueryResult>();
            IntPtr Callback(IntPtr hWnd, uint msg, nuint wParam, nint lParam)
            {
                if (msg == WM_COPYDATA)
                {
                    var cds = Marshal.PtrToStructure<CopyDataStruct>(lParam);
                    if (cds.dwData == 0)
                    {
                        var resultView = Marshal.PtrToStructure<IpcQueryReply>(cds.lpData);
                        var addr = cds.lpData + 20;
                        for (int i = 0; i < resultView.numItems; i++)
                        {
                            var item = Marshal.PtrToStructure<IpcQueryReply.Item>(addr + i * 8);
                            var itemAddr = cds.lpData + item.offset;
                            var nameLength = Marshal.ReadInt32(itemAddr);
                            var pathLength = Marshal.ReadInt32(itemAddr + (nameLength + 1) * 2 + 4);
                            var highlightNameLength = Marshal.ReadInt32(itemAddr + (nameLength + 1 + pathLength + 1) * 2 + 4 + 4);
                            var highlightPathLength = Marshal.ReadInt32(itemAddr + (nameLength + 1 + pathLength + 1 + highlightNameLength + 1) * 2 + 4 + 4 + 4);
                            result.Add(new QueryResult
                            {
                                Flags = item.flags,
                                Name = Marshal.PtrToStringUni(itemAddr + 4, nameLength),
                                Path = Marshal.PtrToStringUni(itemAddr + (nameLength + 1) * 2 + 4 + 4, pathLength),
                                HighlighName = Marshal.PtrToStringUni(itemAddr + (nameLength + 1 + pathLength + 1) * 2 + 4 + 4 + 4, highlightNameLength),
                                HighlighPath = Marshal.PtrToStringUni(itemAddr + (nameLength + 1 + pathLength + 1 + highlightNameLength + 1) * 2 + 4 + 4 + 4 + 4, highlightPathLength)
                            });
                        }
                    }
                    PostQuitMessage(0);
                    return 1;
                }
                return DefWindowProcW(hWnd, msg, wParam, lParam);
            };
            WNDCLASSEX.fnWndProc cbFunc = Callback;
            var funcLock = GCHandle.Alloc(cbFunc);
            var old = SetWindowLongPtrW(ourWnd, -4, Marshal.GetFunctionPointerForDelegate(cbFunc));

            CopyDataStruct cds;
            cds.cbData = 28 + (query.Length + 1) * 2;
            cds.dwData = 18;
            cds.lpData = Marshal.AllocHGlobal(cds.cbData);

            IpcQuery ipcQuery;
            ipcQuery.replyWnd = ourWnd.ToInt32();
            ipcQuery.replyCopyDataMsg = 0;
            ipcQuery.searchFlags = searchFlags;
            ipcQuery.offset = offset;
            ipcQuery.maxResults = maxResults;
            ipcQuery.requestFlags = 0x6003;
            ipcQuery.sortMode = sortMode;

            Marshal.StructureToPtr(ipcQuery, cds.lpData, false);
            Marshal.Copy(Encoding.Unicode.GetBytes(query), 0, cds.lpData + 28, query.Length * 2);
            Marshal.Copy(new byte[2], 0, cds.lpData + 28 + query.Length * 2, 2);

            SendMessageW(wnd, WM_COPYDATA, ourWnd, (IntPtr)Unsafe.AsPointer(ref cds));

            var mem = Marshal.AllocHGlobal(64);
            while (true)
            {
                WaitMessage();

                while (PeekMessageW(mem, 0, 0, 0, 0))
                {
                    if (GetMessageW(mem, 0, 0, 0) <= 0)
                        goto done;
                    TranslateMessage(mem);
                    DispatchMessageW(mem);
                }
            }
            done:

            Marshal.FreeHGlobal(mem);
            Marshal.DestroyStructure<IpcQuery>(cds.lpData);
            Marshal.FreeHGlobal(cds.lpData);

            SetWindowLongPtrW(ourWnd, -4, old);
            DestroyWindow(ourWnd);
            funcLock.Free();

            return result;
        }
    }
}
