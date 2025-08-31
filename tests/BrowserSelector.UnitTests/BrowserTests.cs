using BrowserSelector.Core.Models;
using FluentAssertions;
using Xunit;

namespace BrowserSelector.UnitTests;

public class BrowserTests
{
    [Fact]
    public void Browser_WithValidData_ShouldBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        browser.IsValid.Should().BeTrue();
        browser.DisplayName.Should().Be("Google Chrome");
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
        var original = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act
        var cloned = original.Clone();

        // Assert
        cloned.Should().NotBeSameAs(original);
        cloned.Name.Should().Be(original.Name);
        cloned.ExecutablePath.Should().Be(original.ExecutablePath);
        cloned.Type.Should().Be(original.Type);
        cloned.Id.Should().NotBe(original.Id);
    }
}
