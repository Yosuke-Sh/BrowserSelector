using Xunit;
using FluentAssertions;
using Moq;
using BrowserSelector.Presentation.ViewModels;
using BrowserSelector.Core.Services;
using BrowserSelector.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrowserSelector.UnitTests;

public class SettingsViewModelTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _mockBrowserService = new Mock<IBrowserService>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();

        // デフォルトの設定を設定
        _mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings
            {
                Language = "ja-JP",
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
        // Assert
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "トレース");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "デバッグ");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "情報");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "警告");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "エラー");
        _viewModel.AvailableLogLevels.Should().Contain(l => l.DisplayName == "致命的エラー");
    }

    [Fact]
    public void SettingsViewModel_SelectedLanguage_ShouldBeSetToJapaneseByDefault()
    {
        // Assert
        _viewModel.SelectedLanguage.Should().NotBeNull();
        _viewModel.SelectedLanguage!.CultureCode.Should().Be("ja-JP");
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

    [Fact(Skip = "削除されたプロパティのテストを更新する必要があります")]
    public void SettingsViewModel_DefaultValues_ShouldBeSetCorrectly()
    {
        // Assert
        _viewModel.ShowFocusIndicator.Should().BeTrue();
        _viewModel.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        _viewModel.FocusThickness.Should().Be(2.0);
        // TODO: 削除されたプロパティのテストを更新
        // _viewModel.EnableKeyboardNavigation.Should().BeTrue();
        // _viewModel.EnableShortcuts.Should().BeTrue();
        // _viewModel.EnableScreenReaderSupport.Should().BeTrue();
        // _viewModel.ProvideDetailedDescriptions.Should().BeTrue();
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
            .Setup(x => x.GetAllBrowsersAsync())
            .ReturnsAsync(testBrowsers);

        // Act
        await _viewModel.RefreshBrowsersCommand.ExecuteAsync(null);

        // Assert
        _mockBrowserService.Verify(x => x.GetAllBrowsersAsync(), Times.Exactly(2)); // 初期化時とコマンド実行時
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
