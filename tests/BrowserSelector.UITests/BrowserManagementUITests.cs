using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// ブラウザ管理のUIテスト
/// </summary>
[TestClass]
public class BrowserManagementUITests
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
    public void BrowserManagement_ShouldHaveAddBrowserButton()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザ追加ボタンの確認
        var addBrowserButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddBrowserButton"));
        addBrowserButton.Should().NotBeNull("ブラウザ追加ボタンが存在すること");

        var addBrowserLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddBrowserLabel"));
        addBrowserLabel.Should().NotBeNull("ブラウザ追加ラベルが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveBrowserList()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザリストの確認
        var browserList = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserList"));
        browserList.Should().NotBeNull("ブラウザリストが存在すること");

        var browserListBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserListBox"));
        browserListBox.Should().NotBeNull("ブラウザリストボックスが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveEditBrowserButton()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザ編集ボタンの確認
        var editBrowserButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("EditBrowserButton"));
        editBrowserButton.Should().NotBeNull("ブラウザ編集ボタンが存在すること");

        var editBrowserLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("EditBrowserLabel"));
        editBrowserLabel.Should().NotBeNull("ブラウザ編集ラベルが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveDeleteBrowserButton()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザ削除ボタンの確認
        var deleteBrowserButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DeleteBrowserButton"));
        deleteBrowserButton.Should().NotBeNull("ブラウザ削除ボタンが存在すること");

        var deleteBrowserLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DeleteBrowserLabel"));
        deleteBrowserLabel.Should().NotBeNull("ブラウザ削除ラベルが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveBrowserDetectionButton()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザ検出ボタンの確認
        var detectBrowsersButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DetectBrowsersButton"));
        detectBrowsersButton.Should().NotBeNull("ブラウザ検出ボタンが存在すること");

        var detectBrowsersLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DetectBrowsersLabel"));
        detectBrowsersLabel.Should().NotBeNull("ブラウザ検出ラベルが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveDefaultBrowserSettings()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - デフォルトブラウザ設定の確認
        var defaultBrowserComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DefaultBrowserComboBox"));
        defaultBrowserComboBox.Should().NotBeNull("デフォルトブラウザコンボボックスが存在すること");

        var defaultBrowserLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DefaultBrowserLabel"));
        defaultBrowserLabel.Should().NotBeNull("デフォルトブラウザラベルが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveBrowserIconSettings()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザアイコン設定の確認
        var browserIconButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserIconButton"));
        browserIconButton.Should().NotBeNull("ブラウザアイコンボタンが存在すること");

        var browserIconLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserIconLabel"));
        browserIconLabel.Should().NotBeNull("ブラウザアイコンラベルが存在すること");

        var iconScaleSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("IconScaleSlider"));
        iconScaleSlider.Should().NotBeNull("アイコンスケールスライダーが存在すること");
    }

    [TestMethod]
    public void BrowserManagement_ShouldHaveBrowserPathSettings()
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

        // ブラウザタブをクリック
        var browserTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserTab"));
        if (browserTab == null)
        {
            Assert.Inconclusive("ブラウザタブが見つかりません");
            return;
        }

        browserTab.Click();
        Thread.Sleep(500);

        // Assert - ブラウザパス設定の確認
        var browserPathTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserPathTextBox"));
        browserPathTextBox.Should().NotBeNull("ブラウザパステキストボックスが存在すること");

        var browserPathLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserPathLabel"));
        browserPathLabel.Should().NotBeNull("ブラウザパスラベルが存在すること");

        var browseButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowseButton"));
        browseButton.Should().NotBeNull("参照ボタンが存在すること");
    }
}
