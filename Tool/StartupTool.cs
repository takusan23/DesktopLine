using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DesktopLine.Tool
{
    /// <summary>
    /// スタートアップにショートカットを作成する。
    /// なんか面倒くさい。
    /// プロジェクト右クリック > 追加 > COM参照 へ進み、 Windows Script Host Object Model にチャックを入れる
    /// </summary>
    public class StartupTool
    {

        /// <summary>
        /// スタートアップに登録されていれば true を返す
        /// </summary>
        /// <returns>登録済みなら true</returns>
        public static bool IsRegisterStartup()
        {
            // パス。現在実行中のファイルのパス
            var appPath = Process.GetCurrentProcess().MainModule.FileName;
            // このアプリ名。拡張子は抜いてある
            var appName = Path.GetFileNameWithoutExtension(appPath);
            // ショートカット先。スタートアップ
            var shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutFiles = Directory.GetFiles(shortcutAddress);
            // 追加か削除か。trueなら追加済み
            return shortcutFiles.Select(Path.GetFileNameWithoutExtension).Contains(appName);
        }

        /// <summary>
        /// ショートカットに追加する
        /// </summary>
        public static void RegisterStartup()
        {
            // パス。現在実行中のファイルのパス
            var appPath = Process.GetCurrentProcess().MainModule.FileName;
            // このアプリ名。拡張子は抜いてある
            var appName = Path.GetFileNameWithoutExtension(appPath);
            // ショートカット先。スタートアップ
            var shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            // 追加する
            var shell = new IWshRuntimeLibrary.WshShell();
            // ショートカット作成
            var objShortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(@$"{shortcutAddress}\{appName}.lnk");
            // ショートカット元。本家。
            objShortcut.TargetPath = appPath;
            // ショートカットを保存
            objShortcut.Save();
        }

        /// <summary>
        /// ショートカットから削除する
        /// </summary>
        public static void UnRegisterStartup()
        {
            // パス。現在実行中のファイルのパス
            var appPath = Process.GetCurrentProcess().MainModule.FileName;
            // このアプリ名。拡張子は抜いてある
            var appName = Path.GetFileNameWithoutExtension(appPath);
            // ショートカット先。スタートアップ
            var shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            // 追加済みなので解除
            File.Delete(@$"{shortcutAddress}\{appName}.lnk");
        }

    }
}
