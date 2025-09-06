using BrowserSelector.Core.Services;
using System.Net.Http;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// URL処理サービスの実装
/// </summary>
public class UrlService : IUrlService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly ILogService? _logService;

    public UrlService(ISettingsService settingsService, ILogService? logService = null)
    {
        _settingsService = settingsService;
        _logService = logService;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public Task<string> NormalizeUrlAsync(string url)
    {
        _logService?.LogTrace($"URL正規化処理開始: 入力URL='{url}'", "UrlService");
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logService?.LogWarning("URL正規化: 空のURLが渡されました", "UrlService");
                return Task.FromResult(string.Empty);
            }

            // URLの正規化
            string originalUrl = url;
            url = url.Trim();

            // プロトコルを追加
            url = AddProtocolIfNeeded(url);

            _logService?.LogTrace($"URL正規化処理完了: '{originalUrl}' -> '{url}' (プロトコル追加: {originalUrl != url})", "UrlService");
            return Task.FromResult(url);
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URL正規化エラー: {ex.Message}", "UrlService", ex);
            return Task.FromResult(string.Empty);
        }
    }



    public Task<bool> ValidateUrlAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logService?.LogWarning("URL検証: 空のURLが渡されました", "UrlService");
                return Task.FromResult(false);
            }

            // 基本的なURL形式チェック
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                _logService?.LogWarning($"URL検証失敗: 無効なURL形式 '{url}'", "UrlService");
                return Task.FromResult(false);
            }

            // サポートされているプロトコルかチェック
            bool isValid = uri.Scheme == Uri.UriSchemeHttp ||
                         uri.Scheme == Uri.UriSchemeHttps ||
                         uri.Scheme == Uri.UriSchemeFtp;

            if (isValid)
            {
                _logService?.LogDebug($"URL検証成功: '{url}' (プロトコル: {uri.Scheme})", "UrlService");
            }
            else
            {
                _logService?.LogWarning($"URL検証失敗: サポートされていないプロトコル '{uri.Scheme}' in '{url}'", "UrlService");
            }

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URL検証エラー: {ex.Message}", "UrlService", ex);
            return Task.FromResult(false);
        }
    }

    public string ExtractDomain(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            // プロトコルを追加（必要に応じて）
            url = AddProtocolIfNeeded(url);

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.Host : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string AddProtocolIfNeeded(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        url = url.Trim();

        // 既にプロトコルがある場合はそのまま返す
        if (url.StartsWith("http://") || url.StartsWith("https://") ||
            url.StartsWith("ftp://") || url.StartsWith("file://"))
        {
            return url;
        }

        // プロトコルがない場合はhttps://を追加
        return "https://" + url;
    }


}
