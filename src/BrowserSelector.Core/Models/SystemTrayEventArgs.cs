// <copyright file="SystemTrayEventArgs.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// システムトレイイベントの引数.
    /// </summary>
    public class SystemTrayEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTrayEventArgs"/> class.
        /// </summary>
        /// <param name="actionType">アクションタイプ.</param>
        /// <param name="data">追加データ.</param>
        public SystemTrayEventArgs(SystemTrayActionType actionType, object? data = null)
        {
            ActionType = actionType;
            Data = data;
        }

        /// <summary>
        /// Gets アクションタイプ.
        /// </summary>
        public SystemTrayActionType ActionType { get; }

        /// <summary>
        /// Gets 追加データ.
        /// </summary>
        public object? Data { get; }
    }
}
