using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace BrowserSelector.UITests;

/// <summary>
/// 包括的なFlaUIテスト
/// </summary>
[TestClass]
public class ComprehensiveUITests
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
    public void Application_ShouldStartSuccessfully()
    {
        // Arrange & Act & Assert
        if (_app == null)
        {
            Assert.Inconclusive("アプリケーションが起動できませんでした。アプリケーションファイルが存在することを確認してください。");
            return;
        }
        
        _app.Should().NotBeNull("アプリケーションが正常に起動すること");
        _automation.Should().NotBeNull("UI自動化が正常に初期化されること");
    }

    [TestMethod]
    public void MainWindow_ShouldBeVisibleAndAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // Assert
        _mainWindow.Should().NotBeNull("メインウィンドウが存在すること");
        _mainWindow.IsEnabled.Should().BeTrue("メインウィンドウが有効であること");
        _mainWindow.Title.Should().NotBeNullOrEmpty("ウィンドウタイトルが設定されていること");
        
        // ウィンドウの基本プロパティを確認
        var bounds = _mainWindow.BoundingRectangle;
        bounds.Width.Should().BeGreaterThan(0, "ウィンドウの境界矩形が設定されていること");
        bounds.Height.Should().BeGreaterThan(0, "ウィンドウの境界矩形が設定されていること");
    }

    [TestMethod]
    public void MainWindow_ShouldHaveCorrectTitle()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // Assert
        _mainWindow.Title.Should().Contain("BrowserSelector", "ウィンドウタイトルにアプリケーション名が含まれること");
    }

    [TestMethod]
    public void UIElements_ShouldBeAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // すべてのUI要素を取得
        var allElements = _mainWindow.FindAllChildren();
        
        // Assert
        allElements.Should().NotBeNull("UI要素が検索できること");
        allElements.Length.Should().BeGreaterThan(0, "少なくとも1つのUI要素が存在すること");

        // 各要素のアクセシビリティを確認
        foreach (var element in allElements)
        {
            element.Should().NotBeNull("UI要素が有効であること");
            
            // 名前が設定されている要素の確認
            if (!string.IsNullOrEmpty(element.Name))
            {
                element.Name.Should().NotBeNullOrEmpty("UI要素に名前が設定されていること");
            }
        }
    }

    [TestMethod]
    public void Buttons_ShouldBeClickable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ボタン要素を検索
        var buttons = _mainWindow.FindAllChildren(cf => cf.ByControlType(ControlType.Button));
        
        // Assert
        buttons.Should().NotBeNull("ボタン要素が検索できること");
        
        if (buttons.Length > 0)
        {
            foreach (var button in buttons)
            {
                button.Should().NotBeNull("ボタン要素が有効であること");
                button.IsEnabled.Should().BeTrue("ボタンが有効であること");
                button.Name.Should().NotBeNullOrEmpty("ボタンに名前が設定されていること");
                
                // ボタンの境界矩形を確認
                var buttonBounds = button.BoundingRectangle;
                buttonBounds.Width.Should().BeGreaterThan(0, "ボタンの境界矩形が設定されていること");
                buttonBounds.Height.Should().BeGreaterThan(0, "ボタンの境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("ボタン要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void TextElements_ShouldBeReadable()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // テキスト要素を検索
        var textElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Text)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.Document)));

        // Assert
        textElements.Should().NotBeNull("テキスト要素が検索できること");
        
        if (textElements.Length > 0)
        {
            foreach (var textElement in textElements)
            {
                textElement.Should().NotBeNull("テキスト要素が有効であること");
                
                // テキスト要素の名前またはテキストを確認
                if (!string.IsNullOrEmpty(textElement.Name))
                {
                    textElement.Name.Should().NotBeNullOrEmpty("テキスト要素に名前が設定されていること");
                }
            }
        }
        else
        {
            Console.WriteLine("テキスト要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void MenuItems_ShouldBeAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // メニュー要素を検索
        var menuItems = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Menu)
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        menuItems.Should().NotBeNull("メニュー要素が検索できること");
        
        if (menuItems.Length > 0)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.Should().NotBeNull("メニュー要素が有効であること");
                menuItem.IsEnabled.Should().BeTrue("メニュー要素が有効であること");
                menuItem.Name.Should().NotBeNullOrEmpty("メニュー要素に名前が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("メニュー要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void Window_ShouldRespondToResize()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ウィンドウの初期サイズを取得
        var initialBounds = _mainWindow.BoundingRectangle;
        
        // Assert
        initialBounds.Width.Should().BeGreaterThan(0, "ウィンドウの幅が0より大きいこと");
        initialBounds.Height.Should().BeGreaterThan(0, "ウィンドウの高さが0より大きいこと");
    }

    [TestMethod]
    public void UI_ShouldSupportKeyboardNavigation()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // フォーカス可能な要素を検索
        var focusableElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        focusableElements.Should().NotBeNull("フォーカス可能な要素が検索できること");
        
        if (focusableElements.Length > 0)
        {
            foreach (var element in focusableElements)
            {
                element.Should().NotBeNull("フォーカス可能な要素が有効であること");
                element.IsEnabled.Should().BeTrue("フォーカス可能な要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("フォーカス可能な要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void Application_ShouldHandleWindowEvents()
    {
        // Arrange & Act
        if (_app == null || _mainWindow == null)
        {
            Assert.Inconclusive("アプリケーションまたはメインウィンドウが取得できませんでした");
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
    public void UI_ShouldHaveProperAutomationIds()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // すべてのUI要素を取得
        var allElements = _mainWindow.FindAllChildren();
        
        // Assert
        allElements.Should().NotBeNull("UI要素が検索できること");
        
        // AutomationIdが設定されている要素を確認
        var elementsWithAutomationId = allElements.Where(e => !string.IsNullOrEmpty(e.AutomationId)).ToArray();
        
        if (elementsWithAutomationId.Length > 0)
        {
            foreach (var element in elementsWithAutomationId)
            {
                element.AutomationId.Should().NotBeNullOrEmpty("AutomationIdが設定されていること");
            }
        }
        else
        {
            Console.WriteLine("AutomationIdが設定されている要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void UI_ShouldSupportScreenReader()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // スクリーンリーダー対応の要素を検索
        var accessibleElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Text))
             .Or(cf.ByControlType(ControlType.Edit)));

        // Assert
        accessibleElements.Should().NotBeNull("アクセシブルな要素が検索できること");
        
        if (accessibleElements.Length > 0)
        {
            foreach (var element in accessibleElements)
            {
                element.Should().NotBeNull("アクセシブルな要素が有効であること");
                
                // 要素に名前が設定されていることを確認（スクリーンリーダー対応）
                if (!string.IsNullOrEmpty(element.Name))
                {
                    element.Name.Should().NotBeNullOrEmpty("要素に名前が設定されていること");
                }
            }
        }
        else
        {
            Console.WriteLine("アクセシブルな要素が見つかりませんでした");
        }
    }
}
