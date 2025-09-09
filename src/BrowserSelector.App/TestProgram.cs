using System.Windows;

namespace BrowserSelector.App;

/// <summary>
/// テスト用のエントリーポイント
/// </summary>
public class TestProgram
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // テスト用の設定でアプリケーションを起動
            App app = new();
            app.InitializeComponent();
            _ = app.Run();
        }
        catch (Exception ex)
        {
            // テスト環境では例外を再スロー
            throw new InvalidOperationException($"テスト用アプリケーションの起動に失敗しました: {ex.Message}", ex);
        }
    }
}

