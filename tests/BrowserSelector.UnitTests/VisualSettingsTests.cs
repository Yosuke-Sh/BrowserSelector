using BrowserSelector.Core.Models;
using FluentAssertions;
using System.Windows.Media;

namespace BrowserSelector.UnitTests;

public class VisualSettingsTests
{
    [Fact]
    public void VisualSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var settings = new VisualSettings();

        // Assert
        settings.BackgroundColor.Should().Be(Colors.White);
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
        settings.BackgroundColor = Colors.Gray;
        settings.FocusColor = Colors.Green;
        settings.FocusWidth = 150.0;

        // Assert
        propertyChangedCount.Should().Be(3);
    }

    [Fact]
    public void VisualSettings_BackgroundColor_ShouldAcceptValidValues()
    {
        // Arrange
        var settings = new VisualSettings();

        // Act & Assert
        settings.BackgroundColor = Colors.White;
        settings.BackgroundColor.Should().Be(Colors.White);

        settings.BackgroundColor = Colors.Black;
        settings.BackgroundColor.Should().Be(Colors.Black);

        settings.BackgroundColor = Colors.Red;
        settings.BackgroundColor.Should().Be(Colors.Red);
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(200.0)]
    public void VisualSettings_FocusWidth_ShouldAcceptValidValues(double focusWidth)
    {
        // Arrange
        var settings = new VisualSettings();

        // Act
        settings.FocusWidth = focusWidth;

        // Assert
        settings.FocusWidth.Should().Be(focusWidth);
    }
}
