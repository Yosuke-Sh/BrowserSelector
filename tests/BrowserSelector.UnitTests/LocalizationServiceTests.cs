using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Localization;
using FluentAssertions;
using Moq;
using System.Globalization;

namespace BrowserSelector.UnitTests;

public class LocalizationServiceTests
{
    [Fact]
    public void LocalizationService_GetString_ShouldReturnLocalizedString()
    {
        // Arrange
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(
            [
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            ]);

        LocalizationService service = new(mockCustomLanguageService.Object);

        // Act
        string result = service.GetString("Common.OK");

        // Assert
        _ = result.Should().Be("OK");
    }

    [Fact]
    public void LocalizationService_GetStringWithArgs_ShouldFormatString()
    {
        // Arrange
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(
            [
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            ]);

        LocalizationService service = new(mockCustomLanguageService.Object);

        // Act
        string result = service.GetString("Common.Save", "test");

        // Assert
        _ = result.Should().Be("Save");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LocalizationService_SetLanguage_ShouldChangeCulture()
    {
        // Arrange
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(
            [
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            ]);

        LocalizationService service = new(mockCustomLanguageService.Object);
        CultureInfo japaneseCulture = new("ja-JP"); // 異なる言語を設定
        bool languageChanged = false;
        service.LanguageChanged += (sender, e) => languageChanged = true;

        // Act
        await service.SetLanguage(japaneseCulture);

        // Assert
        _ = service.CurrentCulture.Should().Be(japaneseCulture);
        _ = languageChanged.Should().BeTrue();
    }

    [Fact]
    public void LocalizationService_SupportedLanguages_ShouldContainExpectedLanguages()
    {
        // Arrange
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(
            [
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            ]);

        LocalizationService service = new(mockCustomLanguageService.Object);

        // Act
        List<CultureInfo> supportedLanguages = service.SupportedLanguages.ToList();

        // Assert
        _ = supportedLanguages.Should().HaveCount(2);
        _ = supportedLanguages.Should().Contain(l => l.Name == "ja-JP");
        _ = supportedLanguages.Should().Contain(l => l.Name == "en-US");
    }

    [Fact]
    public void LocalizationService_GetStringWithUnknownKey_ShouldReturnKey()
    {
        // Arrange
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync())
            .ReturnsAsync(
            [
                new LanguageInfo("en-US", "English"),
                new LanguageInfo("ja-JP", "日本語")
            ]);

        LocalizationService service = new(mockCustomLanguageService.Object);
        string unknownKey = "Unknown.Key";

        // Act
        string result = service.GetString(unknownKey);

        // Assert
        _ = result.Should().Be(unknownKey);
    }
}
