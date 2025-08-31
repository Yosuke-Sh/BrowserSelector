using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// Windowsレジストリからブラウザ情報を取得するサービスのインターフェース
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// レジストリからブラウザを検出
    /// </summary>
    Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync();
}

