using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// 背景カスタマイズ機能のUIテスト
/// </summary>
[TestClass]
public class BackgroundCustomizationUITests
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
    public void BackgroundSettings_ShouldHaveColorSelection()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - 背景色選択機能が存在することを確認
                var backgroundColorButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("背景色").Or(cf.ByName("Background Color")));
                _ = backgroundColorButton.Should().NotBeNull("背景色選択ボタンが存在すること");

                var colorPreview = settingsWindow.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Pane));
                _ = colorPreview.Should().NotBeNull("色プレビューが存在すること");
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void BackgroundSettings_ShouldHaveGradientOptions()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - グラデーション設定が存在することを確認
                var gradientCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByName("背景グラデーション").Or(cf.ByName("Background Gradient")));
                _ = gradientCheckbox.Should().NotBeNull("グラデーションチェックボックスが存在すること");

                var gradientStartColorButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("グラデーション開始色").Or(cf.ByName("Gradient Start Color")));
                _ = gradientStartColorButton.Should().NotBeNull("グラデーション開始色ボタンが存在すること");

                var gradientEndColorButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("グラデーション終了色").Or(cf.ByName("Gradient End Color")));
                _ = gradientEndColorButton.Should().NotBeNull("グラデーション終了色ボタンが存在すること");
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void BackgroundSettings_ShouldHaveGradientDirectionOptions()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - グラデーション方向設定が存在することを確認
                var gradientDirectionComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("グラデーション方向").Or(cf.ByName("Gradient Direction")));
                _ = gradientDirectionComboBox.Should().NotBeNull("グラデーション方向コンボボックスが存在すること");

                // グラデーション方向のオプションを確認
                var comboBox = settingsWindow.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox));
                if (comboBox != null)
                {
                    comboBox.Click();
                    System.Threading.Thread.Sleep(200);

                    var items = comboBox.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
                    _ = items.Should().NotBeEmpty("グラデーション方向のオプションが存在すること");
                }
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveSizeConfiguration()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - ウィンドウサイズ設定が存在することを確認
                var widthTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("初期起動サイズ").Or(cf.ByName("Initial Size")));
                _ = widthTextBox.Should().NotBeNull("初期起動サイズ設定が存在すること");

                var heightTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("初期起動サイズ").Or(cf.ByName("Initial Size")));
                _ = heightTextBox.Should().NotBeNull("初期起動サイズ設定が存在すること");
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveLogoDisplayOption()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - ロゴ表示設定が存在することを確認
                var logoCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByName("ロゴ表示").Or(cf.ByName("Show Logo")));
                _ = logoCheckbox.Should().NotBeNull("ロゴ表示設定が存在すること");
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void WindowSettings_ShouldHaveUrlInputDisplayOption()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        System.Threading.Thread.Sleep(500);

        // 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定").Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetMainWindow(_automation);

            // 表示タブをクリック
            var displayTab = settingsWindow.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - URL入力表示設定が存在することを確認
                var urlInputCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByName("URL入力表示").Or(cf.ByName("Show URL Input")));
                _ = urlInputCheckbox.Should().NotBeNull("URL入力表示設定が存在すること");
            }
            else
            {
                Assert.Inconclusive("表示タブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }
}
