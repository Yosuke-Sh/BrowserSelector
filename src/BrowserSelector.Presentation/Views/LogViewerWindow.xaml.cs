using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// LogViewerWindow.xaml の相互作用ロジック
/// </summary>
public partial class LogViewerWindow : Window
{
    public LogViewerWindow(string logContent)
    {
        InitializeComponent();
        DataContext = new LogViewerViewModel(logContent);
    }
}

/// <summary>
/// ログビューアーのViewModel
/// </summary>
public partial class LogViewerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _logContent;

    public LogViewerViewModel(string logContent)
    {
        _logContent = logContent;
    }

    /// <summary>
    /// ログを更新するコマンド
    /// </summary>
    [RelayCommand]
    private void RefreshLogs()
    {
        // 現在のウィンドウを取得してログ内容を更新
        if (Application.Current.Windows.OfType<LogViewerWindow>().FirstOrDefault(w => w.DataContext == this) is LogViewerWindow window)
        {
            // ログ内容の更新処理（必要に応じて実装）
            // ここでは単純にウィンドウを再読み込み
            window.Close();
            var newWindow = new LogViewerWindow(LogContent);
            newWindow.Show();
        }
    }

    /// <summary>
    /// ログをクリップボードにコピーするコマンド
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        try
        {
            Clipboard.SetText(LogContent);
            MessageBox.Show("ログ内容をクリップボードにコピーしました。", "完了", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"クリップボードへのコピーに失敗しました: {ex.Message}", "エラー", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// ウィンドウを閉じるコマンド
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        if (Application.Current.Windows.OfType<LogViewerWindow>().FirstOrDefault(w => w.DataContext == this) is LogViewerWindow window)
        {
            window.Close();
        }
    }
}
