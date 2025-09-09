using BrowserSelector.Presentation.Converters;
using FluentAssertions;
using System.Globalization;
using System.Windows;

namespace BrowserSelector.UnitTests;

/// <summary>
/// PresentationプロジェクトのConverterクラスのテスト.
/// </summary>
public class ConverterTests
{
    #region BoolToVisibilityConverter Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_True_ShouldReturnVisible()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_False_ShouldReturnCollapsed()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithInvertParameter_ShouldInvertValue()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object resultTrue = converter.Convert(true, typeof(Visibility), "Invert", CultureInfo.InvariantCulture);
        object resultFalse = converter.Convert(false, typeof(Visibility), "Invert", CultureInfo.InvariantCulture);

        // Assert
        _ = resultTrue.Should().Be(Visibility.Collapsed);
        _ = resultFalse.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithInvalidValue_ShouldReturnCollapsed()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.Convert("invalid", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithNullValue_ShouldReturnCollapsed()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Visible_ShouldReturnTrue()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(true);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Collapsed_ShouldReturnFalse()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Hidden_ShouldReturnFalse()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.ConvertBack(Visibility.Hidden, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithInvertParameter_ShouldInvertValue()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object resultVisible = converter.ConvertBack(Visibility.Visible, typeof(bool), "Invert", CultureInfo.InvariantCulture);
        object resultCollapsed = converter.ConvertBack(Visibility.Collapsed, typeof(bool), "Invert", CultureInfo.InvariantCulture);

        // Assert
        _ = resultVisible.Should().Be(false);
        _ = resultCollapsed.Should().Be(true);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithInvalidValue_ShouldReturnFalse()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.ConvertBack("invalid", typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(false);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithNullValue_ShouldReturnFalse()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();

        // Act
        object result = converter.ConvertBack(null!, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        _ = result.Should().Be(false);
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithDifferentCultures_ShouldWorkCorrectly()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        CultureInfo[] cultures = new[]
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN")
        };

        // Act & Assert
        foreach (CultureInfo? culture in cultures)
        {
            object resultTrue = converter.Convert(true, typeof(Visibility), null!, culture);
            object resultFalse = converter.Convert(false, typeof(Visibility), null!, culture);

            _ = resultTrue.Should().Be(Visibility.Visible);
            _ = resultFalse.Should().Be(Visibility.Collapsed);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithDifferentCultures_ShouldWorkCorrectly()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        CultureInfo[] cultures = new[]
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN")
        };

        // Act & Assert
        foreach (CultureInfo? culture in cultures)
        {
            object resultVisible = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, culture);
            object resultCollapsed = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, culture);

            _ = resultVisible.Should().Be(true);
            _ = resultCollapsed.Should().Be(false);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_Convert_WithCaseInsensitiveInvertParameter_ShouldWorkCorrectly()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        string[] invertParameters = new[] { "invert", "INVERT", "Invert", "InVeRt" };

        // Act & Assert
        foreach (string? parameter in invertParameters)
        {
            object result = converter.Convert(true, typeof(Visibility), parameter, CultureInfo.InvariantCulture);
            _ = result.Should().Be(Visibility.Collapsed);
        }
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_WithCaseInsensitiveInvertParameter_ShouldWorkCorrectly()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        string[] invertParameters = new[] { "invert", "INVERT", "Invert", "InVeRt" };

        // Act & Assert
        foreach (string? parameter in invertParameters)
        {
            object result = converter.ConvertBack(Visibility.Visible, typeof(bool), parameter, CultureInfo.InvariantCulture);
            _ = result.Should().Be(false);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void BoolToVisibilityConverter_Convert_MultipleCalls_ShouldPerformWell()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        int iterations = 10000;

        // Act
        DateTime startTime = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            _ = converter.Convert(i % 2 == 0, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        }
        DateTime endTime = DateTime.Now;

        // Assert
        TimeSpan duration = endTime - startTime;
        _ = duration.TotalMilliseconds.Should().BeLessThan(1000); // 1秒以内に完了すること
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_MultipleCalls_ShouldPerformWell()
    {
        // Arrange
        BoolToVisibilityConverter converter = new();
        int iterations = 10000;

        // Act
        DateTime startTime = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            Visibility visibility = i % 2 == 0 ? Visibility.Visible : Visibility.Collapsed;
            _ = converter.ConvertBack(visibility, typeof(bool), null!, CultureInfo.InvariantCulture);
        }
        DateTime endTime = DateTime.Now;

        // Assert
        TimeSpan duration = endTime - startTime;
        _ = duration.TotalMilliseconds.Should().BeLessThan(1000); // 1秒以内に完了すること
    }

    #endregion
}
