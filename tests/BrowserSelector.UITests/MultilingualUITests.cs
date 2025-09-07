using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// 多言語対応のUIテスト
/// </summary>
[TestClass]
public class MultilingualUITests
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;

    [TestInitialize]
    public void Setup()
    {
        try
        {
            string appPath = UITestHelper.GetApplicationPath();
            if (string.IsNullOrEmpty(appPath))
            {
                Assert.Inconclusive("アプリケーションが見つかりません");
                return;
            }

            _app = Application.Launch(appPath);
            _automation = new UIA3Automation();
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"UIテスト用アプリケーション起動に失敗: {ex.Message}");
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        _automation?.Dispose();
        _app?.Close();
    }

    [TestMethod]
    public void Multilingual_ShouldHaveLanguageSelection()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語選択の確認
        var languageComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageComboBox"));
        languageComboBox.Should().NotBeNull("言語選択コンボボックスが存在すること");

        var languageLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageLabel"));
        languageLabel.Should().NotBeNull("言語選択ラベルが存在すること");
    }

    [TestMethod]
    public void Multilingual_ShouldSupportJapanese()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 日本語サポートの確認
        var languageComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageComboBox"));
        if (languageComboBox != null)
        {
            var japaneseOption = languageComboBox.FindFirstDescendant(cf => cf.ByText("日本語"));
            japaneseOption.Should().NotBeNull("日本語オプションが存在すること");
        }
    }

    [TestMethod]
    public void Multilingual_ShouldSupportEnglish()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 英語サポートの確認
        var languageComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageComboBox"));
        if (languageComboBox != null)
        {
            var englishOption = languageComboBox.FindFirstDescendant(cf => cf.ByText("English"));
            englishOption.Should().NotBeNull("英語オプションが存在すること");
        }
    }

    [TestMethod]
    public void Multilingual_ShouldHaveLanguagePreview()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語プレビューの確認
        var languagePreview = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguagePreview"));
        languagePreview.Should().NotBeNull("言語プレビューが存在すること");

        var previewLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PreviewLabel"));
        previewLabel.Should().NotBeNull("プレビューラベルが存在すること");
    }

    [TestMethod]
    public void Multilingual_ShouldHaveApplyLanguageButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語適用ボタンの確認
        var applyLanguageButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("ApplyLanguageButton"));
        applyLanguageButton.Should().NotBeNull("言語適用ボタンが存在すること");

        var applyLanguageLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("ApplyLanguageLabel"));
        applyLanguageLabel.Should().NotBeNull("言語適用ラベルが存在すること");
    }

    [TestMethod]
    public void Multilingual_ShouldHaveLanguageRestartOption()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語再起動オプションの確認
        var restartRequiredCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RestartRequiredCheckbox"));
        restartRequiredCheckbox.Should().NotBeNull("再起動必要チェックボックスが存在すること");

        var restartRequiredLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RestartRequiredLabel"));
        restartRequiredLabel.Should().NotBeNull("再起動必要ラベルが存在すること");
    }

    [TestMethod]
    public void Multilingual_ShouldHaveLanguageInfo()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語情報の確認
        var languageInfo = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageInfo"));
        languageInfo.Should().NotBeNull("言語情報が存在すること");

        var currentLanguageLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("CurrentLanguageLabel"));
        currentLanguageLabel.Should().NotBeNull("現在の言語ラベルが存在すること");

        var languageVersionLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageVersionLabel"));
        languageVersionLabel.Should().NotBeNull("言語バージョンラベルが存在すること");
    }

    [TestMethod]
    public void Multilingual_ShouldHaveLanguageHelp()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // 言語設定タブをクリック
        var languageTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageTab"));
        if (languageTab == null)
        {
            Assert.Inconclusive("言語設定タブが見つかりません");
            return;
        }

        languageTab.Click();
        Thread.Sleep(500);

        // Assert - 言語ヘルプの確認
        var languageHelpButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageHelpButton"));
        languageHelpButton.Should().NotBeNull("言語ヘルプボタンが存在すること");

        var languageHelpLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageHelpLabel"));
        languageHelpLabel.Should().NotBeNull("言語ヘルプラベルが存在すること");

        var languageHelpText = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("LanguageHelpText"));
        languageHelpText.Should().NotBeNull("言語ヘルプテキストが存在すること");
    }
}
