using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using System.IO.Compression;

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

        // テスト用のSettingsServiceを作成（ポータブルモードをシミュレート）
        string executablePath = Path.Combine(_testDirectory, "BrowserSelector.exe");
        File.WriteAllText(executablePath, "test");
        string portableMarkerPath = Path.Combine(_testDirectory, "portable.txt");
        File.WriteAllText(portableMarkerPath, "portable");

        _settingsService = new SettingsService();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExportSettingsAsync_ShouldCreateZipFile()
    {
        // Arrange
        string zipFilePath = Path.Combine(_testDirectory, "test_settings.zip");

        // テスト用の設定ファイルを作成
        string appSettingsPath = Path.Combine(_testDirectory, "appsettings.json");
        string visualSettingsPath = Path.Combine(_testDirectory, "visualsettings.json");
        string logSettingsPath = Path.Combine(_testDirectory, "logsettings.json");
        string urlRulesPath = Path.Combine(_testDirectory, "urlrules.json");
        string languagesPath = Path.Combine(_testDirectory, "Languages");

        _ = Directory.CreateDirectory(languagesPath);

        await File.WriteAllTextAsync(appSettingsPath, "{}");
        await File.WriteAllTextAsync(visualSettingsPath, "{}");
        await File.WriteAllTextAsync(logSettingsPath, "{}");
        await File.WriteAllTextAsync(urlRulesPath, "[]");
        await File.WriteAllTextAsync(Path.Combine(languagesPath, "en-US.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(languagesPath, "ja-JP.json"), "{}");

        // Act
        bool result = await _settingsService.ExportSettingsAsync(zipFilePath);

        // Assert
        _ = result.Should().BeTrue();
        _ = File.Exists(zipFilePath).Should().BeTrue();

        // ZIPファイルの内容を確認
        using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
        _ = archive.Entries.Should().Contain(e => e.Name == "appsettings.json");
        _ = archive.Entries.Should().Contain(e => e.Name == "visualsettings.json");
        _ = archive.Entries.Should().Contain(e => e.Name == "logsettings.json");
        _ = archive.Entries.Should().Contain(e => e.Name == "urlrules.json");
        _ = archive.Entries.Should().Contain(e => e.FullName == "Languages\\en-US.json");
        _ = archive.Entries.Should().Contain(e => e.FullName == "Languages\\ja-JP.json");
        _ = archive.Entries.Should().Contain(e => e.Name == "export-info.json");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ImportSettingsAsync_WithZipFile_ShouldExtractFiles()
    {
        // Arrange
        string zipFilePath = Path.Combine(_testDirectory, "test_import.zip");
        string targetDirectory = Path.Combine(_testDirectory, "import_target");
        _ = Directory.CreateDirectory(targetDirectory);

        // テスト用のZIPファイルを作成
        using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            ZipArchiveEntry appSettingsEntry = archive.CreateEntry("appsettings.json");
            using (StreamWriter writer = new(appSettingsEntry.Open()))
            {
                await writer.WriteAsync("{\"Language\":\"ja-JP\"}");
            }

            ZipArchiveEntry visualSettingsEntry = archive.CreateEntry("visualsettings.json");
            using (StreamWriter writer = new(visualSettingsEntry.Open()))
            {
                await writer.WriteAsync("{\"BackgroundColor\":\"#FF0000\"}");
            }
        }

        // Act
        bool result = await _settingsService.ImportSettingsAsync(zipFilePath);

        // Assert
        _ = result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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
