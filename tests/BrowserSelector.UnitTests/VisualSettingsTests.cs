using BrowserSelector.Core.Models;
using FluentAssertions;
using System.Windows.Media;
using Xunit;

namespace BrowserSelector.UnitTests;

public class VisualSettingsTests
{
    [Fact]
    public void VisualSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var settings = new VisualSettings();

        // Assert
        settings.Opacity.Should().Be(1.0);
        settings.TransparencyColor.Should().Be(Colors.Black);
        settings.CornerRadius.Should().Be(0);
        settings.ShowTitleBar.Should().BeTrue();
        settings.BackgroundColor.Should().Be(Colors.Transparent);
        settings.FocusColor.Should().Be(Colors.Blue);
        settings.FocusWidth.Should().Be(100.0);
    }

    [Fact]
    public void VisualSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        var settings = new VisualSettings();
        var propertyChangedCount = 0;
        settings.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        settings.Opacity = 0.8;
        settings.TransparencyColor = Colors.Red;
        settings.CornerRadius = 10;
        settings.ShowTitleBar = false;
        settings.BackgroundColor = Colors.Gray;
        settings.FocusColor = Colors.Green;
        settings.FocusWidth = 150.0;

        // Assert
        propertyChangedCount.Should().Be(7);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void VisualSettings_Opacity_ShouldAcceptValidValues(double opacity)
    {
        // Arrange
        var settings = new VisualSettings();

        // Act
        settings.Opacity = opacity;

        // Assert
        settings.Opacity.Should().Be(opacity);
    }
}
