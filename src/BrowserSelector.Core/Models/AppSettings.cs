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

        /// <summary>
        /// Gets or sets a value indicating whether DWMによるガラス効果（Mica/Acrylic）を有効にするか.
        /// falseの場合、<see cref="Helpers.WindowBackdropHelper"/> は半透明単色ブラシへフォールバックする.
        /// </summary>
        [ObservableProperty]
        private bool _enableGlassEffect = true;

        /// <summary>
        /// Gets or sets a value indicating whether ホバー・フォーカス等のUIアニメーション（拡大・影の遷移）を有効にするか.
        /// Windowsの「アニメーションを表示する」設定と併用し、falseの場合は影・スケール変化を全て無効化する.
        /// </summary>
        [ObservableProperty]
        private bool _enableAnimations = true;
    }
}
