// <copyright file="LogSettings.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// ログ設定を表すモデル.
    /// </summary>
    public partial class LogSettings : ObservableObject
    {
        /// <summary>
        /// ログを有効にするかどうか.
        /// </summary>
        [ObservableProperty]
        private bool _enableLogging = true;

        /// <summary>
        /// ログレベル.
        /// </summary>
        [ObservableProperty]
        private LogLevel _logLevel = LogLevel.Information;

        /// <summary>
        /// ログファイルの出力先フォルダ.
        /// </summary>
        [ObservableProperty]
        private string _logOutputFolder = string.Empty;

        /// <summary>
        /// ログファイルの最大サイズ（MB）.
        /// </summary>
        [ObservableProperty]
        private int _maxLogFileSize = 10;

        /// <summary>
        /// ログファイルの保持期間（日数）.
        /// </summary>
        [ObservableProperty]
        private int _logRetentionDays = 30;

        /// <summary>
        /// コンソールにログを出力するかどうか.
        /// </summary>
        [ObservableProperty]
        private bool _enableConsoleLogging = true;

        /// <summary>
        /// ファイルにログを出力するかどうか.
        /// </summary>
        [ObservableProperty]
        private bool _enableFileLogging = true;

        /// <summary>
        /// ログファイル名のプレフィックス.
        /// </summary>
        [ObservableProperty]
        private string _logFilePrefix = "BrowserSelector";

        /// <summary>
        /// ログファイル名のサフィックス.
        /// </summary>
        [ObservableProperty]
        private string _logFileSuffix = "log";

        /// <summary>
        /// ログメッセージのタイムスタンプ形式.
        /// </summary>
        [ObservableProperty]
        private string _timestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// ログメッセージのテンプレート.
        /// </summary>
        [ObservableProperty]
        private string _logMessageTemplate = "[{Timestamp}] [{Level}] [{EventId}] [{Category}] {RequestTarget} {UserInfo} {ProcessTarget} {ProcessAction} {ProcessResult} {Message}";

        /// <summary>
        /// デフォルトのログ出力フォルダを取得.
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultLogFolder()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "BrowserSelector", "Logs");
        }

        /// <summary>
        /// ログファイルの完全パスを取得.
        /// </summary>
        /// <returns></returns>
        public string GetLogFilePath()
        {
            if (string.IsNullOrEmpty(LogOutputFolder))
            {
                LogOutputFolder = GetDefaultLogFolder();
            }

            string fileName = $"{LogFilePrefix}_{DateTime.Now:yyyyMMdd}.{LogFileSuffix}";
            return Path.Combine(LogOutputFolder, fileName);
        }
    }
}
