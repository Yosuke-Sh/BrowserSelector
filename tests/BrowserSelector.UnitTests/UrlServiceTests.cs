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
        var inputUrl = "example.com";
        var expectedUrl = "https://example.com";

        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        var result = await _urlService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var inputUrl = "http://example.com";

        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        var result = await _urlService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var inputUrl = "https://example.com";

        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        var result = await _urlService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var inputUrl = "";

        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        var result = await _urlService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task NormalizeUrlAsync_WithWhitespaceUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var inputUrl = "   ";

        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new BrowserSelector.Core.Models.AppSettings());

        // Act
        var result = await _urlService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithValidHttpUrl_ShouldReturnTrue()
    {
        // Arrange
        var url = "http://example.com";

        // Act
        var result = await _urlService.ValidateUrlAsync(url);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithValidHttpsUrl_ShouldReturnTrue()
    {
        // Arrange
        var url = "https://example.com";

        // Act
        var result = await _urlService.ValidateUrlAsync(url);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithInvalidUrl_ShouldReturnFalse()
    {
        // Arrange
        var url = "invalid-url";

        // Act
        var result = await _urlService.ValidateUrlAsync(url);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUrlAsync_WithEmptyUrl_ShouldReturnFalse()
    {
        // Arrange
        var url = "";

        // Act
        var result = await _urlService.ValidateUrlAsync(url);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExtractDomain_WithValidUrl_ShouldReturnDomain()
    {
        // Arrange
        var url = "https://example.com/path";
        var expectedDomain = "example.com";

        // Act
        var result = _urlService.ExtractDomain(url);

        // Assert
        result.Should().Be(expectedDomain);
    }

    [Fact]
    public void ExtractDomain_WithUrlWithoutProtocol_ShouldReturnDomain()
    {
        // Arrange
        var url = "example.com/path";
        var expectedDomain = "example.com";

        // Act
        var result = _urlService.ExtractDomain(url);

        // Assert
        result.Should().Be(expectedDomain);
    }

    [Fact]
    public void ExtractDomain_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var url = "";

        // Act
        var result = _urlService.ExtractDomain(url);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AddProtocolIfNeeded_WithUrlWithoutProtocol_ShouldAddHttps()
    {
        // Arrange
        var url = "example.com";
        var expectedUrl = "https://example.com";

        // Act
        var result = _urlService.AddProtocolIfNeeded(url);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var url = "http://example.com";

        // Act
        var result = _urlService.AddProtocolIfNeeded(url);

        // Assert
        result.Should().Be(url);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var url = "https://example.com";

        // Act
        var result = _urlService.AddProtocolIfNeeded(url);

        // Assert
        result.Should().Be(url);
    }

    [Fact]
    public void AddProtocolIfNeeded_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var url = "";

        // Act
        var result = _urlService.AddProtocolIfNeeded(url);

        // Assert
        result.Should().BeEmpty();
    }
}
