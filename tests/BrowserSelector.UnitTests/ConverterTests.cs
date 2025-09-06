using BrowserSelector.Presentation.Converters;
using System.Globalization;
using System.Windows;
using FluentAssertions;
using Xunit;

namespace BrowserSelector.UnitTests;

/// <summary>
/// PresentationプロジェクトのConverterクラスのテスト
/// </summary>
public class ConverterTests
{
    #region BoolToVisibilityConverter Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_True_ShouldReturnVisible()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_False_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithInvertParameter_ShouldInvertValue()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var resultTrue = converter.Convert(true, typeof(Visibility), "Invert", CultureInfo.InvariantCulture);
        var resultFalse = converter.Convert(false, typeof(Visibility), "Invert", CultureInfo.InvariantCulture);

        // Assert
        resultTrue.Should().Be(Visibility.Collapsed);
        resultFalse.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithInvalidValue_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert("invalid", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithNullValue_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Visible_ShouldReturnTrue()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Collapsed_ShouldReturnFalse()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Hidden_ShouldReturnFalse()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Hidden, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithInvertParameter_ShouldInvertValue()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var resultVisible = converter.ConvertBack(Visibility.Visible, typeof(bool), "Invert", CultureInfo.InvariantCulture);
        var resultCollapsed = converter.ConvertBack(Visibility.Collapsed, typeof(bool), "Invert", CultureInfo.InvariantCulture);

        // Assert
        resultVisible.Should().Be(false);
        resultCollapsed.Should().Be(true);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithInvalidValue_ShouldReturnFalse()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack("invalid", typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithNullValue_ShouldReturnFalse()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(null!, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithDifferentCultures_ShouldWorkCorrectly()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var cultures = new[]
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN")
        };

        // Act & Assert
        foreach (var culture in cultures)
        {
            var resultTrue = converter.Convert(true, typeof(Visibility), null!, culture);
            var resultFalse = converter.Convert(false, typeof(Visibility), null!, culture);

            resultTrue.Should().Be(Visibility.Visible);
            resultFalse.Should().Be(Visibility.Collapsed);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithDifferentCultures_ShouldWorkCorrectly()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var cultures = new[]
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN")
        };

        // Act & Assert
        foreach (var culture in cultures)
        {
            var resultVisible = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, culture);
            var resultCollapsed = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, culture);

            resultVisible.Should().Be(true);
            resultCollapsed.Should().Be(false);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithCaseInsensitiveInvertParameter_ShouldWorkCorrectly()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var invertParameters = new[] { "invert", "INVERT", "Invert", "InVeRt" };

        // Act & Assert
        foreach (var parameter in invertParameters)
        {
            var result = converter.Convert(true, typeof(Visibility), parameter, CultureInfo.InvariantCulture);
            result.Should().Be(Visibility.Collapsed);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithCaseInsensitiveInvertParameter_ShouldWorkCorrectly()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var invertParameters = new[] { "invert", "INVERT", "Invert", "InVeRt" };

        // Act & Assert
        foreach (var parameter in invertParameters)
        {
            var result = converter.ConvertBack(Visibility.Visible, typeof(bool), parameter, CultureInfo.InvariantCulture);
            result.Should().Be(false);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_MultipleCalls_ShouldPerformWell()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var iterations = 10000;

        // Act
        var startTime = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            converter.Convert(i % 2 == 0, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        }
        var endTime = DateTime.Now;

        // Assert
        var duration = endTime - startTime;
        duration.TotalMilliseconds.Should().BeLessThan(1000); // 1秒以内に完了すること
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_MultipleCalls_ShouldPerformWell()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();
        var iterations = 10000;

        // Act
        var startTime = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            var visibility = i % 2 == 0 ? Visibility.Visible : Visibility.Collapsed;
            converter.ConvertBack(visibility, typeof(bool), null!, CultureInfo.InvariantCulture);
        }
        var endTime = DateTime.Now;

        // Assert
        var duration = endTime - startTime;
        duration.TotalMilliseconds.Should().BeLessThan(1000); // 1秒以内に完了すること
    }

    #endregion
}
