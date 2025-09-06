using BrowserSelector.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BrowserSelector.IntegrationTests;

public class SettingsServiceIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly ISettingsService _settingsService;
    private readonly string _tempDirectory;

    public SettingsServiceIntegrationTests()
    {
        // テスト用の一時ディレクトリを作成
        _tempDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTest", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_tempDirectory);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                _ = services.AddScoped<ISettingsService>(provider =>
                {
                    ILogService? logService = provider.GetService<ILogService>();
                    return new TestSettingsService(logService, _tempDirectory);
                });
            })
            .Build();

        _settingsService = _host.Services.GetRequiredService<ISettingsService>();
    }

    public void Dispose()
    {
        // テスト用の一時ディレクトリを削除
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // 削除に失敗しても無視
            }
        }

        _host?.Dispose();
    }

    [Fact]
    public async Task SettingsService_SaveAndLoad_ShouldPersistCorrectly()
    {
        // Arrange
        Core.Models.AppSettings testSettings = new()
        {
            Language = "ja-JP",
            CustomProtocol = "testprotocol"
        };

        // Act
        bool saveResult = await _settingsService.SaveAppSettingsAsync(testSettings);
        Core.Models.AppSettings loadedSettings = await _settingsService.LoadAppSettingsAsync();

        // Assert
        // TestSettingsServiceを使用するため、保存と読み込みが正常に動作する
        _ = saveResult.Should().BeTrue();
        _ = loadedSettings.Should().NotBeNull();

        _ = loadedSettings.Language.Should().Be(testSettings.Language);
        _ = loadedSettings.CustomProtocol.Should().Be(testSettings.CustomProtocol);
    }

    [Fact]
    public async Task SettingsService_LoadVisualSettings_ShouldReturnDefaultValues()
    {
        // Act
        Core.Models.VisualSettings visualSettings = await _settingsService.LoadVisualSettingsAsync();

        // Assert
        _ = visualSettings.Should().NotBeNull();
        _ = visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        _ = visualSettings.UseBackgroundGradient.Should().BeFalse();
        _ = visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Vertical);
        _ = visualSettings.IconScale.Should().Be(1.0);
        _ = visualSettings.ShowFocusIndicator.Should().BeTrue();
        _ = visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        _ = visualSettings.FocusThickness.Should().Be(2.0);
        _ = visualSettings.FocusWidth.Should().Be(100.0);
        _ = visualSettings.InitialWindowWidth.Should().Be(800.0);
        _ = visualSettings.InitialWindowHeight.Should().Be(600.0);
        _ = visualSettings.ShowLogo.Should().BeTrue();
        _ = visualSettings.ShowUrlInput.Should().BeTrue();
        _ = visualSettings.BrowserButtonWidth.Should().Be(120.0);
        _ = visualSettings.BrowserButtonHeight.Should().Be(90.0);
        _ = visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Black);
        _ = visualSettings.BrowserButtonOpacity.Should().Be(1.0);
        _ = visualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        _ = visualSettings.ShowBrowserName.Should().BeTrue();
        _ = visualSettings.BrowserIconSize.Should().Be(32.0);
    }

    [Fact]
    public async Task SettingsService_ResetSettings_ShouldRestoreDefaults()
    {
        // Arrange
        _ = await _settingsService.LoadAppSettingsAsync();

        // Act
        bool resetResult = await _settingsService.ResetSettingsAsync();
        Core.Models.AppSettings resetSettings = await _settingsService.LoadAppSettingsAsync();

        // Assert
        _ = resetResult.Should().BeTrue();
        _ = resetSettings.Should().NotBeNull();
        // デフォルト値に戻っていることを確認
        // TODO: 削除されたExpandShortenedUrlsプロパティのテストを更新
    }
}
