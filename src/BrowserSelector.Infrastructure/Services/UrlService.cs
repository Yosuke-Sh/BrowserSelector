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

    public UrlService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public Task<string> NormalizeUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Task.FromResult(string.Empty);

        // URLの正規化
        url = url.Trim();

        // プロトコルを追加
        url = AddProtocolIfNeeded(url);

        return Task.FromResult(url);
    }



    public Task<bool> ValidateUrlAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return Task.FromResult(false);

            // 基本的なURL形式チェック
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return Task.FromResult(false);

            // サポートされているプロトコルかチェック
            return Task.FromResult(uri.Scheme == Uri.UriSchemeHttp ||
                   uri.Scheme == Uri.UriSchemeHttps ||
                   uri.Scheme == Uri.UriSchemeFtp);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string ExtractDomain(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            // プロトコルを追加（必要に応じて）
            url = AddProtocolIfNeeded(url);

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string AddProtocolIfNeeded(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

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
