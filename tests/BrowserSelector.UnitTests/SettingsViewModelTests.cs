using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

public class SettingsViewModelTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _mockBrowserService = new Mock<IBrowserService>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();

        // デフォルトの設定を設定
        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings
            {
                Language = "en-US", // デフォルト言語を英語に変更
                CloseAfterUrlRuleMatch = true
            });

        _mockSettingsService
            .Setup(x => x.LoadVisualSettingsAsync())
            .ReturnsAsync(new VisualSettings
            {
                // TODO: 削除されたプロパティのテストを更新
                // Opacity = 1.0,
                // CornerRadius = 0,
                // ShowTitleBar = true
                BackgroundColor = System.Windows.Media.Colors.White,
                // TODO: 削除されたMessageTextColorプロパティのテストを更新
            });

        _mockBrowserService
            .Setup(x => x.GetAllBrowsersAsync())
            .ReturnsAsync(new List<Browser>());

        _mockUrlRuleService
            .Setup(x => x.GetAllRulesAsync())
            .ReturnsAsync(new List<UrlRule>());

        _mockLogService = new Mock<ILogService>();

        _viewModel = new SettingsViewModel(
            _mockSettingsService.Object,
            _mockBrowserService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public void SettingsViewModel_Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        _viewModel.Should().NotBeNull();
        _viewModel.AppSettings.Should().NotBeNull();
        _viewModel.VisualSettings.Should().NotBeNull();
        _viewModel.AvailableLanguages.Should().HaveCount(2);
        _viewModel.AvailableLogLevels.Should().HaveCount(6);
    }

    [Fact]
    public void SettingsViewModel_AvailableLanguages_ShouldContainJapaneseAndEnglish()
    {
        // Assert
        _viewModel.AvailableLanguages.Should().Contain(l => l.CultureCode == "ja-JP" && l.DisplayName == "日本語");
        _viewModel.AvailableLanguages.Should().Contain(l => l.CultureCode == "en-US" && l.DisplayName == "English");
    }

    [Fact]
    public void SettingsViewModel_AvailableLogLevels_ShouldContainAllLogLevels()
    {
        // Assert - ログレベルは英語で表示される
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Trace");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Debug");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Information");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Warning");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Error");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Critical");
    }

    [Fact]
    public void SettingsViewModel_SelectedLanguage_ShouldBeSetToEnglishByDefault()
    {
        // Assert - デフォルト言語は英語
        _viewModel.SelectedLanguage.Should().NotBeNull();
        _viewModel.SelectedLanguage!.CultureCode.Should().Be("en-US");
    }

    [Fact]
    public void SettingsViewModel_RefreshBrowsersCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.RefreshBrowsersCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ResetSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.ResetSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ImportSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.ImportSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ExportSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.ExportSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_SaveSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.SaveSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_CancelCommand_ShouldBeAvailable()
    {
        // Assert
        _viewModel.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_DefaultValues_ShouldBeSetCorrectly()
    {
        // Assert
        _viewModel.VisualSettings.ShowFocusIndicator.Should().BeTrue();
        _viewModel.VisualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        _viewModel.VisualSettings.FocusThickness.Should().Be(2.0);
        _viewModel.VisualSettings.FocusWidth.Should().Be(100.0);
        _viewModel.VisualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        _viewModel.VisualSettings.IconScale.Should().Be(1.0);
        _viewModel.VisualSettings.ShowLogo.Should().BeTrue();
        _viewModel.VisualSettings.ShowUrlInput.Should().BeTrue();
        _viewModel.VisualSettings.BrowserButtonWidth.Should().Be(120.0);
        _viewModel.VisualSettings.BrowserButtonHeight.Should().Be(90.0);
        _viewModel.VisualSettings.BrowserButtonOpacity.Should().Be(1.0);
        _viewModel.VisualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        _viewModel.VisualSettings.ShowBrowserName.Should().BeTrue();
        _viewModel.VisualSettings.BrowserIconSize.Should().Be(32.0);
    }

    [Fact]
    public async Task SettingsViewModel_RefreshBrowsers_ShouldCallBrowserService()
    {
        // Arrange
        var testBrowsers = new List<Browser>
        {
            new Browser { Name = "Test Browser 1", Type = BrowserType.Chrome },
            new Browser { Name = "Test Browser 2", Type = BrowserType.Firefox }
        };

        _mockBrowserService
            .Setup(x => x.DetectBrowsersAsync())
            .ReturnsAsync(testBrowsers);

        // Act
        await _viewModel.RefreshBrowsersCommand.ExecuteAsync(null);

        // Assert - RefreshBrowsersはDetectBrowsersAsyncを呼び出す
        _mockBrowserService.Verify(x => x.DetectBrowsersAsync(), Times.Once);
        _viewModel.DetectedBrowsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task SettingsViewModel_ResetSettings_ShouldCallSettingsService()
    {
        // Arrange
        _mockSettingsService
            .Setup(x => x.ResetSettingsAsync())
            .ReturnsAsync(true);

        // Act
        await _viewModel.ResetSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(x => x.ResetSettingsAsync(), Times.Once);
    }

    [Fact]
    public async Task SettingsViewModel_SaveSettings_ShouldCallSettingsService()
    {
        // Arrange
        _mockSettingsService
            .Setup(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()))
            .ReturnsAsync(true);

        _mockSettingsService
            .Setup(x => x.SaveVisualSettingsAsync(It.IsAny<VisualSettings>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()), Times.Once);
        _mockSettingsService.Verify(x => x.SaveVisualSettingsAsync(It.IsAny<VisualSettings>()), Times.Once);
    }
}
