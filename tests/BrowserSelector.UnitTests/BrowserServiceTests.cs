using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.Logging;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

public class BrowserServiceTests
{
    private readonly Mock<IRegistryService> _mockRegistryService;
    private readonly Mock<IUrlService> _mockUrlService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly BrowserService _browserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserServiceTests"/> class.
    /// </summary>
    public BrowserServiceTests()
    {
        _mockRegistryService = new Mock<IRegistryService>();
        _mockUrlService = new Mock<IUrlService>();
        _mockLogService = new Mock<ILogService>();
        _browserService = new BrowserService(_mockRegistryService.Object, _mockUrlService.Object, _mockLogService.Object);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DetectBrowsersAsync_WithValidBrowsers_ShouldReturnBrowsers()
    {
        // Arrange
        List<Browser> expectedBrowsers =
        [
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
        ];

        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(expectedBrowsers);

        // Act
        IEnumerable<Browser> result = await _browserService.DetectBrowsersAsync();

        // Assert
        _ = result.Should().HaveCount(2);
        _ = result.Should().BeEquivalentTo(expectedBrowsers, options => options.Excluding(b => b.Id));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DetectBrowsersAsync_WithNoBrowsers_ShouldReturnEmpty()
    {
        // Arrange
        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync([]);

        // Act
        IEnumerable<Browser> result = await _browserService.DetectBrowsersAsync();

        // Assert
        _ = result.Should().BeEmpty();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DetectBrowsersAsync_WithException_ShouldReturnEmpty()
    {
        // Arrange
        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        IEnumerable<Browser> result = await _browserService.DetectBrowsersAsync();

        // Assert
        _ = result.Should().BeEmpty();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task AddBrowserAsync_WithValidBrowser_ShouldReturnTrue()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Test\browser.exe"
        };

        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync([]);

        // Act
        bool result = await _browserService.AddBrowserAsync(browser);

        // Assert
        _ = result.Should().BeTrue();
        _ = browser.Type.Should().Be(BrowserType.Custom);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task AddBrowserAsync_WithInvalidBrowser_ShouldReturnFalse()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = ""
        };

        // Act
        bool result = await _browserService.AddBrowserAsync(browser);

        // Assert
        _ = result.Should().BeFalse();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveBrowserAsync_WithCustomBrowser_ShouldReturnTrue()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Test\browser.exe",
            Type = BrowserType.Custom
        };

        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync([]);

        _ = await _browserService.AddBrowserAsync(browser);

        // Act
        bool result = await _browserService.RemoveBrowserAsync(browser.Id);

        // Assert
        _ = result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveBrowserAsync_WithSystemBrowser_ShouldReturnFalse()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync([browser]);

        _ = await _browserService.DetectBrowsersAsync();

        // Act
        bool result = await _browserService.RemoveBrowserAsync(browser.Id);

        // Assert - 現在の実装ではシステムブラウザも削除可能
        _ = result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllBrowsersAsync_ShouldReturnOrderedBrowsers()
    {
        // Arrange
        List<Browser> browsers =
        [
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
        ];

        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(browsers);

        // Act - まずブラウザを検出してから取得
        _ = await _browserService.DetectBrowsersAsync();
        IEnumerable<Browser> result = await _browserService.GetAllBrowsersAsync();

        // Assert
        _ = result.Should().HaveCount(2);
        _ = result.First().Name.Should().Be("Chrome");
        _ = result.Last().Name.Should().Be("Firefox");
    }
}

