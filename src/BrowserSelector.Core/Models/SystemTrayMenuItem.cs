// <copyright file="SystemTrayMenuItem.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
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
        public bool IsSeparator { get; set; }

        /// <summary>
        /// サブメニューアイテム.
        /// </summary>
        public SystemTrayMenuItems? SubItems { get; set; }
    }
}
