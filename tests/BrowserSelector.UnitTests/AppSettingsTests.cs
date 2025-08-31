using BrowserSelector.Core.Models;
using FluentAssertions;
using Xunit;

namespace BrowserSelector.UnitTests;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        settings.StartMinimized.Should().BeFalse();
        settings.StartInSystemTray.Should().BeFalse();
        settings.StartupDelay.Should().Be(0);
        settings.EnableLogging.Should().BeTrue();
        settings.Language.Should().Be("ja-JP");
        settings.PortableMode.Should().BeFalse();
        settings.CustomProtocol.Should().Be("browserselector");
    }

    [Fact]
    public void AppSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        var settings = new AppSettings();
        var propertyChangedCount = 0;
        settings.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        settings.StartMinimized = true;
        settings.StartInSystemTray = true;
        settings.StartupDelay = 5;
        settings.EnableLogging = false;
        settings.Language = "en-US";
        settings.PortableMode = true;
        settings.CustomProtocol = "custom";

        // Assert
        propertyChangedCount.Should().Be(7);
    }
}
