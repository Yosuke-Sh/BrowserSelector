using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace BrowserSelector.SecurityTests;

/// <summary>
/// レジストリアクセスのセキュリティテスト.
/// </summary>
public class RegistrySecurityTests
{
    /// <summary>
    /// 有効なレジストリキーの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("HKEY_CURRENT_USER\\Software\\BrowserSelector")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run")]
    [InlineData("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Classes\\http")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Classes\\https")]
    public void RegistryKeyShouldAcceptValidKeys(string validKey)
    {
        // Arrange
        var isValidKey = IsValidRegistryKey(validKey);

        // Act & Assert
        isValidKey.Should().BeTrue($"有効なレジストリキー '{validKey}' は受け入れられるべきです");
    }

    /// <summary>
    /// 危険なレジストリキーの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("HKEY_LOCAL_MACHINE\\SAM")]
    [InlineData("HKEY_LOCAL_MACHINE\\SECURITY")]
    [InlineData("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa")]
    [InlineData("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager")]
    [InlineData("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon")]
    [InlineData("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanServer")]
    [InlineData("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanWorkstation")]
    public void RegistryKeyShouldRejectDangerousKeys(string dangerousKey)
    {
        // Arrange
        var isValidKey = IsValidRegistryKey(dangerousKey);

        // Act & Assert
        isValidKey.Should().BeFalse($"危険なレジストリキー '{dangerousKey}' は拒否されるべきです");
    }

    /// <summary>
    /// 無効なレジストリキーの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("INVALID_HIVE\\Software\\Test")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test<script>alert('XSS')</script>")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test'; DROP TABLE registry; --")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test\0\0\0")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test\n\n\n")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test\t\t\t")]
    [InlineData("HKEY_CURRENT_USER\\Software\\Test\r\r\r")]
    public void RegistryKeyShouldRejectInvalidKeys(string invalidKey)
    {
        // Arrange
        var isValidKey = IsValidRegistryKey(invalidKey);

        // Act & Assert
        isValidKey.Should().BeFalse($"無効なレジストリキー '{invalidKey}' は拒否されるべきです");
    }

    /// <summary>
    /// レジストリ値の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("BrowserSelectorPath")]
    [InlineData("DefaultBrowser")]
    [InlineData("LastUsedBrowser")]
    [InlineData("SettingsVersion")]
    [InlineData("InstallationDate")]
    public void RegistryValueShouldAcceptValidValues(string validValue)
    {
        // Arrange
        var isValidValue = IsValidRegistryValue(validValue);

        // Act & Assert
        isValidValue.Should().BeTrue($"有効なレジストリ値 '{validValue}' は受け入れられるべきです");
    }

    /// <summary>
    /// 危険なレジストリ値の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("Value<script>alert('XSS')</script>")]
    [InlineData("Value'; DROP TABLE registry; --")]
    [InlineData("Value\0\0\0")]
    [InlineData("Value\n\n\n")]
    [InlineData("Value\t\t\t")]
    [InlineData("Value\r\r\r")]
    [InlineData("Value|cmd.exe")]
    [InlineData("Value&format C:")]
    [InlineData("Value;rm -rf /")]
    public void RegistryValueShouldRejectDangerousValues(string dangerousValue)
    {
        // Arrange
        var isValidValue = IsValidRegistryValue(dangerousValue);

        // Act & Assert
        isValidValue.Should().BeFalse($"危険なレジストリ値 '{dangerousValue}' は拒否されるべきです");
    }

    /// <summary>
    /// レジストリデータの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Program Files\\BrowserSelector\\BrowserSelector.exe")]
    [InlineData("https://www.google.com")]
    [InlineData("Chrome")]
    [InlineData("Firefox")]
    [InlineData("Edge")]
    [InlineData("Safari")]
    public void RegistryDataShouldAcceptValidData(string validData)
    {
        // Arrange
        var isValidData = IsValidRegistryData(validData);

        // Act & Assert
        isValidData.Should().BeTrue($"有効なレジストリデータ '{validData}' は受け入れられるべきです");
    }

    /// <summary>
    /// 危険なレジストリデータの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Program Files\\BrowserSelector\\BrowserSelector.exe<script>alert('XSS')</script>")]
    [InlineData("https://www.google.com'; DROP TABLE registry; --")]
    [InlineData("Chrome\0\0\0")]
    [InlineData("Firefox\n\n\n")]
    [InlineData("Edge\t\t\t")]
    [InlineData("Safari\r\r\r")]
    [InlineData("C:\\Program Files\\BrowserSelector\\BrowserSelector.exe|cmd.exe")]
    [InlineData("https://www.google.com&format C:")]
    [InlineData("Chrome;rm -rf /")]
    public void RegistryDataShouldRejectDangerousData(string dangerousData)
    {
        // Arrange
        var isValidData = IsValidRegistryData(dangerousData);

        // Act & Assert
        isValidData.Should().BeFalse($"危険なレジストリデータ '{dangerousData}' は拒否されるべきです");
    }

    /// <summary>
    /// 長すぎるレジストリキーの検証テスト.
    /// </summary>
    [Fact]
    public void RegistryKeyShouldRejectExcessivelyLongKeys()
    {
        // Arrange
        var longKey = "HKEY_CURRENT_USER\\Software\\" + new string('a', 300);

        // Act
        var isValidKey = IsValidRegistryKey(longKey);

        // Assert
        isValidKey.Should().BeFalse("長すぎるレジストリキーは拒否されるべきです");
    }

    /// <summary>
    /// 長すぎるレジストリ値の検証テスト.
    /// </summary>
    [Fact]
    public void RegistryValueShouldRejectExcessivelyLongValues()
    {
        // Arrange
        var longValue = new string('a', 1000);

        // Act
        var isValidValue = IsValidRegistryValue(longValue);

        // Assert
        isValidValue.Should().BeFalse("長すぎるレジストリ値は拒否されるべきです");
    }

    /// <summary>
    /// 長すぎるレジストリデータの検証テスト.
    /// </summary>
    [Fact]
    public void RegistryDataShouldRejectExcessivelyLongData()
    {
        // Arrange
        var longData = new string('a', 10000);

        // Act
        var isValidData = IsValidRegistryData(longData);

        // Assert
        isValidData.Should().BeFalse("長すぎるレジストリデータは拒否されるべきです");
    }

    /// <summary>
    /// レジストリキーが有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidRegistryKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        // 長さ制限チェック
        if (key.Length > 512)
        {
            return false;
        }

        // 有効なレジストリハイブチェック
        var validHives = new[]
        {
            "HKEY_CURRENT_USER",
            "HKEY_LOCAL_MACHINE",
            "HKEY_CLASSES_ROOT",
            "HKEY_USERS",
            "HKEY_CURRENT_CONFIG",
            "HKEY_DYN_DATA"
        };

        var hive = key.Split('\\')[0];
        if (!validHives.Contains(hive))
        {
            return false;
        }

        // 危険なレジストリキーチェック
        var dangerousKeys = new[]
        {
            "HKEY_LOCAL_MACHINE\\SAM",
            "HKEY_LOCAL_MACHINE\\SECURITY",
            "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa",
            "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager",
            "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon",
            "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanServer",
            "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanWorkstation"
        };

        foreach (var dangerousKey in dangerousKeys)
        {
            if (key.StartsWith(dangerousKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // 危険な文字列パターンチェック
        var dangerousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"';.*DROP\s+TABLE",
            @"'.*OR\s+'1'\s*=\s*'1",
            @"[\r\n\t\0]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(key, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// レジストリ値が有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidRegistryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // 長さ制限チェック
        if (value.Length > 1000)
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
            @"[\r\n\t\0]",
            @"[;\|&]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(value, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// レジストリデータが有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidRegistryData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        // 長さ制限チェック
        if (data.Length > 10000)
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
            @"[\r\n\t\0]",
            @"[;\|&]"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(data, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
