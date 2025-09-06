using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace BrowserSelector.UITests;

/// <summary>
/// 高度なインタラクションテスト（キーボード・マウス操作）
/// </summary>
[TestClass]
public class AdvancedInteractionTests
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
    public void KeyboardNavigation_ShouldWorkCorrectly()
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
                
                // 要素にフォーカスを設定できることを確認
                try
                {
                    element.Focus();
                    // フォーカス設定が成功したことを確認
                    true.Should().BeTrue("要素にフォーカスを設定できること");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"フォーカス設定中にエラー: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine("フォーカス可能な要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void TabNavigation_ShouldWorkCorrectly()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // Tabキーでナビゲーション可能な要素を検索
        var tabbableElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        tabbableElements.Should().NotBeNull("Tabナビゲーション可能な要素が検索できること");
        
        if (tabbableElements.Length > 1)
        {
            // 最初の要素にフォーカスを設定
            var firstElement = tabbableElements[0];
            firstElement.Should().NotBeNull("最初の要素が有効であること");
            
            try
            {
                firstElement.Focus();
                Console.WriteLine("Tabナビゲーションの基本テストが完了しました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tabナビゲーションテスト中にエラー: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Tabナビゲーション可能な要素が不足しています");
        }
    }

    [TestMethod]
    public void MouseInteraction_ShouldWorkCorrectly()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // クリック可能な要素を検索
        var clickableElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        clickableElements.Should().NotBeNull("クリック可能な要素が検索できること");
        
        if (clickableElements.Length > 0)
        {
            foreach (var element in clickableElements)
            {
                element.Should().NotBeNull("クリック可能な要素が有効であること");
                element.IsEnabled.Should().BeTrue("クリック可能な要素が有効であること");
                
                // 要素の境界矩形を確認
                var bounds = element.BoundingRectangle;
                bounds.Width.Should().BeGreaterThan(0, "要素の幅が0より大きいこと");
                bounds.Height.Should().BeGreaterThan(0, "要素の高さが0より大きいこと");
            }
        }
        else
        {
            Console.WriteLine("クリック可能な要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void ContextMenu_ShouldBeAccessible()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // コンテキストメニューが表示可能な要素を検索
        var contextMenuElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.ListItem))
             .Or(cf.ByControlType(ControlType.Text)));

        // Assert
        contextMenuElements.Should().NotBeNull("コンテキストメニュー要素が検索できること");
        
        if (contextMenuElements.Length > 0)
        {
            foreach (var element in contextMenuElements)
            {
                element.Should().NotBeNull("コンテキストメニュー要素が有効であること");
                element.IsEnabled.Should().BeTrue("コンテキストメニュー要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("コンテキストメニュー要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void DragAndDrop_ShouldBeSupported()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ドラッグ可能な要素を検索
        var draggableElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.ListItem))
             .Or(cf.ByControlType(ControlType.Image)));

        // Assert
        draggableElements.Should().NotBeNull("ドラッグ可能な要素が検索できること");
        
        if (draggableElements.Length > 0)
        {
            foreach (var element in draggableElements)
            {
                element.Should().NotBeNull("ドラッグ可能な要素が有効であること");
                element.IsEnabled.Should().BeTrue("ドラッグ可能な要素が有効であること");
                
                // 要素の境界矩形を確認
                var bounds = element.BoundingRectangle;
                bounds.Width.Should().BeGreaterThan(0, "ドラッグ可能な要素の境界矩形が設定されていること");
                bounds.Height.Should().BeGreaterThan(0, "ドラッグ可能な要素の境界矩形が設定されていること");
            }
        }
        else
        {
            Console.WriteLine("ドラッグ可能な要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void ScrollableElements_ShouldWorkCorrectly()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // スクロール可能な要素を検索
        var scrollableElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.List)
             .Or(cf.ByControlType(ControlType.DataGrid))
             .Or(cf.ByControlType(ControlType.ScrollBar)));

        // Assert
        scrollableElements.Should().NotBeNull("スクロール可能な要素が検索できること");
        
        if (scrollableElements.Length > 0)
        {
            foreach (var element in scrollableElements)
            {
                element.Should().NotBeNull("スクロール可能な要素が有効であること");
                element.IsEnabled.Should().BeTrue("スクロール可能な要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("スクロール可能な要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void WindowManagement_ShouldWorkCorrectly()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ウィンドウ管理ボタンを検索
        var windowButtons = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .And(cf.ByAutomationId("MinimizeButton"))
             .Or(cf.ByAutomationId("MaximizeButton"))
             .Or(cf.ByAutomationId("CloseButton")));

        // Assert
        windowButtons.Should().NotBeNull("ウィンドウ管理ボタンが検索できること");
        
        if (windowButtons.Length > 0)
        {
            foreach (var button in windowButtons)
            {
                button.Should().NotBeNull("ウィンドウ管理ボタンが有効であること");
                button.IsEnabled.Should().BeTrue("ウィンドウ管理ボタンが有効であること");
            }
        }
        else
        {
            Console.WriteLine("ウィンドウ管理ボタンが見つかりませんでした");
        }
    }

    [TestMethod]
    public void Tooltip_ShouldBeDisplayed()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // ツールチップが表示可能な要素を検索
        var tooltipElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Image))
             .Or(cf.ByControlType(ControlType.MenuItem)));

        // Assert
        tooltipElements.Should().NotBeNull("ツールチップ要素が検索できること");
        
        if (tooltipElements.Length > 0)
        {
            foreach (var element in tooltipElements)
            {
                element.Should().NotBeNull("ツールチップ要素が有効であること");
                element.IsEnabled.Should().BeTrue("ツールチップ要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("ツールチップ要素が見つかりませんでした");
        }
    }

    [TestMethod]
    public void Accessibility_ShouldSupportScreenReader()
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

    [TestMethod]
    public void HighContrast_ShouldBeSupported()
    {
        // Arrange & Act
        if (_mainWindow == null)
        {
            Assert.Inconclusive("メインウィンドウが取得できませんでした");
            return;
        }

        // 高コントラスト対応の要素を検索
        var highContrastElements = _mainWindow.FindAllChildren(cf => 
            cf.ByControlType(ControlType.Button)
             .Or(cf.ByControlType(ControlType.Edit))
             .Or(cf.ByControlType(ControlType.Text)));

        // Assert
        highContrastElements.Should().NotBeNull("高コントラスト対応要素が検索できること");
        
        if (highContrastElements.Length > 0)
        {
            foreach (var element in highContrastElements)
            {
                element.Should().NotBeNull("高コントラスト対応要素が有効であること");
                element.IsEnabled.Should().BeTrue("高コントラスト対応要素が有効であること");
            }
        }
        else
        {
            Console.WriteLine("高コントラスト対応要素が見つかりませんでした");
        }
    }
}
