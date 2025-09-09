using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace BrowserSelector.UITests;

/// <summary>
/// 背景カスタマイズ機能のUIテスト.
/// </summary>
public class BackgroundCustomizationUITests : IDisposable
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;

    /// <summary>
    /// BackgroundCustomizationUITestsのインスタンスを初期化.
    /// </summary>
    public BackgroundCustomizationUITests()
    {
        try
        {
            string appPath = UITestHelper.GetApplicationPath();
            if (string.IsNullOrEmpty(appPath))
            {
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }

            // テスト用アプリケーションを起動
            _app = UITestHelper.LaunchTestApplication(appPath);
            _automation = new UIA3Automation();
        }
        catch (Exception)
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// リソースを解放.
    /// </summary>
    public void Dispose()
    {
        _automation?.Dispose();
        _app?.Close();
    }

    /// <summary>
    /// 背景設定に色選択機能があることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void BackgroundSettingsShouldHaveColorSelection()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// 背景設定にグラデーションオプションがあることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void BackgroundSettingsShouldHaveGradientOptions()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// 背景設定にグラデーション方向オプションがあることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void BackgroundSettingsShouldHaveGradientDirectionOptions()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// ウィンドウ設定にサイズ設定があることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void WindowSettingsShouldHaveSizeConfiguration()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// ウィンドウ設定にロゴ表示オプションがあることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void WindowSettingsShouldHaveLogoDisplayOption()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }

    /// <summary>
    /// ウィンドウ設定にURL入力表示オプションがあることを確認するUIテスト.
    /// </summary>
    [Fact]
    public void WindowSettingsShouldHaveUrlInputDisplayOption()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
                Xunit.Assert.Fail("アプリケーションが見つかりません");
            }
        }
        else
        {
            Xunit.Assert.Fail("UIテスト用アプリケーション起動に失敗");
        }
    }
}
