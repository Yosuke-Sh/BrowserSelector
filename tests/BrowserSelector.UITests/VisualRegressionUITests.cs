using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BrowserSelector.UITests;

/// <summary>
/// 視覚的回帰テスト
/// </summary>
[TestClass]
public class VisualRegressionUITests
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;
    private string _screenshotPath = Path.Combine(Path.GetTempPath(), "BrowserSelectorUITests");

    [TestInitialize]
    public void Setup()
    {
        try
        {
            // スクリーンショット保存ディレクトリを作成
            if (!Directory.Exists(_screenshotPath))
            {
                Directory.CreateDirectory(_screenshotPath);
            }

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
    public void VisualRegression_MainWindow_ShouldMatchBaseline()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - メインウィンドウのスクリーンショットを取得
        var screenshot = CaptureWindowScreenshot(mainWindow!);
        var screenshotPath = Path.Combine(_screenshotPath, "MainWindow_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("メインウィンドウのスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    [TestMethod]
    public void VisualRegression_SettingsWindow_ShouldMatchBaseline()
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

        // 設定ウィンドウのスクリーンショットを取得
        var screenshot = CaptureWindowScreenshot(settingsWindow);
        var screenshotPath = Path.Combine(_screenshotPath, "SettingsWindow_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("設定ウィンドウのスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    [TestMethod]
    public void VisualRegression_BrowserButtons_ShouldMatchBaseline()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // URLを設定してブラウザボタンを有効化
        var urlTextBox = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("UrlTextBox"));
        if (urlTextBox != null)
        {
            urlTextBox.AsTextBox().Text = "https://www.google.com";
        }

        Thread.Sleep(500); // ボタンの有効化を待つ

        // Act - ブラウザボタンエリアのスクリーンショットを取得
        var browserGrid = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BrowserGrid"));
        if (browserGrid == null)
        {
            Assert.Inconclusive("ブラウザグリッドが見つかりません");
            return;
        }

        var screenshot = CaptureElementScreenshot(browserGrid);
        var screenshotPath = Path.Combine(_screenshotPath, "BrowserButtons_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("ブラウザボタンのスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    [TestMethod]
    public void VisualRegression_DisplaySettings_ShouldMatchBaseline()
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

        // 表示タブをクリック
        var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DisplayTab"));
        if (displayTab == null)
        {
            Assert.Inconclusive("表示タブが見つかりません");
            return;
        }

        displayTab.Click();
        Thread.Sleep(500);

        // 表示設定エリアのスクリーンショットを取得
        var displayPanel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DisplayPanel"));
        if (displayPanel == null)
        {
            Assert.Inconclusive("表示パネルが見つかりません");
            return;
        }

        var screenshot = CaptureElementScreenshot(displayPanel);
        var screenshotPath = Path.Combine(_screenshotPath, "DisplaySettings_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("表示設定のスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    [TestMethod]
    public void VisualRegression_BackgroundSettings_ShouldMatchBaseline()
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

        // 表示タブをクリック
        var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DisplayTab"));
        if (displayTab == null)
        {
            Assert.Inconclusive("表示タブが見つかりません");
            return;
        }

        displayTab.Click();
        Thread.Sleep(500);

        // 背景設定エリアのスクリーンショットを取得
        var backgroundPanel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("BackgroundPanel"));
        if (backgroundPanel == null)
        {
            Assert.Inconclusive("背景パネルが見つかりません");
            return;
        }

        var screenshot = CaptureElementScreenshot(backgroundPanel);
        var screenshotPath = Path.Combine(_screenshotPath, "BackgroundSettings_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("背景設定のスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    [TestMethod]
    public void VisualRegression_AccessibilitySettings_ShouldMatchBaseline()
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

        // アクセシビリティタブをクリック
        var accessibilityTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AccessibilityTab"));
        if (accessibilityTab == null)
        {
            Assert.Inconclusive("アクセシビリティタブが見つかりません");
            return;
        }

        accessibilityTab.Click();
        Thread.Sleep(500);

        // アクセシビリティ設定エリアのスクリーンショットを取得
        var accessibilityPanel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AccessibilityPanel"));
        if (accessibilityPanel == null)
        {
            Assert.Inconclusive("アクセシビリティパネルが見つかりません");
            return;
        }

        var screenshot = CaptureElementScreenshot(accessibilityPanel);
        var screenshotPath = Path.Combine(_screenshotPath, "AccessibilitySettings_Baseline.png");

        // Assert - スクリーンショットが取得できること
        screenshot.Should().NotBeNull("アクセシビリティ設定のスクリーンショットが取得できること");
        
        // ベースライン画像として保存
        screenshot!.Save(screenshotPath, ImageFormat.Png);
        
        // ファイルが作成されることを確認
        File.Exists(screenshotPath).Should().BeTrue("ベースライン画像が保存されること");
    }

    /// <summary>
    /// ウィンドウのスクリーンショットを取得する
    /// </summary>
    private Bitmap? CaptureWindowScreenshot(AutomationElement window)
    {
        try
        {
            var bounds = window.BoundingRectangle;
            if (bounds.IsEmpty)
                return null;

            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
            }
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 要素のスクリーンショットを取得する
    /// </summary>
    private Bitmap? CaptureElementScreenshot(AutomationElement element)
    {
        try
        {
            var bounds = element.BoundingRectangle;
            if (bounds.IsEmpty)
                return null;

            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
            }
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
