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

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModelTests"/> class.
    /// </summary>
    public SettingsViewModelTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _mockBrowserService = new Mock<IBrowserService>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();

        // デフォルトの設定を設定
        _ = _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings
            {
                Language = "en-US", // デフォルト言語を英語に変更
                CloseAfterUrlRuleMatch = true
            });

        _ = _mockSettingsService
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

        _ = _mockBrowserService
            .Setup(x => x.GetAllBrowsersAsync())
            .ReturnsAsync([]);

        _ = _mockUrlRuleService
            .Setup(x => x.GetAllRulesAsync())
            .ReturnsAsync([]);

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
        _ = _viewModel.Should().NotBeNull();
        _ = _viewModel.AppSettings.Should().NotBeNull();
        _ = _viewModel.VisualSettings.Should().NotBeNull();
        _ = _viewModel.AvailableLanguages.Should().HaveCount(2);
        _ = _viewModel.AvailableLogLevels.Should().HaveCount(6);
    }

    [Fact]
    public void SettingsViewModel_AvailableLanguages_ShouldContainJapaneseAndEnglish()
    {
        // Assert
        _ = _viewModel.AvailableLanguages.Should().Contain(l => l.CultureCode == "ja-JP" && l.DisplayName == "日本語");
        _ = _viewModel.AvailableLanguages.Should().Contain(l => l.CultureCode == "en-US" && l.DisplayName == "English");
    }

    [Fact]
    public void SettingsViewModel_AvailableLogLevels_ShouldContainAllLogLevels()
    {
        // Assert - ログレベルは英語で表示される
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Trace");
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Debug");
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Information");
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Warning");
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Error");
        _ = _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "LogLevel.Critical");
    }

    [Fact]
    public void SettingsViewModel_SelectedLanguage_ShouldBeSetToEnglishByDefault()
    {
        // Assert - デフォルト言語は英語
        _ = _viewModel.SelectedLanguage.Should().NotBeNull();
        _ = _viewModel.SelectedLanguage!.CultureCode.Should().Be("en-US");
    }

    [Fact]
    public void SettingsViewModel_RefreshBrowsersCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.RefreshBrowsersCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ResetSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.ResetSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ImportSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.ImportSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_ExportSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.ExportSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_SaveSettingsCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.SaveSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_CancelCommand_ShouldBeAvailable()
    {
        // Assert
        _ = _viewModel.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_DefaultValues_ShouldBeSetCorrectly()
    {
        // Assert
        _ = _viewModel.VisualSettings.ShowFocusIndicator.Should().BeTrue();
        _ = _viewModel.VisualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        _ = _viewModel.VisualSettings.FocusThickness.Should().Be(2.0);
        _ = _viewModel.VisualSettings.FocusWidth.Should().Be(100.0);
        _ = _viewModel.VisualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        _ = _viewModel.VisualSettings.IconScale.Should().Be(1.0);
        _ = _viewModel.VisualSettings.ShowLogo.Should().BeTrue();
        _ = _viewModel.VisualSettings.ShowUrlInput.Should().BeTrue();
        _ = _viewModel.VisualSettings.BrowserButtonWidth.Should().Be(120.0);
        _ = _viewModel.VisualSettings.BrowserButtonHeight.Should().Be(90.0);
        _ = _viewModel.VisualSettings.BrowserButtonOpacity.Should().Be(1.0);
        _ = _viewModel.VisualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        _ = _viewModel.VisualSettings.ShowBrowserName.Should().BeTrue();
        _ = _viewModel.VisualSettings.BrowserIconSize.Should().Be(32.0);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SettingsViewModel_RefreshBrowsers_ShouldCallBrowserService()
    {
        // Arrange
        List<Browser> testBrowsers =
        [
            new Browser { Name = "Test Browser 1", Type = BrowserType.Chrome },
            new Browser { Name = "Test Browser 2", Type = BrowserType.Firefox }
        ];

        _ = _mockBrowserService
            .Setup(x => x.DetectBrowsersAsync())
            .ReturnsAsync(testBrowsers);

        // Act
        await _viewModel.RefreshBrowsersCommand.ExecuteAsync(null);

        // Assert - RefreshBrowsersはDetectBrowsersAsyncを呼び出す
        _mockBrowserService.Verify(x => x.DetectBrowsersAsync(), Times.Once);
        _ = _viewModel.DetectedBrowsers.Should().HaveCount(2);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SettingsViewModel_ResetSettings_ShouldCallSettingsService()
    {
        // Arrange
        _ = _mockSettingsService
            .Setup(x => x.ResetSettingsAsync())
            .ReturnsAsync(true);

        // Act
        await _viewModel.ResetSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(x => x.ResetSettingsAsync(), Times.Once);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SettingsViewModel_SaveSettings_ShouldCallSettingsService()
    {
        // Arrange
        _ = _mockSettingsService
            .Setup(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()))
            .ReturnsAsync(true);

        _ = _mockSettingsService
            .Setup(x => x.SaveVisualSettingsAsync(It.IsAny<VisualSettings>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()), Times.Once);
        _mockSettingsService.Verify(x => x.SaveVisualSettingsAsync(It.IsAny<VisualSettings>()), Times.Once);
    }
}
