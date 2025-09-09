using System.Windows;

namespace BrowserSelector.App;

/// <summary>
/// アプリケーションのエントリーポイント
/// </summary>
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            App app = new();
            app.InitializeComponent();
            _ = app.Run();
        }
        catch (Exception ex)
        {
            // テストモードの場合は例外を再スロー
            if (args.Contains("--test-mode") || Environment.GetEnvironmentVariable("BROWSERSELECTOR_TEST_MODE") == "true")
            {
                throw new InvalidOperationException($"アプリケーションの起動に失敗しました: {ex.Message}", ex);
            }
            
            // 通常モードではメッセージボックスを表示
            _ = MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}",
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
