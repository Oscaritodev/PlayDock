using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NewLauncher.Helpers
{
    public static class IconExtractor
    {
        // P/Invoke constants
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource? Extract(string path)
        {
            try
            {
                var shinfo = new SHFILEINFO();
                // Get the large icon
                SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

                if (shinfo.hIcon == IntPtr.Zero) return null;

                // Create the BS
                var image = Imaging.CreateBitmapSourceFromHIcon(
                    shinfo.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // Cleanup
                DestroyIcon(shinfo.hIcon);

                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}
