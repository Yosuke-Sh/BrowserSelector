using System.Net.Http;
using System.Text.RegularExpressions;
using BrowserSelector.Core.Services;

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

    public async Task<string> NormalizeUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // URLの正規化
        url = url.Trim();

        // プロトコルを追加
        url = AddProtocolIfNeeded(url);

        // 短縮URLを展開（設定で有効な場合）
        var settings = await _settingsService.LoadAppSettingsAsync();
        if (settings.ExpandShortenedUrls)
        {
            url = await ExpandShortenedUrlAsync(url);
        }

        return url;
    }

    public async Task<string> ExpandShortenedUrlAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // 短縮URLのパターンをチェック
            if (IsShortenedUrl(url))
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                request.Headers.Add("User-Agent", "BrowserSelector/1.0");

                using var response = await _httpClient.SendAsync(request);
                if (response.RequestMessage?.RequestUri != null)
                {
                    return response.RequestMessage.RequestUri.ToString();
                }
            }

            return url;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"短縮URL展開エラー: {ex.Message}");
            return url;
        }
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

    private bool IsShortenedUrl(string url)
    {
        // 短縮URLサービスのドメインパターン
        var shortenedUrlPatterns = new[]
        {
            @"bit\.ly",
            @"t\.co",
            @"goo\.gl",
            @"tinyurl\.com",
            @"is\.gd",
            @"v\.gd",
            @"ow\.ly",
            @"su\.pr",
            @"twurl\.nl",
            @"snipurl\.com",
            @"short\.to",
            @"BudURL\.com",
            @"ping\.fm",
            @"tr\.im",
            @"zip\.my",
            @"metamark\.net",
            @"x\.co",
            @"short\.ie",
            @"kl\.am",
            @"wp\.me",
            @"rubyurl\.com",
            @"om\.ly",
            @"to\.ly",
            @"ad\.vu",
            @"bit\.do",
            @"t\.co",
            @"lnkd\.in",
            @"db\.tt",
            @"qr\.ae",
            @"adf\.ly",
            @"goo\.gl",
            @"bitly\.com",
            @"cur\.lv",
            @"tiny\.cc",
            @"ow\.ly",
            @"bit\.ly",
            @"ad\.cr",
            @"ity\.im",
            @"q\.gs",
            @"is\.gd",
            @"po\.st",
            @"bc\.vc",
            @"twitthis\.com",
            @"u\.to",
            @"j\.mp",
            @"buzurl\.com",
            @"cutt\.us",
            @"u\.bb",
            @"yourls\.org",
            @"x\.co",
            @"prettylinkpro\.com",
            @"viralurl\.com",
            @"qr\.net",
            @"1url\.com",
            @"tweez\.me",
            @"v\.gd",
            @"tr\.im",
            @"link\.zip\.net"
        };

        var domain = ExtractDomain(url);
        return shortenedUrlPatterns.Any(pattern => 
            Regex.IsMatch(domain, pattern, RegexOptions.IgnoreCase));
    }
}
