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


}

