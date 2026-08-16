// <copyright file="ExternalLinkService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.IO;
using System.Reflection;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// <see cref="IExternalLinkService"/> の既定実装（Phase E-2）.
/// <c>Process.Start(url, UseShellExecute = true)</c> は使わず、<see cref="IBrowserService"/> 経由で
/// 検出済みブラウザのうち既定ブラウザを明示的に指定して起動する。
/// 既定ブラウザの実行ファイルパスがBrowserSelector自身（自己再帰）と一致する場合は、
/// 検出済みブラウザ一覧の先頭の実ブラウザへフォールバックする.
/// </summary>
public sealed class ExternalLinkService : IExternalLinkService
{
    private readonly IBrowserService _browserService;
    private readonly ILogService? _logService;
    private readonly Func<string> _currentProcessPathProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLinkService"/> class.
    /// </summary>
    /// <param name="browserService">browserService.</param>
    /// <param name="logService">logService（省略可）.</param>
    public ExternalLinkService(IBrowserService browserService, ILogService? logService = null)
        : this(browserService, logService, static () => Assembly.GetEntryAssembly()?.Location ?? Environment.ProcessPath ?? string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLinkService"/> class.
    /// テスト用に「自プロセスの実行ファイルパス」を差し替え可能にするコンストラクタ.
    /// </summary>
    /// <param name="browserService">browserService.</param>
    /// <param name="logService">logService（省略可）.</param>
    /// <param name="currentProcessPathProvider">自プロセスの実行ファイルパスを返すデリゲート（テスト用）.</param>
    public ExternalLinkService(IBrowserService browserService, ILogService? logService, Func<string> currentProcessPathProvider)
    {
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(currentProcessPathProvider);
        _browserService = browserService;
        _logService = logService;
        _currentProcessPathProvider = currentProcessPathProvider;
    }

    /// <inheritdoc/>
    public async Task<bool> OpenAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            _logService?.LogWarning($"外部リンクを開けません。無効なURL: {url}", nameof(ExternalLinkService));
            return false;
        }

        try
        {
            Browser? target = await ResolveLaunchTargetAsync().ConfigureAwait(false);
            if (target == null)
            {
                _logService?.LogWarning("外部リンクを開けるブラウザが見つかりません", nameof(ExternalLinkService));
                return false;
            }

            return await _browserService.LaunchBrowserAsync(target, uri).ConfigureAwait(false);
        }

        // CA1031: 外部リンクを開く処理はUIから直接呼ばれるため、失敗してもアプリをクラッシュさせない意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"外部リンクを開けませんでした: {ex.Message}", nameof(ExternalLinkService), ex);
            return false;
        }
#pragma warning restore CA1031
    }

    /// <summary>
    /// 起動先のブラウザを決定する。既定ブラウザが自己（BrowserSelector自身）の場合は
    /// 検出済み一覧の先頭の実ブラウザへフォールバックする.
    /// </summary>
    private async Task<Browser?> ResolveLaunchTargetAsync()
    {
        Browser? defaultBrowser = await _browserService.GetDefaultBrowserAsync().ConfigureAwait(false);
        if (defaultBrowser != null && !IsSelf(defaultBrowser))
        {
            return defaultBrowser;
        }

        IEnumerable<Browser> allBrowsers = await _browserService.GetAllBrowsersAsync().ConfigureAwait(false);
        return allBrowsers.FirstOrDefault(b => b.IsEnabled && !IsSelf(b));
    }

    /// <summary>
    /// 指定したブラウザがBrowserSelector自身（自己再帰の原因）かどうかを判定する.
    /// </summary>
    private bool IsSelf(Browser browser)
    {
        if (string.IsNullOrWhiteSpace(browser.ExecutablePath))
        {
            return false;
        }

        string currentPath = _currentProcessPathProvider();
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            return false;
        }

        return string.Equals(
            Path.GetFullPath(browser.ExecutablePath).TrimEnd('\\'),
            Path.GetFullPath(currentPath).TrimEnd('\\'),
            StringComparison.OrdinalIgnoreCase);
    }
}
