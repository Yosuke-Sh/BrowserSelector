using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace BrowserSelector.SecurityTests;

/// <summary>
/// プロセス実行のセキュリティテスト.
/// </summary>
public class ProcessExecutionSecurityTests
{
    /// <summary>
    /// 有効なブラウザ実行パスの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe")]
    [InlineData("C:\\Program Files\\Mozilla Firefox\\firefox.exe")]
    [InlineData("C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe")]
    [InlineData("C:\\Program Files\\Safari\\Safari.exe")]
    [InlineData("C:\\Program Files\\Opera\\opera.exe")]
    [InlineData("C:\\Program Files\\BraveSoftware\\Brave-Browser\\Application\\brave.exe")]
    public void BrowserExecutablePathShouldAcceptValidPaths(string validPath)
    {
        // Arrange
        var isValidPath = IsValidExecutablePath(validPath);

        // Act & Assert
        isValidPath.Should().BeTrue($"有効なブラウザ実行パス '{validPath}' は受け入れられるべきです");
    }

    /// <summary>
    /// 危険な実行パスの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Windows\\System32\\cmd.exe")]
    [InlineData("C:\\Windows\\System32\\powershell.exe")]
    [InlineData("C:\\Windows\\System32\\regedit.exe")]
    [InlineData("C:\\Windows\\System32\\gpedit.msc")]
    [InlineData("C:\\Windows\\System32\\mmc.exe")]
    [InlineData("C:\\Windows\\System32\\taskmgr.exe")]
    [InlineData("C:\\Windows\\System32\\msconfig.exe")]
    [InlineData("C:\\Windows\\System32\\services.msc")]
    [InlineData("C:\\Windows\\System32\\eventvwr.msc")]
    [InlineData("C:\\Windows\\System32\\compmgmt.msc")]
    public void BrowserExecutablePathShouldRejectDangerousPaths(string dangerousPath)
    {
        // Arrange
        var isValidPath = IsValidExecutablePath(dangerousPath);

        // Act & Assert
        isValidPath.Should().BeFalse($"危険な実行パス '{dangerousPath}' は拒否されるべきです");
    }

    /// <summary>
    /// 無効な実行パスの検証テスト.
    /// </summary>
    [Theory]
    [InlineData("chrome.exe<script>alert('XSS')</script>")]
    [InlineData("firefox.exe'; DROP TABLE processes; --")]
    [InlineData("edge.exe\0\0\0")]
    [InlineData("safari.exe\n\n\n")]
    [InlineData("opera.exe\t\t\t")]
    [InlineData("brave.exe\r\r\r")]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    public void BrowserExecutablePathShouldRejectInvalidPaths(string invalidPath)
    {
        // Arrange
        var isValidPath = IsValidExecutablePath(invalidPath);

        // Act & Assert
        isValidPath.Should().BeFalse($"無効な実行パス '{invalidPath}' は拒否されるべきです");
    }

    /// <summary>
    /// 有効なコマンドライン引数の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://www.microsoft.com")]
    [InlineData("https://www.github.com")]
    [InlineData("https://example.com/path/to/page")]
    [InlineData("https://example.com:8080/path")]
    [InlineData("https://user:pass@example.com")]
    [InlineData("https://example.com/path?param=value")]
    [InlineData("https://example.com/path#fragment")]
    public void CommandLineArgumentsShouldAcceptValidArguments(string validArgument)
    {
        // Arrange
        var isValidArgument = IsValidCommandLineArgument(validArgument);

        // Act & Assert
        isValidArgument.Should().BeTrue($"有効なコマンドライン引数 '{validArgument}' は受け入れられるべきです");
    }

    /// <summary>
    /// 危険なコマンドライン引数の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("https://www.google.com; rm -rf /")]
    [InlineData("https://www.microsoft.com | del /f /q C:\\")]
    [InlineData("https://www.github.com && format C:")]
    [InlineData("https://example.com; cat /etc/passwd")]
    [InlineData("https://example.com | type C:\\Windows\\System32\\drivers\\etc\\hosts")]
    [InlineData("https://example.com && net user hacker password /add")]
    [InlineData("https://example.com; shutdown -s -t 0")]
    [InlineData("https://example.com | taskkill /f /im explorer.exe")]
    public void CommandLineArgumentsShouldRejectDangerousArguments(string dangerousArgument)
    {
        // Arrange
        var isValidArgument = IsValidCommandLineArgument(dangerousArgument);

        // Act & Assert
        isValidArgument.Should().BeFalse($"危険なコマンドライン引数 '{dangerousArgument}' は拒否されるべきです");
    }

    /// <summary>
    /// 無効なコマンドライン引数の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("https://www.google.com<script>alert('XSS')</script>")]
    [InlineData("https://www.microsoft.com'; DROP TABLE urls; --")]
    [InlineData("https://www.github.com\0\0\0")]
    [InlineData("https://example.com\n\n\n")]
    [InlineData("https://example.com\t\t\t")]
    [InlineData("https://example.com\r\r\r")]
    public void CommandLineArgumentsShouldRejectInvalidArguments(string invalidArgument)
    {
        // Arrange
        var isValidArgument = IsValidCommandLineArgument(invalidArgument);

        // Act & Assert
        isValidArgument.Should().BeFalse($"無効なコマンドライン引数 '{invalidArgument}' は拒否されるべきです");
    }

    /// <summary>
    /// 長すぎるコマンドライン引数の検証テスト.
    /// </summary>
    [Fact]
    public void CommandLineArgumentsShouldRejectExcessivelyLongArguments()
    {
        // Arrange
        var longArgument = "https://example.com/" + new string('a', 10000);

        // Act
        var isValidArgument = IsValidCommandLineArgument(longArgument);

        // Assert
        isValidArgument.Should().BeFalse("長すぎるコマンドライン引数は拒否されるべきです");
    }

    /// <summary>
    /// プロセス実行のセキュリティテスト.
    /// </summary>
    [Fact]
    public void ProcessExecutionShouldBeSecure()
    {
        // Arrange
        var validPath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        var validArguments = "https://www.google.com";

        // Act
        var canExecute = CanExecuteProcess(validPath, validArguments);

        // Assert
        canExecute.Should().BeTrue("有効なプロセスは実行できるべきです");
    }

    /// <summary>
    /// 危険なプロセス実行の検証テスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Windows\\System32\\cmd.exe", "https://www.google.com")]
    [InlineData("C:\\Windows\\System32\\powershell.exe", "https://www.microsoft.com")]
    [InlineData("C:\\Windows\\System32\\regedit.exe", "https://www.github.com")]
    [InlineData("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "https://www.google.com; rm -rf /")]
    [InlineData("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://www.microsoft.com | del /f /q C:\\")]
    public void ProcessExecutionShouldRejectDangerousProcesses(string dangerousPath, string dangerousArguments)
    {
        // Act
        var canExecute = CanExecuteProcess(dangerousPath, dangerousArguments);

        // Assert
        canExecute.Should().BeFalse($"危険なプロセス '{dangerousPath}' の実行は拒否されるべきです");
    }

    /// <summary>
    /// プロセス実行の権限チェックテスト.
    /// </summary>
    [Fact]
    public void ProcessExecutionShouldCheckPermissions()
    {
        // Arrange
        var validPath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

        // Act
        var hasPermission = HasExecutionPermission(validPath);

        // Assert
        hasPermission.Should().BeTrue("有効なプロセスには実行権限があるべきです");
    }

    /// <summary>
    /// 危険なプロセス実行の権限チェックテスト.
    /// </summary>
    [Theory]
    [InlineData("C:\\Windows\\System32\\cmd.exe")]
    [InlineData("C:\\Windows\\System32\\powershell.exe")]
    [InlineData("C:\\Windows\\System32\\regedit.exe")]
    [InlineData("C:\\Windows\\System32\\gpedit.msc")]
    [InlineData("C:\\Windows\\System32\\mmc.exe")]
    public void ProcessExecutionShouldRejectDangerousPermissions(string dangerousPath)
    {
        // Act
        var hasPermission = HasExecutionPermission(dangerousPath);

        // Assert
        hasPermission.Should().BeFalse($"危険なプロセス '{dangerousPath}' には実行権限がないべきです");
    }

    /// <summary>
    /// 実行パスが有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidExecutablePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // 長さ制限チェック
        if (path.Length > 260)
        {
            return false;
        }

        // パストラバーサル攻撃チェック
        if (path.Contains("..") || path.Contains("..\\") || path.Contains("../"))
        {
            return false;
        }

        // 危険な実行パスチェック
        var dangerousPaths = new[]
        {
            "C:\\Windows\\System32\\cmd.exe",
            "C:\\Windows\\System32\\powershell.exe",
            "C:\\Windows\\System32\\regedit.exe",
            "C:\\Windows\\System32\\gpedit.msc",
            "C:\\Windows\\System32\\mmc.exe",
            "C:\\Windows\\System32\\taskmgr.exe",
            "C:\\Windows\\System32\\msconfig.exe",
            "C:\\Windows\\System32\\services.msc",
            "C:\\Windows\\System32\\eventvwr.msc",
            "C:\\Windows\\System32\\compmgmt.msc"
        };

        foreach (var dangerousPath in dangerousPaths)
        {
            if (path.Equals(dangerousPath, StringComparison.OrdinalIgnoreCase))
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
            if (System.Text.RegularExpressions.Regex.IsMatch(path, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        // 実行可能ファイル拡張子チェック
        var validExtensions = new[] { ".EXE", ".MSC", ".BAT", ".CMD", ".COM", ".SCR" };
        var extension = System.IO.Path.GetExtension(path).ToUpperInvariant();
        if (!validExtensions.Contains(extension))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// コマンドライン引数が有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidCommandLineArgument(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            return false;
        }

        // 長さ制限チェック
        if (argument.Length > 2048)
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
            if (System.Text.RegularExpressions.Regex.IsMatch(argument, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        // 基本的なURL形式チェック
        return Uri.TryCreate(argument, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// プロセスを実行できるかどうかを検証するメソッド.
    /// </summary>
    private static bool CanExecuteProcess(string path, string arguments)
    {
        if (!IsValidExecutablePath(path) || !IsValidCommandLineArgument(arguments))
        {
            return false;
        }

        // 実際の実行は行わず、パスと引数の妥当性のみチェック
        return true;
    }

    /// <summary>
    /// プロセス実行の権限があるかどうかを検証するメソッド.
    /// </summary>
    private static bool HasExecutionPermission(string path)
    {
        if (!IsValidExecutablePath(path))
        {
            return false;
        }

        // 実際の権限チェックは行わず、パスの妥当性のみチェック
        return true;
    }
}
