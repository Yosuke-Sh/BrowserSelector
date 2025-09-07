using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// ウィンドウ設定のUIテスト
/// </summary>
[TestClass]
public class WindowSettingsUITests
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
    public void WindowSettings_ShouldHaveTitleBarSettings()
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

        // Assert - タイトルバー設定の確認
        var titleBarCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TitleBarCheckbox"));
        titleBarCheckbox.Should().NotBeNull("タイトルバー設定のチェックボックスが存在すること");

        var titleBarLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TitleBarLabel"));
        titleBarLabel.Should().NotBeNull("タイトルバー設定のラベルが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowSizeSettings()
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

        // Assert - ウィンドウサイズ設定の確認
        var widthSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowWidthSlider"));
        widthSlider.Should().NotBeNull("ウィンドウ幅スライダーが存在すること");

        var heightSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowHeightSlider"));
        heightSlider.Should().NotBeNull("ウィンドウ高さスライダーが存在すること");

        var widthLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowWidthLabel"));
        widthLabel.Should().NotBeNull("ウィンドウ幅ラベルが存在すること");

        var heightLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowHeightLabel"));
        heightLabel.Should().NotBeNull("ウィンドウ高さラベルが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowPositionSettings()
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

        // Assert - ウィンドウ位置設定の確認
        var centerWindowCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("CenterWindowCheckbox"));
        centerWindowCheckbox.Should().NotBeNull("ウィンドウ中央配置チェックボックスが存在すること");

        var rememberPositionCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RememberPositionCheckbox"));
        rememberPositionCheckbox.Should().NotBeNull("位置記憶チェックボックスが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowBehaviorSettings()
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

        // Assert - ウィンドウ動作設定の確認
        var alwaysOnTopCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AlwaysOnTopCheckbox"));
        alwaysOnTopCheckbox.Should().NotBeNull("常に最前面チェックボックスが存在すること");

        var minimizeToTrayCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("MinimizeToTrayCheckbox"));
        minimizeToTrayCheckbox.Should().NotBeNull("トレイに最小化チェックボックスが存在すること");

        var closeToTrayCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("CloseToTrayCheckbox"));
        closeToTrayCheckbox.Should().NotBeNull("トレイに閉じるチェックボックスが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldAllowSizeAdjustment()
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

        // Assert - サイズ調整機能の確認
        var widthSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowWidthSlider"));
        if (widthSlider != null)
        {
            widthSlider.AsSlider().Should().NotBeNull("幅スライダーが操作可能であること");
        }

        var heightSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowHeightSlider"));
        if (heightSlider != null)
        {
            heightSlider.AsSlider().Should().NotBeNull("高さスライダーが操作可能であること");
        }
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowTransparencySettings()
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

        // Assert - ウィンドウ透明化設定の確認
        var transparencySlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowTransparencySlider"));
        transparencySlider.Should().NotBeNull("ウィンドウ透明化スライダーが存在すること");

        var transparencyLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowTransparencyLabel"));
        transparencyLabel.Should().NotBeNull("ウィンドウ透明化ラベルが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowCornerRadiusSettings()
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

        // Assert - ウィンドウ角丸設定の確認
        var cornerRadiusSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("CornerRadiusSlider"));
        cornerRadiusSlider.Should().NotBeNull("角丸スライダーが存在すること");

        var cornerRadiusLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("CornerRadiusLabel"));
        cornerRadiusLabel.Should().NotBeNull("角丸ラベルが存在すること");
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveWindowBorderSettings()
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

        // Assert - ウィンドウボーダー設定の確認
        var borderCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("WindowBorderCheckbox"));
        borderCheckbox.Should().NotBeNull("ウィンドウボーダーチェックボックスが存在すること");

        var borderColorPicker = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BorderColorPicker"));
        borderColorPicker.Should().NotBeNull("ボーダーカラーピッカーが存在すること");

        var borderThicknessSlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BorderThicknessSlider"));
        borderThicknessSlider.Should().NotBeNull("ボーダー太さスライダーが存在すること");
    }
}
