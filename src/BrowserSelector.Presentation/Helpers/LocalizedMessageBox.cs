using BrowserSelector.Core.Services;
using System.Windows;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 多言語化対応のMessageBoxヘルパークラス.
/// </summary>
public static class LocalizedMessageBox
{
    private static ILocalizationService? _localizationService;

    /// <summary>
    /// ローカライゼーションサービスを設定.
    /// </summary>
    public static void SetLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// 確認ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowConfirm(string message, string? title = null)
    {
        if (IsTestEnvironment())
        {
            return MessageBoxResult.No; // テスト環境ではデフォルトでNoを返す
        }

        string localizedTitle = title ?? _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return ShowCore(message, localizedTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// 情報ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowInformation(string message, string? title = null)
    {
        if (IsTestEnvironment())
        {
            return MessageBoxResult.OK; // テスト環境では何も表示せずOKを返す
        }

        string localizedTitle = title ?? _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return ShowCore(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 警告ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowWarning(string message, string? title = null)
    {
        if (IsTestEnvironment())
        {
            return MessageBoxResult.OK; // テスト環境では何も表示せずOKを返す
        }

        string localizedTitle = title ?? _localizationService?.GetString("MessageBox.Warning") ?? "Warning";
        return ShowCore(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// エラーダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowError(string message, string? title = null)
    {
        if (IsTestEnvironment())
        {
            return MessageBoxResult.OK; // テスト環境では何も表示せずOKを返す
        }

        string localizedTitle = title ?? _localizationService?.GetString("MessageBox.Error") ?? "Error";
        return ShowCore(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// ログクリア確認ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowLogClearConfirm()
    {
        string message = _localizationService?.GetString("MessageBox.LogClearConfirm") ?? "Do you want to clear the log file?";
        string title = _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return ShowCore(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// ログクリア完了ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowLogClearComplete()
    {
        string message = _localizationService?.GetString("MessageBox.LogClearComplete") ?? "Log file has been cleared.";
        string title = _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return ShowCore(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 古いログ削除確認ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowOldLogDeleteConfirm()
    {
        string message = _localizationService?.GetString("MessageBox.OldLogDeleteConfirm") ?? "Do you want to delete old log files?";
        string title = _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return ShowCore(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// 古いログ削除完了ダイアログを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult ShowOldLogDeleteComplete()
    {
        string message = _localizationService?.GetString("MessageBox.OldLogDeleteComplete") ?? "Old log files have been deleted.";
        string title = _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return ShowCore(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 汎用メッセージボックスを表示.
    /// </summary>
    /// <returns></returns>
    public static MessageBoxResult Show(string message, string caption = "情報", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
    {
        if (IsTestEnvironment())
        {
            // テスト環境では何も表示せず、デフォルト値を返す
            return button == MessageBoxButton.YesNo ? MessageBoxResult.No : MessageBoxResult.OK;
        }

        return ShowCore(message, caption, button, icon);
    }

    /// <summary>
    /// アクティブウィンドウ（無ければメインウィンドウ）をOwnerに設定してMessageBoxを表示する。
    /// MainWindowが既定でTopmost=trueになりうるため、Ownerの無いMessageBoxは背面に隠れることがあった。
    /// Ownerが取得できない場合（アプリ起動直後・テスト等）はOwnerなしのオーバーロードにフォールバックする.
    /// </summary>
    private static MessageBoxResult ShowCore(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        Window? owner = ActiveWindowLocator.GetActiveWindow();
        return owner != null
            ? MessageBox.Show(owner, message, caption, button, icon)
            : MessageBox.Show(message, caption, button, icon);
    }

    /// <summary>
    /// テスト環境かどうかを判定する.
    /// </summary>
    private static bool IsTestEnvironment()
    {
        try
        {
            // テスト環境の判定方法
            // 1. デバッガーがアタッチされている
            // 2. 環境変数でテスト実行中であることが示されている
            // 3. アセンブリ名に"Test"が含まれている
            // 4. プロセス名に"test"が含まれている
            // 5. スタックトレースに"xunit"が含まれている
            bool isTest = System.Diagnostics.Debugger.IsAttached ||
                   Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                   System.Reflection.Assembly.GetExecutingAssembly().GetName().Name?.Contains("Test", StringComparison.OrdinalIgnoreCase) == true ||
                   Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") == "true" ||
                   System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpperInvariant().Contains("TEST", StringComparison.OrdinalIgnoreCase) ||
                   IsRunningInTestFramework();

            return isTest;
        }
        // CA1031: リフレクション/環境判定処理はSecurityException等の予測困難な例外を返しうるため、安全側にフォールバックするための意図的な汎用catch。
        #pragma warning disable CA1031
        catch
        {
            // エラーが発生した場合は安全側に倒してテスト環境と判定
            return true;
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// テストフレームワーク内で実行されているかどうかを判定.
    /// </summary>
    private static bool IsRunningInTestFramework()
    {
        try
        {
            System.Diagnostics.StackTrace stackTrace = new();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                System.Diagnostics.StackFrame? frame = stackTrace.GetFrame(i);
                System.Reflection.MethodBase? method = frame?.GetMethod();
                Type? declaringType = method?.DeclaringType;

                if (declaringType != null)
                {
                    string typeName = declaringType.FullName ?? string.Empty;
                    string assemblyName = declaringType.Assembly.GetName().Name ?? string.Empty;

                    // xUnit、NUnit、MSTestなどのテストフレームワークを検出
                    if (typeName.Contains("xunit", StringComparison.OrdinalIgnoreCase) ||
                        typeName.Contains("nunit", StringComparison.OrdinalIgnoreCase) ||
                        typeName.Contains("mstest", StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.Contains("xunit", StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.Contains("nunit", StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.Contains("mstest", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // CA1031: リフレクション/環境判定処理はSecurityException等の予測困難な例外を返しうるため、安全側にフォールバックするための意図的な汎用catch。
        #pragma warning disable CA1031
        catch
        {
            return false;
        }
        #pragma warning restore CA1031
    }
}
