using BrowserSelector.Core.Models;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        AppSettings settings = new();

        // Assert
        _ = settings.EnableLogging.Should().BeTrue();
        _ = settings.Language.Should().Be("en-US"); // デフォルト言語を英語に変更
        _ = settings.PortableMode.Should().BeFalse();
        _ = settings.CustomProtocol.Should().Be("browserselector");
        _ = settings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void AppSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        AppSettings settings = new();
        int propertyChangedCount = 0;
        settings.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        settings.StartupMessage = "Test";
        settings.EnableLogging = false;
        settings.LogLevel = "Debug";
        settings.CheckForUpdates = false;
        settings.UpdateCheckInterval = 12;
        settings.Language = "ja-JP";
        settings.PortableMode = true;
        settings.CustomProtocol = "custom";
        settings.RegisterProtocol = false;
        settings.CloseAfterUrlRuleMatch = false;

        // Assert
        _ = propertyChangedCount.Should().Be(10); // 全プロパティを変更
    }
}
