using BrowserSelector.Infrastructure.Localization;
using BrowserSelector.Core.Services;
using BrowserSelector.Core.Models;
using FluentAssertions;
using System.Globalization;
using Moq;

namespace BrowserSelector.UnitTests;

public class LocalizationServiceTests
{
    [Fact]
    public void LocalizationService_GetString_ShouldReturnLocalizedString()
    {
        // Arrange
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(new List<LanguageInfo>
            {
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            });
        
        var service = new LocalizationService(mockCustomLanguageService.Object);

        // Act
        var result = service.GetString("Common.OK");

        // Assert
        result.Should().Be("OK");
    }

    [Fact]
    public void LocalizationService_GetStringWithArgs_ShouldFormatString()
    {
        // Arrange
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(new List<LanguageInfo>
            {
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            });
        
        var service = new LocalizationService(mockCustomLanguageService.Object);

        // Act
        var result = service.GetString("Common.Save", "test");

        // Assert
        result.Should().Be("Save");
    }

    [Fact]
    public async Task LocalizationService_SetLanguage_ShouldChangeCulture()
    {
        // Arrange
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(new List<LanguageInfo>
            {
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            });
        
        var service = new LocalizationService(mockCustomLanguageService.Object);
        var japaneseCulture = new CultureInfo("ja-JP"); // 異なる言語を設定
        var languageChanged = false;
        service.LanguageChanged += (sender, e) => languageChanged = true;

        // Act
        await service.SetLanguage(japaneseCulture);

        // Assert
        service.CurrentCulture.Should().Be(japaneseCulture);
        languageChanged.Should().BeTrue();
    }

    [Fact]
    public void LocalizationService_SupportedLanguages_ShouldContainExpectedLanguages()
    {
        // Arrange
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(new List<LanguageInfo>
            {
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            });
        
        var service = new LocalizationService(mockCustomLanguageService.Object);

        // Act
        var supportedLanguages = service.SupportedLanguages.ToList();

        // Assert
        supportedLanguages.Should().HaveCount(2);
        supportedLanguages.Should().Contain(l => l.Name == "ja-JP");
        supportedLanguages.Should().Contain(l => l.Name == "en-US");
    }

    [Fact]
    public void LocalizationService_GetStringWithUnknownKey_ShouldReturnKey()
    {
        // Arrange
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(new List<LanguageInfo>
            {
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            });
        
        var service = new LocalizationService(mockCustomLanguageService.Object);
        var unknownKey = "Unknown.Key";

        // Act
        var result = service.GetString(unknownKey);

        // Assert
        result.Should().Be(unknownKey);
    }
}
