// <copyright file="ProtocolRegistrationInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
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
}
