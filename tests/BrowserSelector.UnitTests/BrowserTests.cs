using BrowserSelector.Core.Models;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class BrowserTests
{
    [Fact]
    public void Browser_WithValidData_ShouldBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeTrue();
        _ = browser.DisplayName.Should().Be("Google Chrome");
    }

    [Fact]
    public void Browser_WithEmptyName_ShouldNotBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse();
        _ = browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void Browser_WithEmptyExecutablePath_ShouldNotBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Google Chrome",
            ExecutablePath = "",
            Type = BrowserType.Chrome
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncreaseCount()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        int initialCount = browser.UseCount;

        // Act
        browser.IncrementUseCount();

        // Assert
        _ = browser.UseCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        Browser original = new()
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act
        Browser cloned = original.Clone();

        // Assert
        _ = cloned.Should().NotBeSameAs(original);
        _ = cloned.Name.Should().Be(original.Name);
        _ = cloned.ExecutablePath.Should().Be(original.ExecutablePath);
        _ = cloned.Type.Should().Be(original.Type);
        _ = cloned.Id.Should().NotBe(original.Id);
    }
}
