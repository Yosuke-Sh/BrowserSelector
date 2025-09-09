using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

public class UrlServiceTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly UrlService _urlService;

    public UrlServiceTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _urlService = new UrlService(_mockSettingsService.Object);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithValidUrl_ShouldReturnNormalizedUrl()
    {
        // Arrange
        string inputUrl = "example.com";
        string expectedUrl = "https://example.com";

        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        string result = await _urlService.NormalizeUrlAsync(new Uri(inputUrl));

        // Assert
        _ = result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        string inputUrl = "http://example.com";

        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        string result = await _urlService.NormalizeUrlAsync(new Uri(inputUrl));

        // Assert
        _ = result.Should().Be(inputUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        string inputUrl = "https://example.com";

        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        string result = await _urlService.NormalizeUrlAsync(new Uri(inputUrl));

        // Assert
        _ = result.Should().Be(inputUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        string inputUrl = "";

        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        string result = await _urlService.NormalizeUrlAsync(new Uri(inputUrl));

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithWhitespaceUrl_ShouldReturnEmptyString()
    {
        // Arrange
        string inputUrl = "   ";

        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        string result = await _urlService.NormalizeUrlAsync(new Uri(inputUrl));

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithValidHttpUrl_ShouldReturnTrue()
    {
        // Arrange
        string url = "http://example.com";

        // Act
        bool result = await _urlService.ValidateUrlAsync(new Uri(url));

        // Assert
        _ = result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithValidHttpsUrl_ShouldReturnTrue()
    {
        // Arrange
        string url = "https://example.com";

        // Act
        bool result = await _urlService.ValidateUrlAsync(new Uri(url));

        // Assert
        _ = result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithInvalidUrl_ShouldReturnFalse()
    {
        // Arrange
        string url = "invalid-url";

        // Act
        bool result = await _urlService.ValidateUrlAsync(new Uri(url));

        // Assert
        _ = result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithEmptyUrl_ShouldReturnFalse()
    {
        // Arrange
        string url = "";

        // Act
        bool result = await _urlService.ValidateUrlAsync(new Uri(url));

        // Assert
        _ = result.Should().BeFalse();
    }

    [Fact]
    public void ExtractDomain_WithValidUrl_ShouldReturnDomain()
    {
        // Arrange
        string url = "https://example.com/path";
        string expectedDomain = "example.com";

        // Act
        string result = _urlService.ExtractDomain(new Uri(url));

        // Assert
        _ = result.Should().Be(expectedDomain);
    }

    [Fact]
    public void ExtractDomain_WithUrlWithoutProtocol_ShouldReturnDomain()
    {
        // Arrange
        string url = "example.com/path";
        string expectedDomain = "example.com";

        // Act
        string result = _urlService.ExtractDomain(new Uri(url));

        // Assert
        _ = result.Should().Be(expectedDomain);
    }

    [Fact]
    public void ExtractDomain_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        string url = "";

        // Act
        string result = _urlService.ExtractDomain(new Uri(url));

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public void AddProtocolIfNeeded_WithUrlWithoutProtocol_ShouldAddHttps()
    {
        // Arrange
        string url = "example.com";
        string expectedUrl = "https://example.com";

        // Act
        string result = _urlService.AddProtocolIfNeeded(new Uri(url));

        // Assert
        _ = result.Should().Be(expectedUrl);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        string url = "http://example.com";

        // Act
        string result = _urlService.AddProtocolIfNeeded(new Uri(url));

        // Assert
        _ = result.Should().Be(url);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        string url = "https://example.com";

        // Act
        string result = _urlService.AddProtocolIfNeeded(new Uri(url));

        // Assert
        _ = result.Should().Be(url);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        string url = "";

        // Act
        string result = _urlService.AddProtocolIfNeeded(new Uri(url));

        // Assert
        _ = result.Should().BeEmpty();
    }
}
