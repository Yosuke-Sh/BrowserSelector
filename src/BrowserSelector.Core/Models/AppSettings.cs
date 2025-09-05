using CommunityToolkit.Mvvm.ComponentModel;

namespace BrowserSelector.Core.Models;

/// <summary>
/// アプリケーション全体の設定を表すモデル
/// </summary>
public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _startInSystemTray;

    [ObservableProperty]
    private int _startupDelay;

    [ObservableProperty]
    private string _startupMessage = string.Empty;

    [ObservableProperty]
    private bool _enableLogging = true;

    [ObservableProperty]
    private string _logLevel = "Information";

    [ObservableProperty]
    private bool _checkForUpdates = true;

    [ObservableProperty]
    private int _updateCheckInterval = 24; // 時間単位

    [ObservableProperty]
    private string _language = "ja-JP";

    [ObservableProperty]
    private bool _portableMode;

    [ObservableProperty]
    private string _customProtocol = "browserselector";

    [ObservableProperty]
    private bool _registerProtocol = true;

    [ObservableProperty]
    private bool _closeAfterUrlRuleMatch = false;
}
