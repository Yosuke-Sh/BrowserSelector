// <copyright file="SettingsViewModel.Appearance.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Enums;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> の「外観」タブ（Phase E-1）関連のpartialクラス.
/// バックドロップ方式・不透明度・角丸半径・タイトルバー表示・常に最前面・テーマの選択肢を提供する。
/// 実際の値は<see cref="SettingsViewModel.AppSettings"/>に直接バインドされ、ここでは選択肢（コンボボックス用の
/// 列挙値一覧）のみを公開する。1557行に達した<see cref="SettingsViewModel"/>本体の肥大化を避けるため分割した.
/// </summary>
public partial class SettingsViewModel
{
    /// <summary>
    /// Gets 外観タブの「バックドロップ方式」コンボボックスに表示する選択肢一覧.
    /// </summary>
    public IReadOnlyList<BackdropMode> AvailableBackdropModes { get; } =
        Enum.GetValues<BackdropMode>();

    /// <summary>
    /// Gets 外観タブの「テーマ」コンボボックスに表示する選択肢一覧（ライト/ダーク/システム追従）.
    /// </summary>
    public IReadOnlyList<ThemeMode> AvailableThemeModes { get; } =
        Enum.GetValues<ThemeMode>();
}
