using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace BrowserSelector.UITests;

/// <summary>
/// UIテスト用のヘルパークラス
/// </summary>
public static class UITestHelper
{
    /// <summary>
    /// アプリケーションの実行ファイルパスを取得します
    /// </summary>
    /// <returns>アプリケーションの実行ファイルパス</returns>
    public static string GetApplicationPath()
    {
        // 複数のパス候補を試行
        string[] pathCandidates = {
            // 現在のディレクトリからの相対パス
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "BrowserSelector.App", "bin", "Debug", "net8.0-windows", "BrowserSelector.App.exe"),
            // プロジェクトルートからの絶対パス
            Path.Combine(GetProjectRoot(), "src", "BrowserSelector.App", "bin", "Debug", "net8.0-windows", "BrowserSelector.App.exe"),
            // 現在のディレクトリからの別の相対パス
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "BrowserSelector.App", "bin", "Debug", "net8.0-windows", "BrowserSelector.App.exe"),
            // 環境変数からのパス
            Environment.GetEnvironmentVariable("BROWSERSELECTOR_APP_PATH") ?? string.Empty
        };

        foreach (string candidate in pathCandidates)
        {
            if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        throw new FileNotFoundException("BrowserSelector.App.exe が見つかりません。以下のパスを確認してください: " + string.Join(", ", pathCandidates));
    }

    /// <summary>
    /// プロジェクトルートディレクトリを取得します
    /// </summary>
    /// <returns>プロジェクトルートディレクトリのパス</returns>
    private static string GetProjectRoot()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;

        // プロジェクトルートを探す（.slnファイルがあるディレクトリ）
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (File.Exists(Path.Combine(currentDir, "BrowserSelector.WPF.sln")))
            {
                return currentDir;
            }

            string? parentDir = Path.GetDirectoryName(currentDir);
            if (parentDir == null || parentDir == currentDir)
            {
                break;
            }
            currentDir = parentDir;
        }

        // フォールバック: 現在のディレクトリから5階層上
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    /// <summary>
    /// アプリケーションが起動可能かどうかを確認します
    /// </summary>
    /// <returns>起動可能な場合はtrue</returns>
    public static bool IsApplicationAvailable()
    {
        try
        {
            string appPath = GetApplicationPath();
            return File.Exists(appPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// アプリケーションパスの情報を取得します（デバッグ用）
    /// </summary>
    /// <returns>パス情報の文字列</returns>
    public static string GetPathInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"Current Directory: {AppDomain.CurrentDomain.BaseDirectory}");
        info.AppendLine($"Project Root: {GetProjectRoot()}");

        try
        {
            string appPath = GetApplicationPath();
            info.AppendLine($"Application Path: {appPath}");
            info.AppendLine($"Application Exists: {File.Exists(appPath)}");
        }
        catch (Exception ex)
        {
            info.AppendLine($"Application Path Error: {ex.Message}");
        }

        return info.ToString();
    }

    /// <summary>
    /// メインウィンドウでURLを設定します
    /// </summary>
    /// <param name="mainWindow">メインウィンドウ</param>
    /// <param name="url">設定するURL</param>
    /// <returns>URL設定が成功した場合はtrue</returns>
    public static bool SetUrlInMainWindow(FlaUI.Core.AutomationElements.Window mainWindow, string url)
    {
        try
        {
            // URL入力フィールドを探す
            var urlTextBox = mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
                .And(cf.ByAutomationId("UrlTextBox"))
                .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));

            if (urlTextBox != null)
            {
                // 既存のテキストをクリア
                urlTextBox.Focus();
                System.Threading.Thread.Sleep(100);

                // テキストボックスに直接値を設定
                if (urlTextBox.Patterns.Value.IsSupported)
                {
                    urlTextBox.Patterns.Value.Pattern.SetValue(url);
                }
                else
                {
                    // フォーカスしてテキストを入力
                    urlTextBox.Focus();
                    System.Threading.Thread.Sleep(100);

                    // キーボード入力でURLを設定
                    FlaUI.Core.Input.Keyboard.TypeSimultaneously(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
                    System.Threading.Thread.Sleep(100);
                    FlaUI.Core.Input.Keyboard.Type(url);
                }
                System.Threading.Thread.Sleep(300);

                Console.WriteLine($"URL設定完了: {url}");
                return true;
            }
            else
            {
                Console.WriteLine("URL入力フィールドが見つかりません");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"URL設定エラー: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// STAスレッドでアプリケーションを起動します
    /// </summary>
    /// <param name="appPath">アプリケーションのパス</param>
    /// <returns>起動されたアプリケーション</returns>
    public static Application LaunchApplicationInSTA(string appPath)
    {
        Application? app = null;
        var tcs = new TaskCompletionSource<Application>();
        var thread = new Thread(() =>
        {
            try
            {
                // STAスレッドでアプリケーションを起動
                app = Application.Launch(appPath);
                tcs.SetResult(app);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        // STAスレッドとして設定
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // アプリケーションの起動を待機
        if (!tcs.Task.Wait(30000)) // 30秒でタイムアウト
        {
            throw new TimeoutException("アプリケーションの起動がタイムアウトしました");
        }

        return tcs.Task.Result;
    }

    /// <summary>
    /// プロセスを直接起動してSTAスレッドでアプリケーションを取得します
    /// </summary>
    /// <param name="appPath">アプリケーションのパス</param>
    /// <returns>起動されたアプリケーション</returns>
    public static Application LaunchApplicationWithProcess(string appPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = appPath,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException("プロセスの起動に失敗しました");
        }

        // プロセスが起動するまで少し待機
        Thread.Sleep(2000);

        // プロセスからアプリケーションを取得
        var app = Application.Attach(process);
        return app;
    }

    /// <summary>
    /// テスト用のアプリケーションを起動します（STAスレッド対応）
    /// </summary>
    /// <param name="appPath">アプリケーションのパス</param>
    /// <returns>起動されたアプリケーション</returns>
    public static Application LaunchTestApplication(string appPath)
    {
        Application? app = null;
        Exception? exception = null;
        
        // STAスレッドでアプリケーションを起動
        var thread = new Thread(() =>
        {
            try
            {
                // STAスレッドとして設定
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                
                // テスト用の環境変数を設定
                Environment.SetEnvironmentVariable("BROWSERSELECTOR_TEST_MODE", "true");
                
                // ポート競合を避けるため、固定ポートを設定
                int testPort = 50001;
                Environment.SetEnvironmentVariable("BROWSERSELECTOR_TEST_PORT", testPort.ToString());
                
                // プロセス起動情報を設定
                var startInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = $"--test-mode --port={testPort}",
                    UseShellExecute = false,
                    CreateNoWindow = false, // ウィンドウを表示してデバッグしやすくする
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                // テストモード用の環境変数を設定
                startInfo.EnvironmentVariables["TestMode"] = "true";
                startInfo.EnvironmentVariables["BROWSERSELECTOR_TEST_MODE"] = "true";
                startInfo.EnvironmentVariables["BROWSERSELECTOR_TEST_PORT"] = testPort.ToString();
                
                Console.WriteLine($"アプリケーション起動開始: {appPath}");
                Console.WriteLine($"引数: {startInfo.Arguments}");
                
                // プロセスを起動
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("アプリケーションの起動に失敗しました");
                }
                
                Console.WriteLine($"プロセス起動完了: PID={process.Id}");
                
                // プロセスが起動するまで少し待機
                Thread.Sleep(3000);
                
                // プロセスが終了していないかチェック
                if (process.HasExited)
                {
                    string errorOutput = process.StandardError.ReadToEnd();
                    string standardOutput = process.StandardOutput.ReadToEnd();
                    throw new InvalidOperationException($"アプリケーションが起動直後に終了しました。エラー: {errorOutput}, 出力: {standardOutput}");
                }
                
                // FlaUIのApplication.Attachを使用してアプリケーションに接続
                app = Application.Attach(process);
                
                // メインウィンドウが利用可能になるまで待機
                using var automation = new FlaUI.UIA3.UIA3Automation();
                var mainWindow = app.GetMainWindow(automation, TimeSpan.FromSeconds(15));
                if (mainWindow == null)
                {
                    throw new InvalidOperationException("メインウィンドウの取得に失敗しました");
                }

                Console.WriteLine($"メインウィンドウ取得完了: {mainWindow.Name}");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        
        // スレッドの完了を待機
        if (!thread.Join(30000)) // 30秒でタイムアウト
        {
            thread.Abort();
            throw new TimeoutException("アプリケーションの起動がタイムアウトしました");
        }
        
        if (exception != null)
        {
            throw new InvalidOperationException($"アプリケーションの起動に失敗しました: {exception.Message}", exception);
        }
        
        if (app == null)
        {
            throw new InvalidOperationException("アプリケーションの起動に失敗しました");
        }
        
        return app;
    }

    /// <summary>
    /// ブラウザボタンが有効になるまで待機します
    /// </summary>
    /// <param name="mainWindow">メインウィンドウ</param>
    /// <param name="timeoutMs">タイムアウト時間（ミリ秒）</param>
    /// <returns>ブラウザボタンが有効になった場合はtrue</returns>
    public static bool WaitForBrowserButtonsEnabled(FlaUI.Core.AutomationElements.Window mainWindow, int timeoutMs = 5000)
    {
        var startTime = DateTime.Now;

        while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
        {
            try
            {
                var browserButtons = mainWindow.FindAllDescendants(cf =>
                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                    .And(cf.ByAutomationId("BrowserButton")));

                if (browserButtons.Length > 0)
                {
                    // 少なくとも1つのブラウザボタンが有効かチェック
                    foreach (var button in browserButtons)
                    {
                        if (button.IsEnabled)
                        {
                            Console.WriteLine($"ブラウザボタンが有効になりました: {button.Name}");
                            return true;
                        }
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ブラウザボタン待機エラー: {ex.Message}");
            }
        }

        Console.WriteLine($"ブラウザボタンの有効化タイムアウト: {timeoutMs}ms");
        return false;
    }
}