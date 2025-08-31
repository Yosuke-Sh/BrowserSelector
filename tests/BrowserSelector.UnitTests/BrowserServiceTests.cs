using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrowserSelector.UnitTests;

public class BrowserServiceTests
{
    private readonly Mock<IRegistryService> _mockRegistryService;
    private readonly Mock<IUrlService> _mockUrlService;
    private readonly BrowserService _browserService;

    public BrowserServiceTests()
    {
        _mockRegistryService = new Mock<IRegistryService>();
        _mockUrlService = new Mock<IUrlService>();
        _browserService = new BrowserService(_mockRegistryService.Object, _mockUrlService.Object);
    }

    [Fact]
    public async Task DetectBrowsersAsync_WithValidBrowsers_ShouldReturnBrowsers()
    {
        // Arrange
        var expectedBrowsers = new List<Browser>
        {
            new Browser
            {
                Name = "Google Chrome",
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                Type = BrowserType.Chrome,
                DisplayOrder = 1
            },
            new Browser
            {
                Name = "Mozilla Firefox",
                ExecutablePath = @"C:\Program Files\Mozilla Firefox\firefox.exe",
                Type = BrowserType.Firefox,
                DisplayOrder = 2
            }
        };

        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(expectedBrowsers);

        // Act
        var result = await _browserService.DetectBrowsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedBrowsers, options => options.Excluding(b => b.Id));
    }

    [Fact]
    public async Task DetectBrowsersAsync_WithNoBrowsers_ShouldReturnEmpty()
    {
        // Arrange
        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(new List<Browser>());

        // Act
        var result = await _browserService.DetectBrowsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectBrowsersAsync_WithException_ShouldReturnEmpty()
    {
        // Arrange
        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _browserService.DetectBrowsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddBrowserAsync_WithValidBrowser_ShouldReturnTrue()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Test\browser.exe"
        };

        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(new List<Browser>());

        // Act
        var result = await _browserService.AddBrowserAsync(browser);

        // Assert
        result.Should().BeTrue();
        browser.Type.Should().Be(BrowserType.Custom);
    }

    [Fact]
    public async Task AddBrowserAsync_WithInvalidBrowser_ShouldReturnFalse()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = ""
        };

        // Act
        var result = await _browserService.AddBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveBrowserAsync_WithCustomBrowser_ShouldReturnTrue()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Test\browser.exe",
            Type = BrowserType.Custom
        };

        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(new List<Browser>());

        await _browserService.AddBrowserAsync(browser);

        // Act
        var result = await _browserService.RemoveBrowserAsync(browser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveBrowserAsync_WithSystemBrowser_ShouldReturnFalse()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(new List<Browser> { browser });

        await _browserService.DetectBrowsersAsync();

        // Act
        var result = await _browserService.RemoveBrowserAsync(browser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllBrowsersAsync_ShouldReturnOrderedBrowsers()
    {
        // Arrange
        var browsers = new List<Browser>
        {
            new Browser
            {
                Name = "Firefox",
                ExecutablePath = @"C:\Firefox\firefox.exe",
                DisplayOrder = 2
            },
            new Browser
            {
                Name = "Chrome",
                ExecutablePath = @"C:\Chrome\chrome.exe",
                DisplayOrder = 1
            }
        };

        _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(browsers);

        // Act
        var result = await _browserService.GetAllBrowsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Chrome");
        result.Last().Name.Should().Be("Firefox");
    }
}

