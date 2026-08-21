// <copyright file="DefaultBrowserService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Diagnostics;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// <see cref="IDefaultBrowserService"/> のWindows実装。
/// <c>HKCU\...\UrlAssociations\https\UserChoice</c>はハッシュ保護されており書き込みできないため、
/// 判定は読み取りのみに限定し、設定はWindowsの「既定のアプリ」画面へ誘導する（<c>ms-settings:</c>）。
/// <c>IApplicationAssociationRegistration(UI)</c>はWindows 11で機能しないため使用しない。
/// アプリ本体は<c>asInvoker</c>で動作しHKLMへ書き込めないため、レジストリ登録自体はインストーラー
/// （<c>PrivilegesRequired=admin</c>）の責務のまま据え置く.
/// </summary>
public class DefaultBrowserService : IDefaultBrowserService
{
    /// <summary>
    /// Windowsの「既定のアプリ」設定画面を開くためのURI（単体テスト用にinternal公開）。
    /// 以前はregisteredAppNameクエリでBrowserSelectorの項目にフォーカスしていたが、
    /// 名前解決に失敗する環境があり、その場合ボタンを押しても何も起きなかったため単純化した.
    /// </summary>
    internal const string DefaultAppsSettingsUri = "ms-settings:defaultapps";

    // 実際のUserChoiceキーパス（Shell\Associations配下）。
    // 過去バージョンでは存在しない`CurrentVersion\Explorer\UrlAssociations`配下を参照しており、
    // OpenSubKeyが常にnullを返すため既定ブラウザ判定が常にfalseになる不具合があった.
    private const string UserChoiceKeyPathPrefix = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\";
    private const string UserChoiceKeyPathSuffix = @"\UserChoice";
    private const string ExpectedProgId = "BrowserSelector.https";

    private readonly ILogService? _logService;
    private readonly IBrowserService? _browserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultBrowserService"/> class.
    /// </summary>
    /// <param name="logService">logService（省略可）.</param>
    /// <param name="browserService">
    /// ProgIdから表示名を解決する際、検出済みブラウザの実行ファイルパスと突き合わせるために使用する（省略可）.
    /// </param>
    public DefaultBrowserService(ILogService? logService = null, IBrowserService? browserService = null)
    {
        _logService = logService;
        _browserService = browserService;
    }

    /// <inheritdoc/>
    public bool IsDefaultBrowser()
    {
        string? progId = ReadUserChoiceProgId("https");
        bool isDefault = IsExpectedProgId(progId);
        _logService?.LogDebug($"既定ブラウザ判定: ProgId={progId ?? "(none)"}, IsDefault={isDefault}", nameof(DefaultBrowserService));
        return isDefault;
    }

    /// <inheritdoc/>
    public string? GetDefaultBrowserDisplayName()
    {
        string? progId = ReadUserChoiceProgId("https") ?? ReadUserChoiceProgId("http");
        if (string.IsNullOrEmpty(progId))
        {
            return null;
        }

        if (IsExpectedProgId(progId))
        {
            return null;
        }

        string? executablePath = ReadProgIdExecutablePath(progId);
        string? matchedName = FindBrowserNameByExecutablePath(executablePath);
        if (!string.IsNullOrEmpty(matchedName))
        {
            return matchedName;
        }

        string? friendlyName = ReadProgIdFriendlyTypeName(progId);
        return !string.IsNullOrEmpty(friendlyName) ? friendlyName : progId;
    }

    /// <inheritdoc/>
    public bool OpenDefaultAppsSettings()
    {
        try
        {
            // registeredAppNameクエリによる項目フォーカスは名前解決に失敗する環境があり、
            // その場合ボタン押下時に何も起きなかった（不具合報告）。単純に一覧トップを開く方式に変更する。
            using Process? process = Process.Start(new ProcessStartInfo(DefaultAppsSettingsUri)
            {
                UseShellExecute = true
            });
            return process != null;
        }
#pragma warning disable CA1031 // ms-settings:起動失敗（環境依存のシェル関連付け異常等）はアプリ動作を継続させるための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogError($"既定のアプリ設定画面を開けませんでした: {ex.Message}", nameof(DefaultBrowserService), ex);
            return false;
        }
#pragma warning restore CA1031
    }

    /// <summary>
    /// UserChoiceキーから読み取ったProgIdがBrowserSelectorのものと一致するかどうかを判定する（純粋関数・単体テスト用）。
    /// 大文字小文字は区別しない.
    /// </summary>
    /// <param name="progId">UserChoiceキーの<c>ProgId</c>値（未設定時はnull）.</param>
    /// <returns>BrowserSelectorが既定として設定されていれば<see langword="true"/>.</returns>
    internal static bool IsExpectedProgId(string? progId)
    {
        return string.Equals(progId, ExpectedProgId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <c>shell\open\command</c>の既定値（例: <c>"C:\...\chrome.exe" --single-argument %1</c>）から
    /// 実行ファイルパスのみを抽出する（純粋関数・単体テスト用）.
    /// </summary>
    /// <param name="command">コマンド文字列.</param>
    /// <returns>実行ファイルパス。抽出できない場合は<see langword="null"/>.</returns>
    internal static string? ExtractExecutablePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        string trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            int closingQuoteIndex = trimmed.IndexOf('"', 1);
            return closingQuoteIndex > 0 ? trimmed[1..closingQuoteIndex] : null;
        }

        int spaceIndex = trimmed.IndexOf(' ', StringComparison.Ordinal);
        return spaceIndex > 0 ? trimmed[..spaceIndex] : trimmed;
    }

    private string? ReadUserChoiceProgId(string scheme)
    {
        try
        {
            string keyPath = UserChoiceKeyPathPrefix + scheme + UserChoiceKeyPathSuffix;
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
            return key?.GetValue("ProgId") as string;
        }
#pragma warning disable CA1031 // レジストリ読み取りはアクセス権限・キー不存在等の予測困難な例外を返しうるため、未設定として安全側に倒すための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogWarning($"既定ブラウザ判定エラー（{scheme}）: {ex.Message}", nameof(DefaultBrowserService));
            return null;
        }
#pragma warning restore CA1031
    }

    private string? ReadProgIdExecutablePath(string progId)
    {
        try
        {
            using RegistryKey? commandKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
            string? command = commandKey?.GetValue(string.Empty) as string;
            return ExtractExecutablePath(command);
        }
#pragma warning disable CA1031 // レジストリ読み取り失敗時はフレンドリ名フォールバックへ進めるための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogWarning($"ProgIdの実行ファイルパス取得エラー: {ex.Message}", nameof(DefaultBrowserService));
            return null;
        }
#pragma warning restore CA1031
    }

    private string? ReadProgIdFriendlyTypeName(string progId)
    {
        try
        {
            using RegistryKey? progIdKey = Registry.ClassesRoot.OpenSubKey(progId);
            return progIdKey?.GetValue("FriendlyTypeName") as string;
        }
#pragma warning disable CA1031 // レジストリ読み取り失敗時はProgIdそのものへフォールバックするための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogWarning($"ProgIdのFriendlyTypeName取得エラー: {ex.Message}", nameof(DefaultBrowserService));
            return null;
        }
#pragma warning restore CA1031
    }

    private string? FindBrowserNameByExecutablePath(string? executablePath)
    {
        if (string.IsNullOrEmpty(executablePath) || _browserService == null)
        {
            return null;
        }

        try
        {
            IEnumerable<Browser> browsers = _browserService.GetAllBrowsersAsync().GetAwaiter().GetResult();
            Browser? matched = browsers.FirstOrDefault(b =>
                string.Equals(b.ExecutablePath, executablePath, StringComparison.OrdinalIgnoreCase));
            return matched?.Name;
        }
#pragma warning disable CA1031 // 検出済みブラウザ取得失敗時はフレンドリ名フォールバックへ進めるための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogWarning($"検出済みブラウザとの突き合わせエラー: {ex.Message}", nameof(DefaultBrowserService));
            return null;
        }
#pragma warning restore CA1031
    }
}
