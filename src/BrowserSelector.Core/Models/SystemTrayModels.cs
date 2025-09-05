namespace BrowserSelector.Core.Models;

/// <summary>
/// システムトレイメニューアイテム
/// </summary>
public class SystemTrayMenuItems
{
    public List<SystemTrayMenuItem> Items { get; set; } = new();
}

/// <summary>
/// システムトレイメニューアイテム
/// </summary>
public class SystemTrayMenuItem
{
    public string Text { get; set; } = string.Empty;
    public SystemTrayActionType Action { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsSeparator { get; set; } = false;
    public SystemTrayMenuItems? SubItems { get; set; }
}

/// <summary>
/// システムトレイアクションタイプ
/// </summary>
public enum SystemTrayActionType
{
    None,
    Show,
    Hide,
    Settings,
    Exit,
    Custom
}

/// <summary>
/// システムトレイイベント引数
/// </summary>
public class SystemTrayEventArgs : EventArgs
{
    public SystemTrayActionType ActionType { get; }

    public SystemTrayEventArgs(SystemTrayActionType actionType)
    {
        ActionType = actionType;
    }
}

/// <summary>
/// プロトコル登録情報
/// </summary>
public class ProtocolRegistrationInfo
{
    public string ProtocolName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
}

/// <summary>
/// アップデート情報
/// </summary>
public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string? LocalFilePath { get; set; }
}

/// <summary>
/// アップデート利用可能イベント引数
/// </summary>
public class UpdateAvailableEventArgs : EventArgs
{
    public UpdateInfo UpdateInfo { get; }

    public UpdateAvailableEventArgs(UpdateInfo updateInfo)
    {
        UpdateInfo = updateInfo;
    }
}


