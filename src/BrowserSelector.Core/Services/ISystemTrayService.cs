using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// システムトレイ機能を提供するサービスのインターフェース
/// </summary>
public interface ISystemTrayService
{
    /// <summary>
    /// システムトレイアクションが発生した時のイベント
    /// </summary>
    event EventHandler<SystemTrayEventArgs>? SystemTrayAction;

    /// <summary>
    /// システムトレイアイコンを初期化
    /// </summary>
    /// <param name="iconPath">アイコンファイルのパス</param>
    /// <param name="tooltipText">ツールチップテキスト</param>
    void InitializeSystemTray(string iconPath, string tooltipText);

    /// <summary>
    /// システムトレイアイコンを表示
    /// </summary>
    void ShowSystemTray();

    /// <summary>
    /// システムトレイアイコンを非表示
    /// </summary>
    void HideSystemTray();

    /// <summary>
    /// バルーンティップを表示
    /// </summary>
    /// <param name="title">タイトル</param>
    /// <param name="text">テキスト</param>
    /// <param name="icon">アイコンタイプ</param>
    /// <param name="timeout">表示時間（ミリ秒）</param>
    void ShowBalloonTip(string title, string text, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info, int timeout = 3000);

    /// <summary>
    /// コンテキストメニューを更新
    /// </summary>
    /// <param name="menuItems">メニューアイテム</param>
    void UpdateContextMenu(SystemTrayMenuItems menuItems);
}


