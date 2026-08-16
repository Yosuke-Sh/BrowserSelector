// <copyright file="IExternalLinkService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Services;

/// <summary>
/// アプリ内から外部URL（GitHubリポジトリ、Issues、ライセンス等）を開くためのサービス（Phase E-2）.
/// <c>Process.Start(url, UseShellExecute = true)</c> を直接使ってはいけない。
/// BrowserSelector自身が既定ブラウザや <c>browser://</c> ハンドラとして登録されている場合、
/// 自分自身が再帰的に起動してしまうため、検出済みブラウザのうち既定ブラウザを明示的に指定して起動する.
/// </summary>
public interface IExternalLinkService
{
    // CA1054: IBrowserService.LaunchBrowserAsync(Browser, string)と同じ理由で、呼び出し側の利便性のためstringのまま受け取る
    // （Uri.TryCreateでの検証はOpenAsync内部で行い、無効な形式は false を返す）。
#pragma warning disable CA1054
    /// <summary>
    /// 指定したURLを外部ブラウザで開く。
    /// 既定ブラウザがBrowserSelector自身に設定されている場合は、検出済みブラウザ一覧の先頭の実ブラウザへフォールバックする.
    /// 開けるブラウザが1つも見つからない場合は何もしない（例外はスローしない）.
    /// </summary>
    /// <param name="url">開くURL.</param>
    /// <returns>起動に成功した場合<see langword="true"/>.</returns>
    Task<bool> OpenAsync(string url);
#pragma warning restore CA1054
}
