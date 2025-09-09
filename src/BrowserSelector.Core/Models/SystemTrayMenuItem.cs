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
        /// Gets or sets メニューアイテムのテキスト.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets メニューアイテムのアクション.
        /// </summary>
        public SystemTrayActionType Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether メニューアイテムが有効かどうか.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether メニューアイテムが表示されるかどうか.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether メニューアイテムがセパレーターかどうか.
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// Gets or sets サブメニューアイテム.
        /// </summary>
        public SystemTrayMenuItems? SubItems { get; set; }
    }
}
