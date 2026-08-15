// <copyright file="AppSettings.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// アプリケーション全体の設定を表すモデル.
    /// </summary>
    public partial class AppSettings : ObservableObject
    {
        [ObservableProperty]
        private bool _enableLogging = true;

        [ObservableProperty]
        private ThemeMode _themeMode = ThemeMode.System;

        [ObservableProperty]
        private string _logLevel = "Information";

        [ObservableProperty]
        private bool _checkForUpdates = true;

        [ObservableProperty]
        private int _updateCheckInterval = 24; // 時間単位

        [ObservableProperty]
        private string _language = "en-US";

        [ObservableProperty]
        private string _customProtocol = "browserselector";

        [ObservableProperty]
        private bool _registerProtocol = true;

        [ObservableProperty]
        private bool _closeAfterUrlRuleMatch = true;
    }
}
