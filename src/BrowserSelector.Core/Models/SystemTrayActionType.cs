// <copyright file="SystemTrayActionType.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
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
        /// カスタムアクション.
        /// </summary>
        Custom,

        /// <summary>
        /// アプリケーションを終了.
        /// </summary>
        Exit,

        /// <summary>
        /// アプリケーションを表示.
        /// </summary>
        Show,

        /// <summary>
        /// アプリケーションを非表示.
        /// </summary>
        Hide,

        /// <summary>
        /// 設定を開く.
        /// </summary>
        OpenSettings,

        /// <summary>
        /// アプリケーションを再起動.
        /// </summary>
        Restart,

        /// <summary>
        /// アップデートをチェック.
        /// </summary>
        CheckUpdate,

        /// <summary>
        /// ログを表示.
        /// </summary>
        ShowLog,

        /// <summary>
        /// ヘルプを表示.
        /// </summary>
        ShowHelp,

        /// <summary>
        /// アプリケーションについて.
        /// </summary>
        About
    }
}
