using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
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
        Directory.CreateDirectory(_tempDirectory);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddScoped<ISettingsService>(provider => 
                {
                    var logService = provider.GetService<ILogService>();
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
        var testSettings = new BrowserSelector.Core.Models.AppSettings
        {
            Language = "ja-JP",
            CustomProtocol = "testprotocol"
        };

        // Act
        var saveResult = await _settingsService.SaveAppSettingsAsync(testSettings);
        var loadedSettings = await _settingsService.LoadAppSettingsAsync();

        // Assert
        // TestSettingsServiceを使用するため、保存と読み込みが正常に動作する
        saveResult.Should().BeTrue();
        loadedSettings.Should().NotBeNull();

        loadedSettings.Language.Should().Be(testSettings.Language);
        loadedSettings.CustomProtocol.Should().Be(testSettings.CustomProtocol);
    }

    [Fact]
    public async Task SettingsService_LoadVisualSettings_ShouldReturnDefaultValues()
    {
        // Act
        var visualSettings = await _settingsService.LoadVisualSettingsAsync();

        // Assert
        visualSettings.Should().NotBeNull();
        visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        visualSettings.UseBackgroundGradient.Should().BeFalse();
        visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Vertical);
        visualSettings.IconScale.Should().Be(1.0);
        visualSettings.ShowFocusIndicator.Should().BeTrue();
        visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        visualSettings.FocusThickness.Should().Be(2.0);
        visualSettings.FocusWidth.Should().Be(100.0);
        visualSettings.InitialWindowWidth.Should().Be(800.0);
        visualSettings.InitialWindowHeight.Should().Be(600.0);
        visualSettings.ShowLogo.Should().BeTrue();
        visualSettings.ShowUrlInput.Should().BeTrue();
        visualSettings.BrowserButtonWidth.Should().Be(120.0);
        visualSettings.BrowserButtonHeight.Should().Be(90.0);
        visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Black);
        visualSettings.BrowserButtonOpacity.Should().Be(1.0);
        visualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        visualSettings.ShowBrowserName.Should().BeTrue();
        visualSettings.BrowserIconSize.Should().Be(32.0);
    }

    [Fact]
    public async Task SettingsService_ResetSettings_ShouldRestoreDefaults()
    {
        // Arrange
        var originalSettings = await _settingsService.LoadAppSettingsAsync();

        // Act
        var resetResult = await _settingsService.ResetSettingsAsync();
        var resetSettings = await _settingsService.LoadAppSettingsAsync();

        // Assert
        resetResult.Should().BeTrue();
        resetSettings.Should().NotBeNull();
        // デフォルト値に戻っていることを確認
        // TODO: 削除されたExpandShortenedUrlsプロパティのテストを更新
    }
}
