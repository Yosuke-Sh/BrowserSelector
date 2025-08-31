using BrowserSelector.Infrastructure.Localization;
using FluentAssertions;
using System.Globalization;
using Xunit;

namespace BrowserSelector.UnitTests;

public class LocalizationServiceTests
{
    [Fact]
    public void LocalizationService_GetString_ShouldReturnLocalizedString()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var result = service.GetString("Common.OK");

        // Assert
        result.Should().Be("OK");
    }

    [Fact]
    public void LocalizationService_GetStringWithArgs_ShouldFormatString()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var result = service.GetString("Common.Save", "test");

        // Assert
        result.Should().Be("保存");
    }

    [Fact]
    public void LocalizationService_SetLanguage_ShouldChangeCulture()
    {
        // Arrange
        var service = new LocalizationService();
        var englishCulture = new CultureInfo("en-US");
        var languageChanged = false;
        service.LanguageChanged += (sender, e) => languageChanged = true;

        // Act
        service.SetLanguage(englishCulture);

        // Assert
        service.CurrentCulture.Should().Be(englishCulture);
        languageChanged.Should().BeTrue();
    }

    [Fact]
    public void LocalizationService_SupportedLanguages_ShouldContainExpectedLanguages()
    {
        // Arrange
        var service = new LocalizationService();

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
        var service = new LocalizationService();
        var unknownKey = "Unknown.Key";

        // Act
        var result = service.GetString(unknownKey);

        // Assert
        result.Should().Be(unknownKey);
    }
}
