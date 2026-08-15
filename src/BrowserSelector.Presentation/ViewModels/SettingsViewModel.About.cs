// <copyright file="SettingsViewModel.About.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> の「一般」タブ最下部のバージョン情報（About）セクション（Phase E-2）関連の
/// partialクラス。1557行に達した<see cref="SettingsViewModel"/>本体の肥大化を避けるため分割した.
/// </summary>
// CA1056: これらはXAML側でTextBlock表示・ToolTip等に直接バインドする文字列であり、
// AppInfo（Core層）と同じ理由でstringのまま公開する。
#pragma warning disable CA1056
public partial class SettingsViewModel
{
    /// <summary>
    /// Gets アプリ名+バージョン表示文字列（例: "BrowserSelector v0.2.0"）。<see cref="AppInfo.CurrentVersion"/>から動的取得する.
    /// </summary>
    public string AppNameVersionDisplay => $"BrowserSelector v{AppInfo.CurrentVersion}";

    /// <summary>
    /// Gets GitHubリポジトリのURL.
    /// </summary>
    public string RepositoryUrl => AppInfo.RepositoryUrl;

    /// <summary>
    /// Gets Issues一覧のURL.
    /// </summary>
    public string IssuesUrl => AppInfo.IssuesUrl;

    /// <summary>
    /// Gets リリース一覧のURL.
    /// </summary>
    public string ReleasesUrl => AppInfo.ReleasesUrl;

    /// <summary>
    /// GitHubリポジトリを開く。<see cref="IExternalLinkService"/>経由で開くため、
    /// 既定ブラウザがBrowserSelector自身であっても自己再帰的に起動しない.
    /// </summary>
    [RelayCommand]
    private async Task OpenRepositoryAsync()
    {
        await OpenExternalLinkAsync(RepositoryUrl).ConfigureAwait(false);
    }

    /// <summary>
    /// Issues一覧を開く.
    /// </summary>
    [RelayCommand]
    private async Task OpenIssuesAsync()
    {
        await OpenExternalLinkAsync(IssuesUrl).ConfigureAwait(false);
    }

    /// <summary>
    /// リリース一覧を開く.
    /// </summary>
    [RelayCommand]
    private async Task OpenReleasesAsync()
    {
        await OpenExternalLinkAsync(ReleasesUrl).ConfigureAwait(false);
    }

    private async Task OpenExternalLinkAsync(string url)
    {
        if (_externalLinkService == null)
        {
            LogService?.LogWarning($"IExternalLinkServiceが未設定のためリンクを開けません: {url}", "SettingsViewModel");
            return;
        }

        try
        {
            bool opened = await _externalLinkService.OpenAsync(url).ConfigureAwait(false);
            if (!opened)
            {
                LogService?.LogWarning($"外部リンクを開けませんでした: {url}", "SettingsViewModel");
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。ブラウザ起動処理は例外種別が多岐にわたり、
        // UIスレッドをクラッシュさせないための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"外部リンクを開く際にエラーが発生しました: {ex.Message}", "SettingsViewModel", ex);
        }
#pragma warning restore CA1031
    }
}
#pragma warning restore CA1056
