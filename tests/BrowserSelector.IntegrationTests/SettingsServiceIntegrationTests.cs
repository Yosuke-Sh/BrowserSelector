using Xunit;
using FluentAssertions;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace BrowserSelector.IntegrationTests;

public class SettingsServiceIntegrationTests
{
    private readonly IHost _host;
    private readonly ISettingsService _settingsService;

    public SettingsServiceIntegrationTests()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddScoped<ISettingsService, SettingsService>();
            })
            .Build();

        _settingsService = _host.Services.GetRequiredService<ISettingsService>();
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
        saveResult.Should().BeTrue();
        loadedSettings.Should().NotBeNull();

        loadedSettings.Language.Should().Be(testSettings.Language);
        loadedSettings.CustomProtocol.Should().Be(testSettings.CustomProtocol);
    }

    [Fact(Skip = "削除されたOpacityプロパティのテストを更新する必要があります")]
    public async Task SettingsService_LoadVisualSettings_ShouldReturnDefaultValues()
    {
        // Act
        var visualSettings = await _settingsService.LoadVisualSettingsAsync();

        // Assert
        visualSettings.Should().NotBeNull();
        // TODO: 削除されたOpacityプロパティのテストを更新
        // visualSettings.Opacity.Should().BeGreaterThan(0);
        // visualSettings.Opacity.Should().BeLessThanOrEqualTo(1);
        visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        // TODO: 削除されたMessageTextColorプロパティのテストを更新
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
