// <copyright file="SettingsViewModel.Startup.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> の起動制御系設定（Phase D: カウントダウン秒数・トレイ常駐・起動後に閉じる）
/// 関連のpartialクラス。1557行に達した<see cref="SettingsViewModel"/>本体の肥大化を避けるため分割した。
/// 値そのものは<see cref="SettingsViewModel.AppSettings"/>（<c>DefaultDelay</c>/<c>AlwaysResidentInTray</c>/
/// <c>CloseAfterUrlRuleMatch</c>）へ直接バインドされるため、このpartialクラスは将来の検証ロジック追加のための置き場である.
/// </summary>
public partial class SettingsViewModel
{
    /// <summary>
    /// カウントダウン遅延秒数として有効な最小値（0=無効）.
    /// </summary>
    public const int MinCountdownDelaySeconds = 0;

    /// <summary>
    /// カウントダウン遅延秒数として有効な最大値.
    /// </summary>
    public const int MaxCountdownDelaySeconds = 3600;

    /// <summary>
    /// カウントダウン遅延秒数が有効範囲内かどうかを検証する.
    /// </summary>
    /// <param name="delaySeconds">検証する秒数.</param>
    /// <returns>有効範囲内の場合<see langword="true"/>.</returns>
    public static bool IsValidCountdownDelay(int delaySeconds)
    {
        return delaySeconds >= MinCountdownDelaySeconds && delaySeconds <= MaxCountdownDelaySeconds;
    }
}
