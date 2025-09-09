using FluentAssertions;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace BrowserSelector.SecurityTests;

/// <summary>
/// 入力検証のセキュリティテスト.
/// </summary>
public class InputValidationSecurityTests
{
    /// <summary>
    /// URL入力のXSS攻撃検証テスト.
    /// </summary>
    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("data:text/html,<script>alert('XSS')</script>")]
    [InlineData("vbscript:msgbox('XSS')")]
    [InlineData("onload=alert('XSS')")]
    [InlineData("onerror=alert('XSS')")]
    [InlineData("onclick=alert('XSS')")]
    [InlineData("onmouseover=alert('XSS')")]
    [InlineData("onfocus=alert('XSS')")]
    [InlineData("onblur=alert('XSS')")]
    public void UrlInputShouldRejectXSSAttacks(string maliciousUrl)
    {
        // Arrange
        var isValidUrl = IsValidUrl(maliciousUrl);

        // Act & Assert
        isValidUrl.Should().BeFalse($"悪意のあるURL '{maliciousUrl}' は拒否されるべきです");
    }

    /// <summary>
    /// URL入力のXSS攻撃検証テスト（Uri版）.
    /// </summary>
    /// <param name="maliciousUrl">maliciousUrl.</param>
    public void UrlInputShouldRejectXSSAttacksUri(Uri maliciousUrl)
    {
        UrlInputShouldRejectXSSAttacks(maliciousUrl.ToString());
    }

    /// <summary>
    /// URL入力のSQLインジェクション攻撃検証テスト.
    /// </summary>
    [Theory]
    [InlineData("'; DROP TABLE users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("' UNION SELECT * FROM users --")]
    [InlineData("'; INSERT INTO users VALUES ('hacker', 'password'); --")]
    [InlineData("' OR 1=1 --")]
    [InlineData("'; DELETE FROM users WHERE '1'='1")]
    public void UrlInputShouldRejectSQLInjectionAttacks(string maliciousUrl)
    {
        // Arrange
        var isValidUrl = IsValidUrl(maliciousUrl);

        // Act & Assert
        isValidUrl.Should().BeFalse($"SQLインジェクション攻撃URL '{maliciousUrl}' は拒否されるべきです");
    }

    /// <summary>
    /// URL入力のSQLインジェクション攻撃検証テスト（Uri版）.
    /// </summary>
    /// <param name="maliciousUrl">maliciousUrl.</param>
    public void UrlInputShouldRejectSQLInjectionAttacksUri(Uri maliciousUrl)
    {
        UrlInputShouldRejectSQLInjectionAttacks(maliciousUrl.ToString());
    }

    /// <summary>
    /// URL入力のパストラバーサル攻撃検証テスト.
    /// </summary>
    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("file:///etc/passwd")]
    [InlineData("file:///C:/Windows/System32/config/SAM")]
    [InlineData("\\\\server\\share\\..\\..\\windows\\system32")]
    public void UrlInputShouldRejectPathTraversalAttacks(string maliciousUrl)
    {
        // Arrange
        var isValidUrl = IsValidUrl(maliciousUrl);

        // Act & Assert
        isValidUrl.Should().BeFalse($"パストラバーサル攻撃URL '{maliciousUrl}' は拒否されるべきです");
    }

    /// <summary>
    /// URL入力のパストラバーサル攻撃検証テスト（Uri版）.
    /// </summary>
    /// <param name="maliciousUrl">maliciousUrl.</param>
    public void UrlInputShouldRejectPathTraversalAttacksUri(Uri maliciousUrl)
    {
        UrlInputShouldRejectPathTraversalAttacks(maliciousUrl.ToString());
    }

    /// <summary>
    /// URL入力のコマンドインジェクション攻撃検証テスト.
    /// </summary>
    /// <param name="maliciousUrl">maliciousUrl.</param>
    [Theory]
    [InlineData("https://example.com; rm -rf /")]
    [InlineData("https://example.com | del /f /q C:\\")]
    [InlineData("https://example.com && format C:")]
    [InlineData("https://example.com; cat /etc/passwd")]
    [InlineData("https://example.com | type C:\\Windows\\System32\\drivers\\etc\\hosts")]
    public void UrlInputShouldRejectCommandInjectionAttacks(string maliciousUrl)
    {
        // Arrange
        var isValidUrl = IsValidUrl(maliciousUrl);

        // Act & Assert
        isValidUrl.Should().BeFalse($"コマンドインジェクション攻撃URL '{maliciousUrl}' は拒否されるべきです");
    }

    /// <summary>
    /// URL入力のコマンドインジェクション攻撃検証テスト（Uri版）.
    /// </summary>
    /// <param name="maliciousUrl">maliciousUrl.</param>
    public void UrlInputShouldRejectCommandInjectionAttacksUri(Uri maliciousUrl)
    {
        UrlInputShouldRejectCommandInjectionAttacks(maliciousUrl.ToString());
    }

    /// <summary>
    /// 有効なURLの検証テスト.
    /// </summary>
    /// <param name="validUrl">validUrl.</param>
    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("http://www.microsoft.com")]
    [InlineData("https://github.com")]
    [InlineData("https://stackoverflow.com")]
    [InlineData("https://www.wikipedia.org")]
    [InlineData("https://example.com/path/to/page")]
    [InlineData("https://example.com:8080/path")]
    [InlineData("https://user:pass@example.com")]
    [InlineData("https://example.com/path?param=value")]
    [InlineData("https://example.com/path#fragment")]
    public void UrlInputShouldAcceptValidUrls(string validUrl)
    {
        // Arrange
        var isValidUrl = IsValidUrl(validUrl);

        // Act & Assert
        isValidUrl.Should().BeTrue($"有効なURL '{validUrl}' は受け入れられるべきです");
    }

    /// <summary>
    /// 有効なURLの検証テスト（Uri版）.
    /// </summary>
    /// <param name="validUrl">validUrl.</param>
    public void UrlInputShouldAcceptValidUrlsUri(Uri validUrl)
    {
        UrlInputShouldAcceptValidUrls(validUrl.ToString());
    }

    /// <summary>
    /// 長すぎるURLの検証テスト.
    /// </summary>
    [Fact]
    public void UrlInputShouldRejectExcessivelyLongUrls()
    {
        // Arrange
        var longUrl = "https://example.com/" + new string('a', 10000);

        // Act
        var isValidUrl = IsValidUrl(longUrl);

        // Assert
        isValidUrl.Should().BeFalse("長すぎるURLは拒否されるべきです");
    }

    /// <summary>
    /// 特殊文字を含むURLの検証テスト.
    /// </summary>
    /// <param name="urlWithSpecialChars">urlWithSpecialChars.</param>
    [Theory]
    [InlineData("https://example.com/path with spaces")]
    [InlineData("https://example.com/path\twith\ttabs")]
    [InlineData("https://example.com/path\nwith\nnewlines")]
    [InlineData("https://example.com/path\rwith\rcarriage")]
    [InlineData("https://example.com/path\0with\0null")]
    public void UrlInputShouldRejectUrlsWithSpecialCharacters(string urlWithSpecialChars)
    {
        // Arrange
        var isValidUrl = IsValidUrl(urlWithSpecialChars);

        // Act & Assert
        isValidUrl.Should().BeFalse($"特殊文字を含むURL '{urlWithSpecialChars}' は拒否されるべきです");
    }

    /// <summary>
    /// 特殊文字を含むURLの検証テスト（Uri版）.
    /// </summary>
    /// <param name="urlWithSpecialChars">urlWithSpecialChars.</param>
    public void UrlInputShouldRejectUrlsWithSpecialCharactersUri(Uri urlWithSpecialChars)
    {
        UrlInputShouldRejectUrlsWithSpecialCharacters(urlWithSpecialChars.ToString());
    }

    /// <summary>
    /// ブラウザ名入力の検証テスト.
    /// </summary>
    /// <param name="browserName">browserName.</param>
    [Theory]
    [InlineData("Chrome")]
    [InlineData("Firefox")]
    [InlineData("Edge")]
    [InlineData("Safari")]
    [InlineData("Opera")]
    [InlineData("Brave")]
    public void BrowserNameInputShouldAcceptValidNames(string browserName)
    {
        // Arrange
        var isValidName = IsValidBrowserName(browserName);

        // Act & Assert
        isValidName.Should().BeTrue($"有効なブラウザ名 '{browserName}' は受け入れられるべきです");
    }

    /// <summary>
    /// ブラウザ名入力の悪意のある文字列検証テスト.
    /// </summary>
    /// <param name="maliciousName">maliciousName.</param>
    [Theory]
    [InlineData("Chrome<script>alert('XSS')</script>")]
    [InlineData("Firefox'; DROP TABLE browsers; --")]
    [InlineData("Edge\0\0\0")]
    [InlineData("Safari\n\n\n")]
    [InlineData("Opera\t\t\t")]
    [InlineData("Brave\r\r\r")]
    public void BrowserNameInputShouldRejectMaliciousNames(string maliciousName)
    {
        // Arrange
        var isValidName = IsValidBrowserName(maliciousName);

        // Act & Assert
        isValidName.Should().BeFalse($"悪意のあるブラウザ名 '{maliciousName}' は拒否されるべきです");
    }

    /// <summary>
    /// 設定値入力の検証テスト.
    /// </summary>
    /// <param name="settingValue">settingValue.</param>
    [Theory]
    [InlineData("normal_setting_value")]
    [InlineData("setting-with-dashes")]
    [InlineData("setting_with_underscores")]
    [InlineData("setting.with.dots")]
    [InlineData("setting123with456numbers")]
    public void SettingValueInputShouldAcceptValidValues(string settingValue)
    {
        // Arrange
        var isValidValue = IsValidSettingValue(settingValue);

        // Act & Assert
        isValidValue.Should().BeTrue($"有効な設定値 '{settingValue}' は受け入れられるべきです");
    }

    /// <summary>
    /// 設定値入力の悪意のある文字列検証テスト.
    /// </summary>
    /// <param name="maliciousValue">maliciousValue.</param>
    [Theory]
    [InlineData("setting<script>alert('XSS')</script>")]
    [InlineData("setting'; DROP TABLE settings; --")]
    [InlineData("setting\0\0\0")]
    [InlineData("setting\n\n\n")]
    [InlineData("setting\t\t\t")]
    [InlineData("setting\r\r\r")]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32")]
    public void SettingValueInputShouldRejectMaliciousValues(string maliciousValue)
    {
        // Arrange
        var isValidValue = IsValidSettingValue(maliciousValue);

        // Act & Assert
        isValidValue.Should().BeFalse($"悪意のある設定値 '{maliciousValue}' は拒否されるべきです");
    }

    /// <summary>
    /// URLが有効かどうかを検証するメソッド.
    /// </summary>
    /// <param name="url">url.</param>
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // 長さ制限チェック
        if (url.Length > 2048)
        {
            return false;
        }

        // 危険な文字列パターンチェック
        var dangerousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"data:text/html",
            @"on\w+\s*=",
            @"';.*DROP\s+TABLE",
            @"'.*OR\s+'1'\s*=\s*'1",
            @"'.*UNION\s+SELECT",
            @"\.\.\/",
            @"\.\.\\",
            @"file:\/\/",
            @"\\\\",
            @"[;\|&]",
            @"[\r\n\t\0]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        // 基本的なURL形式チェック
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// ブラウザ名が有効かどうかを検証するメソッド.
    /// </summary>
    /// <param name="name">name.</param>
    private static bool IsValidBrowserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        // 長さ制限チェック
        if (name.Length > 100)
        {
            return false;
        }

        // 危険な文字列パターンチェック
        var dangerousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"';.*DROP\s+TABLE",
            @"'.*OR\s+'1'\s*=\s*'1",
            @"\.\.\/",
            @"\.\.\\",
            @"[\r\n\t\0]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        // 英数字、ハイフン、アンダースコア、ドットのみ許可
        return Regex.IsMatch(name, @"^[a-zA-Z0-9\-_.]+$");
    }

    /// <summary>
    /// 設定値が有効かどうかを検証するメソッド.
    /// </summary>
    /// <param name="value">value.</param>
    private static bool IsValidSettingValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // 長さ制限チェック
        if (value.Length > 500)
        {
            return false;
        }

        // 危険な文字列パターンチェック
        var dangerousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"';.*DROP\s+TABLE",
            @"'.*OR\s+'1'\s*=\s*'1",
            @"\.\.\/",
            @"\.\.\\",
            @"[\r\n\t\0]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
