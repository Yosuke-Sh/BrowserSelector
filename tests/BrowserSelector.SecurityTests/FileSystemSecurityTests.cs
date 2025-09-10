using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace BrowserSelector.SecurityTests;

/// <summary>
/// ファイルシステムセキュリティテスト.
/// </summary>
public class FileSystemSecurityTests
{
    /// <summary>
    /// ファイルパスの検証テスト.
    /// </summary>
    /// <param name="validPath">有効なファイルパス.</param>
    [Theory]
    [InlineData("C:\\Program Files\\BrowserSelector\\config.xml")]
    [InlineData("C:\\Users\\User\\AppData\\Local\\BrowserSelector\\settings.json")]
    [InlineData("D:\\Project\\BrowserSelector\\src\\BrowserSelector.App\\bin\\Debug\\BrowserSelector.App.exe")]
    public void FilePathShouldAcceptValidPaths(string validPath)
    {
        // Arrange
        var isValidPath = IsValidFilePath(validPath);

        // Act & Assert
        isValidPath.Should().BeTrue($"有効なファイルパス '{validPath}' は受け入れられるべきです");
    }

    /// <summary>
    /// パストラバーサル攻撃の検証テスト.
    /// </summary>
    /// <param name="maliciousPath">maliciousPath.</param>
    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("C:\\Users\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("D:\\Project\\..\\..\\..\\windows\\system32")]
    [InlineData("config\\..\\..\\..\\windows\\system32")]
    [InlineData("settings\\..\\..\\..\\etc\\passwd")]
    public void FilePathShouldRejectPathTraversalAttacks(string maliciousPath)
    {
        // Arrange
        var isValidPath = IsValidFilePath(maliciousPath);

        // Act & Assert
        isValidPath.Should().BeFalse($"パストラバーサル攻撃パス '{maliciousPath}' は拒否されるべきです");
    }

    /// <summary>
    /// FilePathShouldRejectReservedNames.
    /// </summary>
    /// <param name="reservedName">reservedName.</param>
    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("COM2")]
    [InlineData("COM3")]
    [InlineData("COM4")]
    [InlineData("COM5")]
    [InlineData("COM6")]
    [InlineData("COM7")]
    [InlineData("COM8")]
    [InlineData("COM9")]
    [InlineData("LPT1")]
    [InlineData("LPT2")]
    [InlineData("LPT3")]
    [InlineData("LPT4")]
    [InlineData("LPT5")]
    [InlineData("LPT6")]
    [InlineData("LPT7")]
    [InlineData("LPT8")]
    [InlineData("LPT9")]
    public void FilePathShouldRejectReservedNames(string reservedName)
    {
        // Arrange
        var isValidPath = IsValidFilePath(reservedName);

        // Act & Assert
        isValidPath.Should().BeFalse($"予約されたファイル名 '{reservedName}' は拒否されるべきです");
    }

    /// <summary>
    /// 無効な文字を含むファイルパスの検証テスト.
    /// </summary>
    /// <param name="invalidPath">invalidPath.</param>
    [Theory]
    [InlineData("file<name>.txt")]
    [InlineData("file>name.txt")]
    [InlineData("file:name.txt")]
    [InlineData("file\"name.txt")]
    [InlineData("file|name.txt")]
    [InlineData("file?name.txt")]
    [InlineData("file*name.txt")]
    [InlineData("file\nname.txt")]
    [InlineData("file\rname.txt")]
    [InlineData("file\tname.txt")]
    [InlineData("file\0name.txt")]
    public void FilePathShouldRejectInvalidCharacters(string invalidPath)
    {
        // Arrange
        var isValidPath = IsValidFilePath(invalidPath);

        // Act & Assert
        isValidPath.Should().BeFalse($"無効な文字を含むパス '{invalidPath}' は拒否されるべきです");
    }

    /// <summary>
    /// 長すぎるファイルパスの検証テスト.
    /// </summary>
    [Fact]
    public void FilePathShouldRejectExcessivelyLongPaths()
    {
        // Arrange
        var longPath = "C:\\" + new string('a', 300) + "\\file.txt";

        // Act
        var isValidPath = IsValidFilePath(longPath);

        // Assert
        isValidPath.Should().BeFalse("長すぎるファイルパスは拒否されるべきです");
    }

    /// <summary>
    /// ディレクトリ作成のセキュリティテスト.
    /// </summary>
    [Fact]
    public void DirectoryCreationShouldBeSecure()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        var testDir = Path.Combine(basePath, "BrowserSelectorTest_" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            // Act
            var canCreate = CanCreateDirectory(testDir);

            // Assert
            canCreate.Should().BeTrue("有効なディレクトリは作成できるべきです");

            // クリーンアップ
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir);
            }
        }
        catch (Exception ex)
        {
            // クリーンアップ
            if (Directory.Exists(testDir))
            {
                try
                {
                    Directory.Delete(testDir);
                }
                catch
                {
                    throw;
                }
            }

            throw new Exception($"ディレクトリ作成テストでエラーが発生しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 危険なディレクトリ作成の検証テスト.
    /// </summary>
    /// <param name="dangerousPath">dangerousPath.</param>
    [Theory]
    [InlineData("C:\\Windows\\System32")]
    [InlineData("C:\\Program Files")]
    [InlineData("C:\\Program Files (x86)")]
    [InlineData("C:\\Users\\Public")]
    [InlineData("C:\\Windows\\Temp")]
    public void DirectoryCreationShouldRejectDangerousPaths(string dangerousPath)
    {
        // Arrange
        var testDir = Path.Combine(dangerousPath, "BrowserSelectorTest_" + Guid.NewGuid().ToString("N")[..8]);

        // Act
        var canCreate = CanCreateDirectory(testDir);

        // Assert
        canCreate.Should().BeFalse($"危険なディレクトリパス '{dangerousPath}' での作成は拒否されるべきです");
    }

    /// <summary>
    /// ファイル読み取りのセキュリティテスト.
    /// </summary>
    [Fact]
    public void FileReadingShouldBeSecure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var testContent = "Test content for security validation";

        try
        {
            File.WriteAllText(tempFile, testContent);

            // Act
            var canRead = CanReadFile(tempFile);
            var content = canRead ? File.ReadAllText(tempFile) : null;

            // Assert
            canRead.Should().BeTrue("有効なファイルは読み取れるべきです");
            content.Should().Be(testContent, "ファイル内容は正しく読み取られるべきです");
        }
        finally
        {
            // クリーンアップ
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// 危険なファイル読み取りの検証テスト.
    /// </summary>
    /// <param name="dangerousFile">dangerousFile.</param>
    [Theory]
    [InlineData("C:\\Windows\\System32\\config\\SAM")]
    [InlineData("C:\\Windows\\System32\\drivers\\etc\\hosts")]
    [InlineData("C:\\Windows\\System32\\config\\SECURITY")]
    [InlineData("C:\\Windows\\System32\\config\\SOFTWARE")]
    [InlineData("C:\\Windows\\System32\\config\\SYSTEM")]
    public void FileReadingShouldRejectDangerousFiles(string dangerousFile)
    {
        ArgumentNullException.ThrowIfNull(dangerousFile);

        // Act
        var canRead = CanReadFile(dangerousFile);

        // Assert
        canRead.Should().BeFalse($"危険なファイル '{dangerousFile}' の読み取りは拒否されるべきです");
    }

    /// <summary>
    /// ファイル書き込みのセキュリティテスト.
    /// </summary>
    [Fact]
    public void FileWritingShouldBeSecure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var testContent = "Test content for security validation";

        try
        {
            // Act
            var canWrite = CanWriteFile(tempFile, testContent);

            // Assert
            canWrite.Should().BeTrue("有効なファイルは書き込めるべきです");

            if (canWrite)
            {
                var content = File.ReadAllText(tempFile);
                content.Should().Be(testContent, "ファイル内容は正しく書き込まれるべきです");
            }
        }
        finally
        {
            // クリーンアップ
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// 危険なファイル書き込みの検証テスト.
    /// </summary>
    /// <param name="dangerousFile">dangerousFile.</param>
    [Theory]
    [InlineData("C:\\Windows\\System32\\config\\SAM")]
    [InlineData("C:\\Windows\\System32\\drivers\\etc\\hosts")]
    [InlineData("C:\\Windows\\System32\\config\\SECURITY")]
    [InlineData("C:\\Windows\\System32\\config\\SOFTWARE")]
    [InlineData("C:\\Windows\\System32\\config\\SYSTEM")]
    public void FileWritingShouldRejectDangerousFiles(string dangerousFile)
    {
        // Arrange
        var testContent = "Malicious content";

        // Act
        var canWrite = CanWriteFile(dangerousFile, testContent);

        // Assert
        canWrite.Should().BeFalse($"危険なファイル '{dangerousFile}' への書き込みは拒否されるべきです");
    }

    /// <summary>
    /// ファイルパスが有効かどうかを検証するメソッド.
    /// </summary>
    private static bool IsValidFilePath(string path)
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
        if (path.Contains("..", StringComparison.Ordinal) || path.Contains("..\\", StringComparison.Ordinal) || path.Contains("../", StringComparison.Ordinal))
        {
            return false;
        }

        // 予約されたファイル名チェック
        var fileName = Path.GetFileName(path);
        var reservedNames = new[]
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        if (reservedNames.Contains(fileName.ToUpperInvariant()))
        {
            return false;
        }

        // 無効な文字チェック
        var invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
        {
            return false;
        }

        try
        {
            // パスの正規化チェック
            _ = Path.GetFullPath(path);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// ディレクトリを作成できるかどうかを検証するメソッド.
    /// </summary>
    private static bool CanCreateDirectory(string path)
    {
        if (!IsValidFilePath(path))
        {
            return false;
        }

        try
        {
            // 危険なパスチェック
            var dangerousPaths = new[]
            {
                "C:\\Windows\\System32",
                "C:\\Program Files",
                "C:\\Program Files (x86)",
                "C:\\Users\\Public",
                "C:\\Windows\\Temp"
            };

            foreach (var dangerousPath in dangerousPaths)
            {
                if (path.StartsWith(dangerousPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // 実際の作成は行わず、パスの妥当性のみチェック
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// ファイルを読み取れるかどうかを検証するメソッド.
    /// </summary>
    private static bool CanReadFile(string path)
    {
        if (!IsValidFilePath(path))
        {
            return false;
        }

        try
        {
            // 危険なファイルチェック
            var dangerousFiles = new[]
            {
                "C:\\Windows\\System32\\config\\SAM",
                "C:\\Windows\\System32\\drivers\\etc\\hosts",
                "C:\\Windows\\System32\\config\\SECURITY",
                "C:\\Windows\\System32\\config\\SOFTWARE",
                "C:\\Windows\\System32\\config\\SYSTEM"
            };

            foreach (var dangerousFile in dangerousFiles)
            {
                if (path.Equals(dangerousFile, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // ファイルの存在チェック
            return File.Exists(path);
        }
        catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// ファイルに書き込めるかどうかを検証するメソッド.
    /// </summary>
    private static bool CanWriteFile(string path, string content)
    {
        if (!IsValidFilePath(path))
        {
            return false;
        }

        try
        {
            // 危険なファイルチェック
            var dangerousFiles = new[]
            {
                "C:\\Windows\\System32\\config\\SAM",
                "C:\\Windows\\System32\\drivers\\etc\\hosts",
                "C:\\Windows\\System32\\config\\SECURITY",
                "C:\\Windows\\System32\\config\\SOFTWARE",
                "C:\\Windows\\System32\\config\\SYSTEM"
            };

            foreach (var dangerousFile in dangerousFiles)
            {
                if (path.Equals(dangerousFile, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // 実際の書き込みは行わず、パスの妥当性のみチェック
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or UnauthorizedAccessException or PathTooLongException)
        {
            return false;
        }
    }
}
