using BrowserSelector.Core.Models;
using BrowserSelector.Core.Enums;
using FluentAssertions;

namespace BrowserSelector.CoreTests;

/// <summary>
/// Coreプロジェクト専用のテストクラス
/// ドメインモデルのテストを重点的に実施
/// </summary>
public class CoreModelsTests
{
    [Fact]
    public void Browser_WithValidData_ShouldBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome,
            DisplayOrder = 1
        };

        // Act & Assert
        browser.IsValid.Should().BeTrue();
        browser.DisplayName.Should().Be("Google Chrome");
        browser.UseCount.Should().Be(0);
    }

    [Fact]
    public void Browser_WithEmptyName_ShouldNotBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse();
        browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void Browser_WithEmptyExecutablePath_ShouldNotBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = "",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncreaseCount()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        var initialCount = browser.UseCount;

        // Act
        browser.IncrementUseCount();

        // Assert
        browser.UseCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        var originalBrowser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome,
            DisplayOrder = 1
        };

        // Act
        var clonedBrowser = originalBrowser.Clone();

        // Assert
        clonedBrowser.Should().NotBeSameAs(originalBrowser);
        clonedBrowser.Should().BeEquivalentTo(originalBrowser, options => options.Excluding(b => b.Id));
    }

    [Fact]
    public void AppSettings_WithDefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.Language.Should().Be("en-US"); // デフォルト言語
        settings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void AppSettings_WithCustomValues_ShouldBeValid()
    {
        // Arrange
        var settings = new AppSettings
        {
            Language = "en-US",
            CloseAfterUrlRuleMatch = true
        };

        // Act & Assert
        settings.Language.Should().Be("en-US");
        settings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void VisualSettings_WithDefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var settings = new VisualSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
    }

    [Fact]
    public void VisualSettings_WithCustomValues_ShouldBeValid()
    {
        // Arrange
        var customColor = System.Windows.Media.Colors.Blue;
        var settings = new VisualSettings
        {
            BackgroundColor = customColor
        };

        // Act & Assert
        settings.BackgroundColor.Should().Be(customColor);
    }

    [Fact]
    public void UrlRule_WithValidData_ShouldBeValid()
    {
        // Arrange
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            IsEnabled = true
        };

        // Act & Assert
        rule.Should().NotBeNull();
        rule.Pattern.Should().Be(".*\\.google\\.com.*");
        rule.BrowserName.Should().Be("Google Chrome");
        rule.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void UrlRule_WithEmptyPattern_ShouldNotBeValid()
    {
        // Arrange
        var rule = new UrlRule
        {
            Pattern = "",
            BrowserName = "Google Chrome",
            IsEnabled = true
        };

        // Act & Assert
        rule.Pattern.Should().Be("");
        rule.BrowserName.Should().Be("Google Chrome");
    }

    [Fact]
    public void LogSettings_WithDefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var settings = new LogSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.LogLevel.Should().Be(LogLevel.Information);
        settings.EnableFileLogging.Should().BeTrue();
    }

    [Fact]
    public void LogSettings_WithCustomValues_ShouldBeValid()
    {
        // Arrange
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            EnableFileLogging = false
        };

        // Act & Assert
        settings.LogLevel.Should().Be(LogLevel.Information);
        settings.EnableFileLogging.Should().BeFalse();
    }

    [Theory]
    [InlineData(BrowserType.Chrome, "Chrome")]
    [InlineData(BrowserType.Firefox, "Firefox")]
    [InlineData(BrowserType.Edge, "Edge")]
    [InlineData(BrowserType.Safari, "Safari")]
    [InlineData(BrowserType.Opera, "Opera")]
    [InlineData(BrowserType.Custom, "Custom")]
    public void BrowserType_ShouldHaveCorrectStringRepresentation(BrowserType browserType, string expectedString)
    {
        // Act & Assert
        browserType.ToString().Should().Be(expectedString);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "Debug")]
    [InlineData(LogLevel.Information, "Information")]
    [InlineData(LogLevel.Warning, "Warning")]
    [InlineData(LogLevel.Error, "Error")]
    [InlineData(LogLevel.Critical, "Critical")]
    public void LogLevel_ShouldHaveCorrectStringRepresentation(LogLevel logLevel, string expectedString)
    {
        // Act & Assert
        logLevel.ToString().Should().Be(expectedString);
    }
}
