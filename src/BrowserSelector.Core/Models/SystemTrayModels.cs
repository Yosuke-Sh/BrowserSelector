// <copyright file="SystemTrayModels.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// システムトレイメニューアイテム.
    /// </summary>
    public class SystemTrayMenuItems
    {
        /// <summary>
        /// メニューアイテムリスト.
        /// </summary>
        public List<SystemTrayMenuItem> Items { get; set; } =[];
    }

    /// <summary>
    /// システムトレイメニューアイテム.
    /// </summary>
    public class SystemTrayMenuItem
    {
        /// <summary>
        /// メニューアイテムのテキスト.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// メニューアイテムのアクション.
        /// </summary>
        public SystemTrayActionType Action { get; set; }

        /// <summary>
        /// メニューアイテムが有効かどうか.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// メニューアイテムが表示されるかどうか.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// メニューアイテムがセパレーターかどうか.
        /// </summary>
        public bool IsSeparator { get; set; } = false;

        /// <summary>
        /// サブメニューアイテム.
        /// </summary>
        public SystemTrayMenuItems? SubItems { get; set; }
    }

    /// <summary>
    /// システムトレイアクションタイプ.
    /// </summary>
    public enum SystemTrayActionType
    {
        /// <summary>
        /// アクションなし.
        /// </summary>
        None,

        /// <summary>
        /// ウィンドウを表示.
        /// </summary>
        Show,

        /// <summary>
        /// ウィンドウを非表示.
        /// </summary>
        Hide,

        /// <summary>
        /// 設定を開く.
        /// </summary>
        Settings,

        /// <summary>
        /// アプリケーションを終了.
        /// </summary>
        Exit,

        /// <summary>
        /// カスタムアクション.
        /// </summary>
        Custom
    }

    /// <summary>
    /// システムトレイイベント引数.
    /// </summary>
    public class SystemTrayEventArgs : EventArgs
    {
        /// <summary>
        /// アクションタイプ.
        /// </summary>
        public SystemTrayActionType ActionType { get; }

        /// <summary>
        /// SystemTrayEventArgsのインスタンスを初期化.
        /// </summary>
        /// <param name="actionType">アクションタイプ.</param>
        public SystemTrayEventArgs(SystemTrayActionType actionType)
        {
            ActionType = actionType;
        }
    }

    /// <summary>
    /// プロトコル登録情報.
    /// </summary>
    public class ProtocolRegistrationInfo
    {
        /// <summary>
        /// プロトコル名.
        /// </summary>
        public string ProtocolName { get; set; } = string.Empty;

        /// <summary>
        /// プロトコルの説明.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 実行コマンド.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// プロトコルが登録されているかどうか.
        /// </summary>
        public bool IsRegistered { get; set; }
    }

    /// <summary>
    /// アップデート情報.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// バージョン.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// ダウンロードURL.
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// リリースノート.
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// ファイルサイズ.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// チェックサム.
        /// </summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>
        /// リリース日.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// ローカルファイルパス.
        /// </summary>
        public string? LocalFilePath { get; set; }
    }

    /// <summary>
    /// アップデート利用可能イベント引数.
    /// </summary>
    public class UpdateAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// アップデート情報.
        /// </summary>
        public UpdateInfo UpdateInfo { get; }

        /// <summary>
        /// UpdateAvailableEventArgsのインスタンスを初期化.
        /// </summary>
        /// <param name="updateInfo">アップデート情報.</param>
        public UpdateAvailableEventArgs(UpdateInfo updateInfo)
        {
            UpdateInfo = updateInfo;
        }
    }
}
