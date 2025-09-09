using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace BrowserSelector.UITests;

/// <summary>
/// ブラウザボタンのカスタマイズ機能のUIテスト
/// </summary>
[Collection("UI Tests")]
public class BrowserButtonCustomizationUITests : IDisposable
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;

    public BrowserButtonCustomizationUITests()
    {
        try
        {
            string appPath = UITestHelper.GetApplicationPath();
            if (string.IsNullOrEmpty(appPath))
            {
            }

            // テスト用アプリケーションを起動
            _app = UITestHelper.LaunchTestApplication(appPath);
            _automation = new UIA3Automation();
        }
        catch (Exception ex)
        {
        }
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _app?.Close();
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableAppearance()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - URLを設定してブラウザボタンを有効化
        bool urlSet = UITestHelper.SetUrlInMainWindow(mainWindow, "https://www.google.com");
        if (urlSet)
        {
            // ブラウザボタンが有効になるまで待機
            bool buttonsEnabled = UITestHelper.WaitForBrowserButtonsEnabled(mainWindow, 3000);
            if (!buttonsEnabled)
            {
                return;
            }
        }

        // ブラウザボタンを検索
        var browserButtons = mainWindow.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
        var actualBrowserButtons = browserButtons.Where(btn => btn.Name != "設定" && btn.Name != "Settings").ToList();

        // Assert - ブラウザボタンが存在し、カスタマイズ可能であることを確認
        _ = actualBrowserButtons.Should().NotBeEmpty("ブラウザボタンが存在すること");

        foreach (var button in actualBrowserButtons)
        {
            _ = button.Should().NotBeNull("ボタン要素が有効であること");

            // ボタンが無効の場合はスキップ（ブラウザが検出されていない可能性）
            if (!button.IsEnabled)
            {
                Console.WriteLine($"ボタン '{button.Name}' が無効です。ブラウザが検出されていない可能性があります。");
                continue;
            }

            _ = button.Name.Should().NotBeNullOrEmpty("ボタンに名前が設定されていること");

            // ボタンの境界矩形を確認
            System.Drawing.Rectangle buttonBounds = button.BoundingRectangle;
            _ = buttonBounds.Width.Should().BeGreaterThan(0, "ボタンの境界矩形が設定されていること");
            _ = buttonBounds.Height.Should().BeGreaterThan(0, "ボタンの境界矩形が設定されていること");
        }
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableOpacity()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - ボタン透明度設定が存在することを確認
                var opacitySlider = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン背景透明化").Or(cf.ByName("Button Transparency")));
                _ = opacitySlider.Should().NotBeNull("ボタン透明度スライダーが存在すること");

                var opacityLabel = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン背景透明化").Or(cf.ByName("Button Transparency")));
                _ = opacityLabel.Should().NotBeNull("ボタン透明度ラベルが存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableSize()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - ボタンサイズ設定が存在することを確認
                var widthTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン幅").Or(cf.ByName("Button Width")));
                _ = widthTextBox.Should().NotBeNull("ボタン幅設定が存在すること");

                var heightTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン高さ").Or(cf.ByName("Button Height")));
                _ = heightTextBox.Should().NotBeNull("ボタン高さ設定が存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableColors()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - ボタン色設定が存在することを確認
                var backgroundColorButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン背景色").Or(cf.ByName("Button Background Color")));
                _ = backgroundColorButton.Should().NotBeNull("ボタン背景色設定が存在すること");

                var foregroundColorButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン前景色").Or(cf.ByName("Button Foreground Color")));
                _ = foregroundColorButton.Should().NotBeNull("ボタン前景色設定が存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableCornerRadius()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - ボタン角の丸み設定が存在することを確認
                var cornerRadiusTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("ボタン角の丸み").Or(cf.ByName("Button Corner Radius")));
                _ = cornerRadiusTextBox.Should().NotBeNull("ボタン角の丸み設定が存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }

    [Fact]
    public void BrowserButtonsShouldHaveCustomizableIconSize()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - アイコンサイズ設定が存在することを確認
                var iconSizeTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByName("アイコンサイズ").Or(cf.ByName("Icon Size")));
                _ = iconSizeTextBox.Should().NotBeNull("アイコンサイズ設定が存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }

    [Fact]
    public void BrowserButtonsShouldShowHideBrowserNames()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
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

                // Assert - ブラウザ名表示設定が存在することを確認
                var showBrowserNameCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByName("ブラウザ名表示").Or(cf.ByName("Show Browser Name")));
                _ = showBrowserNameCheckbox.Should().NotBeNull("ブラウザ名表示設定が存在すること");
            }
            else
            {
            }
        }
        else
        {
        }
    }
}
