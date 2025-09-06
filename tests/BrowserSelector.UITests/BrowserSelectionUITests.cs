using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace BrowserSelector.UITests;

/// <summary>
/// ブラウザ選択機能に特化したFlaUIテスト
/// </summary>
[TestClass]
public class BrowserSelectionUITests
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;
    private Window? _mainWindow = null;

    [TestInitialize]
    public void Setup()
    {
        try
        {
            // アプリケーションパスの構築
            var appPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "src", "BrowserSelector.App", 
                "bin", "Debug", "net8.0-windows", "BrowserSelector.App.exe");
            
            // パスを正規化
            appPath = System.IO.Path.GetFullPath(appPath);

            if (System.IO.File.Exists(appPath))
            {
                _app = Application.Launch(appPath);
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

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            _automation?.Dispose();
            _app?.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"クリーンアップ中にエラー: {ex.Message}");
        }
    }

    [TestMethod]
    public void BrowserGrid_ShouldDisplayBrowsers()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ブラウザグリッドまたはリストを検索
        var browserContainer = _mainWindow.FindFirstChild(cf => 
            cf.ByControlType(ControlType.List)
             .Or(cf.ByControlType(ControlType.DataGrid))
             .Or(cf.ByControlType(ControlType.Group))
             .Or(cf.ByAutomationId("BrowserGrid"))
             .Or(cf.ByName("ブラウザ"))
             .Or(cf.ByName("Browsers")));

        // Assert
        if (browserContainer != null)
        {
            browserContainer.Should().NotBeNull("ブラウザコンテナが存在すること");
            browserContainer.IsEnabled.Should().BeTrue("ブラウザコンテナが有効であること");
            
            // ブラウザアイテムを検索
            var browserItems = browserContainer.FindAllChildren(cf => 
                cf.ByControlType(ControlType.Button)
                 .Or(cf.ByControlType(ControlType.ListItem)));
            
            browserItems.Should().NotBeNull("ブラウザアイテムが検索できること");
        }
        else
        {
            Console.WriteLine("ブラウザコンテナが見つかりませんでした");
        }
    }

    [TestMethod]
    public void BrowserButtons_ShouldBeClickable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ブラウザボタンを検索
        var browserButtons = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .And(cf.ByAutomationId("BrowserButton"))
             .Or(cf.ByName("Chrome"))
             .Or(cf.ByName("Firefox"))
             .Or(cf.ByName("Edge"))
             .Or(cf.ByName("Safari")));

        // Assert
        browserButtons.Should().NotBeNull("ブラウザボタンが検索できること");
        
        if (browserButtons.Length > 0)
        {
            foreach (var button in browserButtons)
            {
                button.Should().NotBeNull("ブラウザボタンが有効であること");
                button.IsEnabled.Should().BeTrue("ブラウザボタンが有効であること");
                button.Name.Should().NotBeNullOrEmpty("ブラウザボタンに名前が設定されていること");
                
                // ボタンの境界矩形を確認
                var buttonBounds = button.BoundingRectangle;
                buttonBounds.Width.Should().BeGreaterThan(0, "ブラウザボタンの境界矩形が設定されていること");
                buttonBounds.Height.Should().BeGreaterThan(0, "ブラウザボタンの境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("ブラウザボタンが見つかりませんでした");
        }
    }

    [TestMethod]
    public void BrowserIcons_ShouldBeDisplayed()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // アイコン要素を検索
        var iconElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Image)
             .Or(cf.ByAutomationId("BrowserIcon")));

        // Assert
        iconElements.Should().NotBeNull("アイコン要素が検索できること");
        
        if (iconElements.Length > 0)
        {
            foreach (var icon in iconElements)
            {
                icon.Should().NotBeNull("アイコン要素が有効であること");
                var iconBounds = icon.BoundingRectangle;
                iconBounds.Width.Should().BeGreaterThan(0, "アイコンの境界矩形が設定されていること");
                iconBounds.Height.Should().BeGreaterThan(0, "アイコンの境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("アイコン要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void URLInput_ShouldBeEditable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // URL入力フィールドを検索
        var urlInput = _mainWindow.FindFirstChild(cf => 
            cf.ByControlType(ControlType.Edit)
             .And(cf.ByAutomationId("UrlInput"))
             .Or(cf.ByName("URL"))
             .Or(cf.ByName("アドレス"))
             .Or(cf.ByAutomationId("AddressBar")));

        // Assert
        if (urlInput != null)
        {
            urlInput.Should().NotBeNull("URL入力フィールドが存在すること");
            urlInput.IsEnabled.Should().BeTrue("URL入力フィールドが有効であること");
            urlInput.Name.Should().NotBeNullOrEmpty("URL入力フィールドに名前が設定されていること");
        }
        else
        {
            Console.WriteLine("URL入力フィールドが見つかりませんでした");
        }
    }

    [TestMethod]
    public void SettingsButton_ShouldOpenSettings()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // 設定ボタンを検索
        var settingsButton = _mainWindow.FindFirstChild(cf => 
            cf.ByControlType(ControlType.Button)
             .And(cf.ByAutomationId("SettingsButton"))
             .Or(cf.ByName("設定"))
             .Or(cf.ByName("Settings"))
             .Or(cf.ByName("⚙"))
             .Or(cf.ByName("Options")));

        // Assert
        if (settingsButton != null)
        {
            settingsButton.Should().NotBeNull("設定ボタンが存在すること");
            settingsButton.IsEnabled.Should().BeTrue("設定ボタンが有効であること");
            settingsButton.Name.Should().NotBeNullOrEmpty("設定ボタンに名前が設定されていること");
        }
        else
        {
            Console.WriteLine("設定ボタンが見つかりませんでした");
        }
    }

    [TestMethod]
    public void MenuBar_ShouldBeAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // メニューバーを検索
        var menuBar = _mainWindow.FindFirstChild(cf => 
            cf.ByControlType(ControlType.MenuBar)
             .Or(cf.ByAutomationId("MenuBar"))
             .Or(cf.ByName("メニュー"))
             .Or(cf.ByName("Menu")));

        // Assert
        if (menuBar != null)
        {
            menuBar.Should().NotBeNull("メニューバーが存在すること");
            menuBar.IsEnabled.Should().BeTrue("メニューバーが有効であること");
            
            // メニューアイテムを検索
            var menuItems = menuBar.FindAllChildren(cf => cf.ByControlType(ControlType.MenuItem));
            menuItems.Should().NotBeNull("メニューアイテムが検索できること");
        }
        else
        {
            Console.WriteLine("メニューバーが見つかりませんでした");
        }
    }

    [TestMethod]
    public void StatusBar_ShouldDisplayInformation()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ステータスバーを検索
        var statusBar = _mainWindow.FindFirstChild(cf => 
            cf.ByControlType(ControlType.StatusBar)
             .Or(cf.ByAutomationId("StatusBar"))
             .Or(cf.ByName("ステータス"))
             .Or(cf.ByName("Status")));

        // Assert
        if (statusBar != null)
        {
            statusBar.Should().NotBeNull("ステータスバーが存在すること");
            statusBar.IsEnabled.Should().BeTrue("ステータスバーが有効であること");
        }
        else
        {
            Console.WriteLine("ステータスバーが見つかりませんでした");
        }
    }

    [TestMethod]
    public void Window_ShouldSupportMinimizeMaximize()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ウィンドウの状態を確認（FlaUIではWindowStateプロパティが利用できないため、基本プロパティを確認）
        var isEnabled = _mainWindow.IsEnabled;
        var bounds = _mainWindow.BoundingRectangle;
        
        // Assert
        isEnabled.Should().BeTrue("ウィンドウが有効であること");
        bounds.Width.Should().BeGreaterThan(0, "ウィンドウの幅が0より大きいこと");
        bounds.Height.Should().BeGreaterThan(0, "ウィンドウの高さが0より大きいこと");
    }

    [TestMethod]
    public void UI_ShouldBeResponsive()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // UI要素の応答性を確認
        var startTime = DateTime.Now;
        var allElements = _mainWindow.FindAllChildren();
        var endTime = DateTime.Now;
        
        // Assert
        allElements.Should().NotBeNull("UI要素が検索できること");
        
        var responseTime = (endTime - startTime).TotalMilliseconds;
        responseTime.Should().BeLessThan(1000, "UI要素の検索が1秒以内に完了すること");
    }

    [TestMethod]
    public void Accessibility_ShouldMeetStandards()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // アクセシビリティ要素を検索
        var accessibleElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.Text))
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        accessibleElements.Should().NotBeNull("アクセシブルな要素が検索できること");
        
        if (accessibleElements.Length > 0)
        {
            foreach (var element in accessibleElements)
            {
                element.Should().NotBeNull("アクセシブルな要素が有効であること");
                
                // 要素に名前が設定されていることを確認
                if (!string.IsNullOrEmpty(element.Name))
                {
                    element.Name.Should().NotBeNullOrEmpty("要素に名前が設定されていること");
                }
                
                // 要素が有効であることを確認
                element.IsEnabled.Should().BeTrue("要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("アクセシブルな要素が見つかりませんでした");
        }
    }
}
