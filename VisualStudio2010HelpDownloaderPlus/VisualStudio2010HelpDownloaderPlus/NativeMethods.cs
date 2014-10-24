using System.Runtime.InteropServices;

namespace VisualStudio2010HelpDownloaderPlus
{
    /// <summary>
    /// Native code helper.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("UxTheme", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(HandleRef handle, string subApplicationName, string subIDList);
    }
}