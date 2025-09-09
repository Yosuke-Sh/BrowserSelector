using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// LogViewerWindow.xaml の相互作用ロジック.
/// </summary>
public partial class LogViewerWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerWindow"/> class.
    /// </summary>
    /// <param name="logContent"></param>
    public LogViewerWindow(string logContent)
    {
        InitializeComponent();
        DataContext = new LogViewerViewModel(logContent);
    }
}

/// <summary>
/// ログビューアーのViewModel.
/// </summary>
public partial class LogViewerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _logContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerViewModel"/> class.
    /// </summary>
    /// <param name="logContent"></param>
    public LogViewerViewModel(string logContent)
    {
        _logContent = logContent;
    }

    /// <summary>
    /// ログを更新するコマンド.
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
            LogViewerWindow newWindow = new(LogContent);
            newWindow.Show();
        }
    }

    /// <summary>
    /// ログをクリップボードにコピーするコマンド.
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        try
        {
            Clipboard.SetText(LogContent);
            _ = LocalizedMessageBox.ShowInformation("Dialog.LogViewer.CopyToClipboardComplete", "MessageBox.Complete");
        }
        catch (Exception ex)
        {
            _ = LocalizedMessageBox.ShowError($"Dialog.LogViewer.CopyToClipboardError: {ex.Message}", "MessageBox.Error");
        }
    }

    /// <summary>
    /// ウィンドウを閉じるコマンド.
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
