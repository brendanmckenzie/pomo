using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Pomo
{
    public static class WinAPI
    {
        #region Private Methods

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out WinRect lpRect);

        static IntPtr GetTrayHandle()
        {
            IntPtr taskBarHandle = WinAPI.FindWindow("Shell_TrayWnd", null);
            if (!taskBarHandle.Equals(IntPtr.Zero))
            {
                return WinAPI.FindWindowEx(taskBarHandle, IntPtr.Zero, "TrayNotifyWnd", IntPtr.Zero);
            }
            return IntPtr.Zero;
        }

        #endregion

        #region Public Methods

        public static Rectangle GetTrayRectangle()
        {
            WinRect rect;
            GetWindowRect(GetTrayHandle(), out rect);
            return new Rectangle(new Point(rect.left, rect.top), new Size((rect.right - rect.left) + 1, (rect.bottom - rect.top) + 1));
        }

        #endregion

        #region Models

        private struct WinRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return "(" + left + ", " + top + ") --> (" + right + ", " + bottom + ")";
            }
        }

        #endregion
    }
}