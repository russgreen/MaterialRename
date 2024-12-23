﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MaterialRename;
internal class WindowHandle : IWin32Window
{
    private readonly IntPtr _hwnd;

    public WindowHandle(IntPtr h)
    {
        Debug.Assert(IntPtr.Zero != h, "expected non-null window handle");
        _hwnd = h;
    }
    public WindowHandle()
    {
        Handle = RevitMainWindowHandle;
    }

    public static IntPtr RevitMainWindowHandle { get; set; }

    public IntPtr Handle { get; }
}
