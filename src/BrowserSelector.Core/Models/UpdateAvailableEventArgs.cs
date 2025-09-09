// <copyright file="UpdateAvailableEventArgs.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// アップデート利用可能イベントの引数.
    /// </summary>
    public class UpdateAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAvailableEventArgs"/> class.
        /// </summary>
        /// <param name="updateInfo">アップデート情報.</param>
        public UpdateAvailableEventArgs(UpdateInfo updateInfo)
        {
            UpdateInfo = updateInfo;
        }

        /// <summary>
        /// Gets アップデート情報.
        /// </summary>
        public UpdateInfo UpdateInfo { get; }
    }
}
