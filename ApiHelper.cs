using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace ColorNameAssistant
{
    public class ApiHelper
    {
        #region Window cursor position
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref POINT lpPoint);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr window, IntPtr dc);


        public static Point GetCursorPosition()
        {
            POINT cursor = new POINT();
            bool success = GetCursorPos(ref cursor);
            if (success)
                return cursor;
            else
                return new Point(0, 0);
        }


        public static string GetHexColorFromCursorPosition()
        {
            POINT cursor = new POINT();
            bool success = GetCursorPos(ref cursor);
            if (success == false)
                return "#000000";  // Black
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int pixel = (int)GetPixel(dc, cursor.X, cursor.Y);
            ReleaseDC(desk, dc);
            // Convert the int Pixel value > to > int RGB values (without alpha channel) > to > Hex string
            return string.Format("#{0:X2}{1:X2}{2:X2}",
                        pixel & 0x000000FF,
                        (pixel & 0x0000FF00) >> 8,
                        (pixel & 0x00FF0000) >> 16);
        }
        #endregion

        #region Window styles
        [Flags]
        private enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_DLGMODALFRAME = 0x0001,
            // ...
        }

        private enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        private static extern void SetLastError(int dwErrorCode);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        /// <summary>
        /// Hide the window from Windows Task Switcher (ALT + TAB)
        /// Taken from: https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
        /// </summary>
        public static void HideWindowFromTaskSwitcher(Window window)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(window);
            int extendedStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            extendedStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)extendedStyle);
        }

        /// <summary>
        /// Hide the window icon
        /// Taken from: https://stackoverflow.com/questions/2341230/removing-icon-from-a-wpf-window/2341385
        ///             https://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window
        /// </summary>
        public static void RemoveWindowIcon(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            
            // Change the extended window style to not show a window icon
            int extendedStyle = (int)GetWindowLong(hwnd, (int)GetWindowLongFields.GWL_EXSTYLE);
            SetWindowLong(hwnd, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)(extendedStyle | (int)ExtendedWindowStyles.WS_EX_DLGMODALFRAME));

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            
            // This is required when the executable is running not as debug
            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
        }
        #endregion
    }
}
