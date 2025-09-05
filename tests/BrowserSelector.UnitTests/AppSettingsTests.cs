using BrowserSelector.Core.Models;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        settings.EnableLogging.Should().BeTrue();
        settings.Language.Should().Be("ja-JP");
        settings.PortableMode.Should().BeFalse();
        settings.CustomProtocol.Should().Be("browserselector");
        settings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void AppSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        var settings = new AppSettings();
        var propertyChangedCount = 0;
        settings.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        settings.EnableLogging = false;
        settings.Language = "en-US";
        settings.PortableMode = true;
        settings.CustomProtocol = "custom";
        settings.CloseAfterUrlRuleMatch = false;

        // Assert
        propertyChangedCount.Should().Be(5);
    }
}
