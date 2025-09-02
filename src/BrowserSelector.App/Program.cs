using System;
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
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}", 
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
