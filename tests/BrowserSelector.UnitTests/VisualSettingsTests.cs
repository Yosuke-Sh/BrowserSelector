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
        VisualSettings settings = new();

        // Assert
        _ = settings.BackgroundColor.Should().Be(Colors.White);
        _ = settings.FocusColor.Should().Be(Colors.Blue);
        _ = settings.FocusWidth.Should().Be(100.0);
    }

    [Fact]
    public void VisualSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        VisualSettings settings = new();
        int propertyChangedCount = 0;
        settings.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        settings.BackgroundColor = Colors.Gray;
        settings.FocusColor = Colors.Green;
        settings.FocusWidth = 150.0;

        // Assert
        _ = propertyChangedCount.Should().Be(3);
    }

    [Fact]
    public void VisualSettings_BackgroundColor_ShouldAcceptValidValues()
    {
        // Arrange
        VisualSettings settings = new()
        {
            // Act & Assert
            BackgroundColor = Colors.White
        };
        _ = settings.BackgroundColor.Should().Be(Colors.White);

        settings.BackgroundColor = Colors.Black;
        _ = settings.BackgroundColor.Should().Be(Colors.Black);

        settings.BackgroundColor = Colors.Red;
        _ = settings.BackgroundColor.Should().Be(Colors.Red);
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(200.0)]
    public void VisualSettings_FocusWidth_ShouldAcceptValidValues(double focusWidth)
    {
        // Arrange
        VisualSettings settings = new()
        {
            // Act
            FocusWidth = focusWidth
        };

        // Assert
        _ = settings.FocusWidth.Should().Be(focusWidth);
    }
}
