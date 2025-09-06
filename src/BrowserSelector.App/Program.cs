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
            _ = MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}",
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
