using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.Logging;
using FluentAssertions;
using System.IO.Compression;
using Moq;

namespace BrowserSelector.UnitTests;

public class SettingsServiceTests
{
    private readonly string _testDirectory;
    private readonly SettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsServiceTests"/> class.
    /// </summary>
    public SettingsServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTests", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_testDirectory);

        // テスト用のSettingsServiceを作成（ログサービスなし）
        _settingsService = new SettingsService(null);
    }


    [Fact]
    public async Task ImportSettingsAsync_WithZipFile_ShouldExtractFiles()
    {
        // Arrange
        string zipFilePath = Path.Combine(_testDirectory, "test_import.zip");
        string targetDirectory = Path.Combine(_testDirectory, "import_target");
        _ = Directory.CreateDirectory(targetDirectory);

        // テスト用のZIPファイルを作成
        using (ZipArchive archive = await ZipFile.OpenAsync(zipFilePath, ZipArchiveMode.Create))
        {
            ZipArchiveEntry appSettingsEntry = archive.CreateEntry("appsettings.json");
            using (StreamWriter writer = new(await appSettingsEntry.OpenAsync()))
            {
                await writer.WriteAsync("{\"Language\":\"ja-JP\"}");
            }

            ZipArchiveEntry visualSettingsEntry = archive.CreateEntry("visualsettings.json");
            using (StreamWriter writer = new(await visualSettingsEntry.OpenAsync()))
            {
                await writer.WriteAsync("{\"BackgroundColor\":\"#FF0000\"}");
            }
        }

        // Act
        bool result = await _settingsService.ImportSettingsAsync(zipFilePath);

        // Assert
        _ = result.Should().BeTrue();
    }

    [Fact]
    public async Task ImportSettingsAsync_WithJsonFile_ShouldImportSettings()
    {
        // Arrange
        string jsonFilePath = Path.Combine(_testDirectory, "test_settings.json");
        string jsonContent = """
        {
            "Language": "ja-JP",
            "EnableLogging": true,
            "CloseAfterUrlRuleMatch": false
        }
        """;
        await File.WriteAllTextAsync(jsonFilePath, jsonContent);

        // Act
        bool result = await _settingsService.ImportSettingsAsync(jsonFilePath);

        // Assert
        _ = result.Should().BeTrue();
    }

    [Fact]
    public async Task ImportSettingsAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_testDirectory, "non_existent.zip");

        // Act
        bool result = await _settingsService.ImportSettingsAsync(nonExistentPath);

        // Assert
        _ = result.Should().BeFalse();
    }

    private void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
