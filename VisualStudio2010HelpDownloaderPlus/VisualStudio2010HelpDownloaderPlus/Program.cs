using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace VisualStudio2010HelpDownloaderPlus
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogException(e.ExceptionObject as Exception);
            Application.ThreadException += (sender, e) => LogException(e.Exception);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Log exception to event log.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public static void LogException(Exception ex)
        {
            if (null != ex)
                EventLog.WriteEntry(
                    "Application",
                    string.Format(CultureInfo.InvariantCulture, "{1}{0}{0}{2}", Environment.NewLine, ex.Message, ex.StackTrace),
                    EventLogEntryType.Error);
        }
    }
}