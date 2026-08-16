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

        /// <summary>
        /// Gets or sets 既定ブラウザへ自動起動するまでのカウントダウン秒数（Phase D）.
        /// 0の場合はカウントダウン自動起動を無効にする. 既定値は5秒.
        /// </summary>
        [ObservableProperty]
        private int _defaultDelay = 5;

        /// <summary>
        /// Gets or sets a value indicating whether ウィンドウを閉じた際にアプリを終了せずシステムトレイへ常駐するか（Phase D）.
        /// trueの場合、✕ボタンでの終了はトレイへの最小化として扱われる.
        /// </summary>
        [ObservableProperty]
        private bool _alwaysResidentInTray;

        /// <summary>
        /// Gets or sets ウィンドウ背景の描画方式（Phase E-1: 外観タブ）.
        /// </summary>
        [ObservableProperty]
        private BackdropMode _backdropMode = BackdropMode.Mica;

        /// <summary>
        /// Gets or sets ウィンドウ全体の不透明度（Phase E-1）。0.3〜1.0の範囲.
        /// </summary>
        [ObservableProperty]
        private double _windowOpacity = 1.0;

        /// <summary>
        /// Gets or sets ウィンドウの角丸半径（Phase E-1、DWM側の丸め対応環境で有効）.
        /// </summary>
        [ObservableProperty]
        private double _windowCornerRadius = 8.0;

        /// <summary>
        /// Gets or sets a value indicating whether カスタムタイトルバーを表示するか（Phase E-1）.
        /// </summary>
        [ObservableProperty]
        private bool _showTitleBar = true;

        /// <summary>
        /// Gets or sets a value indicating whether ウィンドウを常に最前面に表示するか（Phase E-1）.
        /// </summary>
        [ObservableProperty]
        private bool _alwaysOnTop = true;

        /// <summary>
        /// Gets or sets 最後にアップデート確認を行った日時（Phase H-1）.
        /// タイムゾーンをまたいでも間隔判定が壊れないようUTC固定で保存する. 未チェックの場合はnull.
        /// </summary>
        [ObservableProperty]
        private DateTimeOffset? _lastUpdateCheckUtc;

        /// <summary>
        /// Gets or sets ユーザーが「このバージョンをスキップ」を選んだバージョン（Phase H-1）.
        /// このバージョンは再提示しない. 空文字の場合はスキップ指定なし.
        /// </summary>
        [ObservableProperty]
        private string _skippedUpdateVersion = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether プレリリースも更新対象に含めるか（Phase H-1）.
        /// </summary>
        [ObservableProperty]
        private bool _includePrereleases;

        /// <summary>
        /// Gets or sets a value indicating whether ユーザーが「次回起動時に更新」を選んだ状態かどうか（Phase H-1）.
        /// trueの場合、次回起動時はUpdateCheckIntervalを無視して即座に確認する.
        /// </summary>
        [ObservableProperty]
        private bool _updatePendingOnNextLaunch;
    }
}
