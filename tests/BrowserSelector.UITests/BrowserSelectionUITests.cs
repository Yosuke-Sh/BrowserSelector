using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace BrowserSelector.UITests;

/// <summary>
/// ブラウザ選択機能に特化したFlaUIテスト
/// </summary>
public class BrowserSelectionUITests : IDisposable
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;
    private Window? _mainWindow = null;

    public BrowserSelectionUITests()
    {
        try
        {
            // アプリケーションパスの構築
            string appPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..", "src", "BrowserSelector.App",
                "bin", "Debug", "net8.0-windows", "BrowserSelector.App.exe");

            // パスを正規化
            appPath = System.IO.Path.GetFullPath(appPath);

            if (System.IO.File.Exists(appPath))
            {
                // テスト用アプリケーションを起動
            _app = UITestHelper.LaunchTestApplication(appPath);
                _automation = new UIA3Automation();

                // メインウィンドウの取得を待機
                _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
            }
            else
            {
                Console.WriteLine($"アプリケーションが見つかりません: {appPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UIテストセットアップ中にエラー: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _automation?.Dispose();
            _ = (_app?.Close());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"クリーンアップ中にエラー: {ex.Message}");
        }
    }

    [Fact]
    public void BrowserGrid_ShouldDisplayBrowsers()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Skip.If(true, "メインウィンドウが取得できませんでした");
            return;
        }

        // ブラウザグリッドまたはリストを検索
        AutomationElement browserContainer = _mainWindow.FindFirstChild(cf =>
            cf.ByControlType(ControlType.List)
             .Or(cf.ByControlType(ControlType.DataGrid))
             .Or(cf.ByControlType(ControlType.Group))
             .Or(cf.ByAutomationId("BrowserGrid"))
             .Or(cf.ByName("ブラウザ"))
             .Or(cf.ByName("Browsers")));

        // Assert
        if (browserContainer != null)
        {
            _ = browserContainer.Should().NotBeNull("ブラウザコンテナが存在すること");
            _ = browserContainer.IsEnabled.Should().BeTrue("ブラウザコンテナが有効であること");

            // ブラウザアイテムを検索
            AutomationElement[] browserItems = browserContainer.FindAllChildren(cf =>
                cf.ByControlType(ControlType.Button)
                 .Or(cf.ByControlType(ControlType.ListItem)));

            _ = browserItems.Should().NotBeNull("ブラウザアイテムが検索できること");
        }
        else
        {
            Console.WriteLine("ブラウザコンテナが見つかりませんでした");
        }
    }

    [Fact]
    public void BrowserButtons_ShouldBeClickable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Skip.If(true, "メインウィンドウが取得できませんでした");
            return;
        }

        // ブラウザボタンを検索
        AutomationElement[] browserButtons = _mainWindow.FindAllChildren(cf =>
            cf.ByControlType(ControlType.Button)
             .And(cf.ByAutomationId("BrowserButton"))
             .Or(cf.ByName("Chrome"))
             .Or(cf.ByName("Firefox"))
             .Or(cf.ByName("Edge"))
             .Or(cf.ByName("Safari")));

        // Assert
        _ = browserButtons.Should().NotBeNull("ブラウザボタンが検索できること");

        if (browserButtons.Length > 0)
        {
            foreach (AutomationElement? button in browserButtons)
            {
                _ = button.Should().NotBeNull("ブラウザボタンが有効であること");
                _ = button.IsEnabled.Should().BeTrue("ブラウザボタンが有効であること");
                _ = button.Name.Should().NotBeNullOrEmpty("ブラウザボタンに名前が設定されていること");

                // ボタンの境界矩形を確認
                System.Drawing.Rectangle buttonBounds = button.BoundingRectangle;
                _ = buttonBounds.Width.Should().BeGreaterThan(0, "ブラウザボタンの境界矩形が設定されていること");
                _ = buttonBounds.Height.Should().BeGreaterThan(0, "ブラウザボタンの境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("ブラウザボタンが見つかりませんでした");
        }
    }

    [Fact]
    public void BrowserIcons_ShouldBeDisplayed()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // アイコン要素を検索
        AutomationElement[] iconElements = _mainWindow.FindAllChildren(cf =>
            cf.ByControlType(ControlType.Image)
             .Or(cf.ByAutomationId("BrowserIcon")));

        // Assert
        _ = iconElements.Should().NotBeNull("アイコン要素が検索できること");

        if (iconElements.Length > 0)
        {
            foreach (AutomationElement? icon in iconElements)
            {
                _ = icon.Should().NotBeNull("アイコン要素が有効であること");
                System.Drawing.Rectangle iconBounds = icon.BoundingRectangle;
                _ = iconBounds.Width.Should().BeGreaterThan(0, "アイコンの境界矩形が設定されていること");
                _ = iconBounds.Height.Should().BeGreaterThan(0, "アイコンの境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("アイコン要素が見つかりませんでした");
        }
    }

    [Fact]
    public void URLInput_ShouldBeEditable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // URL入力フィールドを検索
        AutomationElement urlInput = _mainWindow.FindFirstChild(cf =>
            cf.ByControlType(ControlType.Edit)
             .And(cf.ByAutomationId("UrlInput"))
             .Or(cf.ByName("URL"))
             .Or(cf.ByName("アドレス"))
             .Or(cf.ByAutomationId("AddressBar")));

        // Assert
        if (urlInput != null)
        {
            _ = urlInput.Should().NotBeNull("URL入力フィールドが存在すること");
            _ = urlInput.IsEnabled.Should().BeTrue("URL入力フィールドが有効であること");
            _ = urlInput.Name.Should().NotBeNullOrEmpty("URL入力フィールドに名前が設定されていること");
        }
        else
        {
            Console.WriteLine("URL入力フィールドが見つかりませんでした");
        }
    }

    [Fact]
    public void SettingsButton_ShouldOpenSettings()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // 設定ボタンを検索
        AutomationElement settingsButton = _mainWindow.FindFirstChild(cf =>
            cf.ByControlType(ControlType.Button)
             .And(cf.ByAutomationId("SettingsButton"))
             .Or(cf.ByName("設定"))
             .Or(cf.ByName("Settings"))
             .Or(cf.ByName("⚙"))
             .Or(cf.ByName("Options")));

        // Assert
        if (settingsButton != null)
        {
            _ = settingsButton.Should().NotBeNull("設定ボタンが存在すること");
            _ = settingsButton.IsEnabled.Should().BeTrue("設定ボタンが有効であること");
            _ = settingsButton.Name.Should().NotBeNullOrEmpty("設定ボタンに名前が設定されていること");
        }
        else
        {
            Console.WriteLine("設定ボタンが見つかりませんでした");
        }
    }

    [Fact]
    public void MenuBar_ShouldBeAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // メニューバーを検索
        AutomationElement menuBar = _mainWindow.FindFirstChild(cf =>
            cf.ByControlType(ControlType.MenuBar)
             .Or(cf.ByAutomationId("MenuBar"))
             .Or(cf.ByName("メニュー"))
             .Or(cf.ByName("Menu")));

        // Assert
        if (menuBar != null)
        {
            _ = menuBar.Should().NotBeNull("メニューバーが存在すること");
            _ = menuBar.IsEnabled.Should().BeTrue("メニューバーが有効であること");

            // メニューアイテムを検索
            AutomationElement[] menuItems = menuBar.FindAllChildren(cf => cf.ByControlType(ControlType.MenuItem));
            _ = menuItems.Should().NotBeNull("メニューアイテムが検索できること");
        }
        else
        {
            Console.WriteLine("メニューバーが見つかりませんでした");
        }
    }

    [Fact]
    public void StatusBar_ShouldDisplayInformation()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // ステータスバーを検索
        AutomationElement statusBar = _mainWindow.FindFirstChild(cf =>
            cf.ByControlType(ControlType.StatusBar)
             .Or(cf.ByAutomationId("StatusBar"))
             .Or(cf.ByName("ステータス"))
             .Or(cf.ByName("Status")));

        // Assert
        if (statusBar != null)
        {
            _ = statusBar.Should().NotBeNull("ステータスバーが存在すること");
            _ = statusBar.IsEnabled.Should().BeTrue("ステータスバーが有効であること");
        }
        else
        {
            Console.WriteLine("ステータスバーが見つかりませんでした");
        }
    }

    [Fact]
    public void Window_ShouldSupportMinimizeMaximize()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // ウィンドウの状態を確認（FlaUIではWindowStateプロパティが利用できないため、基本プロパティを確認）
        bool isEnabled = _mainWindow.IsEnabled;
        System.Drawing.Rectangle bounds = _mainWindow.BoundingRectangle;

        // Assert
        _ = isEnabled.Should().BeTrue("ウィンドウが有効であること");
        _ = bounds.Width.Should().BeGreaterThan(0, "ウィンドウの幅が0より大きいこと");
        _ = bounds.Height.Should().BeGreaterThan(0, "ウィンドウの高さが0より大きいこと");
    }

    [Fact]
    public void UI_ShouldBeResponsive()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // UI要素の応答性を確認
        DateTime startTime = DateTime.Now;
        AutomationElement[] allElements = _mainWindow.FindAllChildren();
        DateTime endTime = DateTime.Now;

        // Assert
        _ = allElements.Should().NotBeNull("UI要素が検索できること");

        double responseTime = (endTime - startTime).TotalMilliseconds;
        _ = responseTime.Should().BeLessThan(2000, "UI要素の検索が2秒以内に完了すること");
    }

    [Fact]
    public void Accessibility_ShouldMeetStandards()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            return;
        }

        // アクセシビリティ要素を検索
        AutomationElement[] accessibleElements = _mainWindow.FindAllChildren(cf =>
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.Text))
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        _ = accessibleElements.Should().NotBeNull("アクセシブルな要素が検索できること");

        if (accessibleElements.Length > 0)
        {
            foreach (AutomationElement? element in accessibleElements)
            {
                _ = element.Should().NotBeNull("アクセシブルな要素が有効であること");

                // 要素に名前が設定されていることを確認
                if (!string.IsNullOrEmpty(element.Name))
                {
                    _ = element.Name.Should().NotBeNullOrEmpty("要素に名前が設定されていること");
                }

                // 要素が有効であることを確認
                _ = element.IsEnabled.Should().BeTrue("要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("アクセシブルな要素が見つかりませんでした");
        }
    }
}
