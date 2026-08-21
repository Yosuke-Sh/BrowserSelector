// <copyright file="UrlRuleImportTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// URLルール編集ダイアログの「現在のURLを取り込む」機能に関するテスト。
/// メイン画面のURLが設定画面へどう伝播するかを検証する。
/// UrlRuleEditDialog自体はWPF WindowでありSingle-Threaded Apartmentを要求するため
/// （UnitTestsプロジェクトはSTA未構成）、CanImportCurrentUrlの判定ロジック検証は
/// UITests側のFlaUIベースのテストで別途担保する.
/// </summary>
public class UrlRuleImportTests
{
    [Fact]
    public void SettingsViewModel_WithCurrentUrl_ExposesCurrentUrl()
    {
        SettingsViewModel viewModel = CreateSettingsViewModel(currentUrl: "https://example.com/path");

        viewModel.CurrentUrl.Should().Be("https://example.com/path");
    }

    [Fact]
    public void SettingsViewModel_WithoutCurrentUrl_CurrentUrlIsNull()
    {
        SettingsViewModel viewModel = CreateSettingsViewModel(currentUrl: null);

        viewModel.CurrentUrl.Should().BeNull();
    }

    private static SettingsViewModel CreateSettingsViewModel(string? currentUrl)
    {
        var mockSettingsService = new Mock<ISettingsService>();
        var mockBrowserService = new Mock<IBrowserService>();
        var mockLocalizationService = new Mock<ILocalizationService>();
        var mockCustomLanguageService = new Mock<ICustomLanguageService>();
        var mockUrlRuleService = new Mock<IUrlRuleService>();
        var mockLogService = new Mock<ILogService>();

        _ = mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());
        _ = mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());
        _ = mockSettingsService.Setup(x => x.LoadLogSettingsAsync()).ReturnsAsync(new LogSettings());
        _ = mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);
        _ = mockUrlRuleService.Setup(x => x.GetAllRulesAsync()).ReturnsAsync([]);
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync()).ReturnsAsync([]);

        return new SettingsViewModel(
            mockSettingsService.Object,
            mockBrowserService.Object,
            mockLocalizationService.Object,
            mockCustomLanguageService.Object,
            mockUrlRuleService.Object,
            mockLogService.Object,
            currentUrl: currentUrl);
    }
}
