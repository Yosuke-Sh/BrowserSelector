// <copyright file="DefaultBrowserService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Diagnostics;
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
    private const string UserChoiceKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\UrlAssociations\https\UserChoice";
    private const string ExpectedProgId = "BrowserSelector.https";
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultBrowserService"/> class.
    /// </summary>
    /// <param name="logService">logService（省略可）.</param>
    public DefaultBrowserService(ILogService? logService = null)
    {
        _logService = logService;
    }

    /// <inheritdoc/>
    public bool IsDefaultBrowser()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(UserChoiceKeyPath);
            string? progId = key?.GetValue("ProgId") as string;
            bool isDefault = IsExpectedProgId(progId);
            _logService?.LogDebug($"既定ブラウザ判定: ProgId={progId ?? "(none)"}, IsDefault={isDefault}", nameof(DefaultBrowserService));
            return isDefault;
        }
#pragma warning disable CA1031 // レジストリ読み取りはアクセス権限・キー不存在等の予測困難な例外を返しうるため、未設定として安全側に倒すための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogWarning($"既定ブラウザ判定エラー: {ex.Message}", nameof(DefaultBrowserService));
            return false;
        }
#pragma warning restore CA1031
    }

    /// <inheritdoc/>
    public void OpenDefaultAppsSettings()
    {
        try
        {
            using Process? process = Process.Start(new ProcessStartInfo("ms-settings:defaultapps?registeredAppName=BrowserSelector")
            {
                UseShellExecute = true
            });
        }
#pragma warning disable CA1031 // ms-settings:起動失敗（環境依存のシェル関連付け異常等）はアプリ動作を継続させるための意図的な汎用catch
        catch (Exception ex)
        {
            _logService?.LogError($"既定のアプリ設定画面を開けませんでした: {ex.Message}", nameof(DefaultBrowserService), ex);
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
}
