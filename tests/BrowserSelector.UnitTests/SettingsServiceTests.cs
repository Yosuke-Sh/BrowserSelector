using BrowserSelector.Core.Models;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using System.IO;
using System.IO.Compression;

namespace BrowserSelector.UnitTests;

public class SettingsServiceTests
{
    private readonly string _testDirectory;
    private readonly SettingsService _settingsService;

    public SettingsServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        // テスト用のSettingsServiceを作成（ポータブルモードをシミュレート）
        var executablePath = Path.Combine(_testDirectory, "BrowserSelector.exe");
        File.WriteAllText(executablePath, "test");
        var portableMarkerPath = Path.Combine(_testDirectory, "portable.txt");
        File.WriteAllText(portableMarkerPath, "portable");
        
        _settingsService = new SettingsService();
    }

    [Fact]
    public async Task ExportSettingsAsync_ShouldCreateZipFile()
    {
        // Arrange
        var zipFilePath = Path.Combine(_testDirectory, "test_settings.zip");
        
        // テスト用の設定ファイルを作成
        var appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
        var visualSettingsPath = Path.Combine(_testDirectory, "visualsettings.json");
        var logSettingsPath = Path.Combine(_testDirectory, "logsettings.json");
        var urlRulesPath = Path.Combine(_testDirectory, "urlrules.json");
        var languagesPath = Path.Combine(_testDirectory, "Languages");
        
        Directory.CreateDirectory(languagesPath);
        
        await File.WriteAllTextAsync(appSettingsPath, "{}");
        await File.WriteAllTextAsync(visualSettingsPath, "{}");
        await File.WriteAllTextAsync(logSettingsPath, "{}");
        await File.WriteAllTextAsync(urlRulesPath, "[]");
        await File.WriteAllTextAsync(Path.Combine(languagesPath, "en-US.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(languagesPath, "ja-JP.json"), "{}");

        // Act
        var result = await _settingsService.ExportSettingsAsync(zipFilePath);

        // Assert
        result.Should().BeTrue();
        File.Exists(zipFilePath).Should().BeTrue();
        
        // ZIPファイルの内容を確認
        using var archive = ZipFile.OpenRead(zipFilePath);
        archive.Entries.Should().Contain(e => e.Name == "appsettings.json");
        archive.Entries.Should().Contain(e => e.Name == "visualsettings.json");
        archive.Entries.Should().Contain(e => e.Name == "logsettings.json");
        archive.Entries.Should().Contain(e => e.Name == "urlrules.json");
        archive.Entries.Should().Contain(e => e.FullName == "Languages\\en-US.json");
        archive.Entries.Should().Contain(e => e.FullName == "Languages\\ja-JP.json");
        archive.Entries.Should().Contain(e => e.Name == "export-info.json");
    }

    [Fact]
    public async Task ImportSettingsAsync_WithZipFile_ShouldExtractFiles()
    {
        // Arrange
        var zipFilePath = Path.Combine(_testDirectory, "test_import.zip");
        var targetDirectory = Path.Combine(_testDirectory, "import_target");
        Directory.CreateDirectory(targetDirectory);
        
        // テスト用のZIPファイルを作成
        using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            var appSettingsEntry = archive.CreateEntry("appsettings.json");
            using (var writer = new StreamWriter(appSettingsEntry.Open()))
            {
                await writer.WriteAsync("{\"Language\":\"ja-JP\"}");
            }
            
            var visualSettingsEntry = archive.CreateEntry("visualsettings.json");
            using (var writer = new StreamWriter(visualSettingsEntry.Open()))
            {
                await writer.WriteAsync("{\"BackgroundColor\":\"#FF0000\"}");
            }
        }

        // Act
        var result = await _settingsService.ImportSettingsAsync(zipFilePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ImportSettingsAsync_WithJsonFile_ShouldImportSettings()
    {
        // Arrange
        var jsonFilePath = Path.Combine(_testDirectory, "test_settings.json");
        var jsonContent = """
        {
            "Language": "ja-JP",
            "EnableLogging": true,
            "CloseAfterUrlRuleMatch": false
        }
        """;
        await File.WriteAllTextAsync(jsonFilePath, jsonContent);

        // Act
        var result = await _settingsService.ImportSettingsAsync(jsonFilePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ImportSettingsAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "non_existent.zip");

        // Act
        var result = await _settingsService.ImportSettingsAsync(nonExistentPath);

        // Assert
        result.Should().BeFalse();
    }

    private void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
