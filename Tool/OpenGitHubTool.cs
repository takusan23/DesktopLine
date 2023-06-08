using System.Diagnostics;

namespace DesktopLine.Tool
{
    public class OpenGitHubTool
    {

        /// <summary>
        /// ソースコードを開く
        /// </summary>
        public static void OpenGitHubWebPage()
        {
            var info = new ProcessStartInfo()
            {
                FileName = "https://github.com/takusan23/DesktopLine",
                UseShellExecute = true
            };
            Process.Start(info);
        }

    }
}
