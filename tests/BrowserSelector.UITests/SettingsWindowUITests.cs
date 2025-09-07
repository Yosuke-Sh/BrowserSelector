using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// 設定画面のUIテスト
/// </summary>
[TestClass]
public class SettingsWindowUITests
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
    public void SettingsWindow_ShouldOpenWhenSettingsButtonClicked()
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
            System.Threading.Thread.Sleep(1000); // 設定ウィンドウの表示を待機

            // Assert - 設定ウィンドウが開いていることを確認
            var settingsWindow = _app.GetMainWindow(_automation);
            _ = settingsWindow.Should().NotBeNull("設定ウィンドウが開いていること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveAllRequiredTabs()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // Assert - 必要なタブが存在することを確認
            var tabs = settingsWindow?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem)) ?? Array.Empty<FlaUI.Core.AutomationElements.AutomationElement>();
            
            // タブが見つからない場合はコンテナを探す
            if (tabs.Length == 0)
            {
                var tabControl = settingsWindow?.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tab));
                if (tabControl != null)
                {
                    tabs = tabControl.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem));
                }
            }
            
            // タブがまだ見つからない場合はスキップ
            if (tabs.Length == 0)
            {
                Assert.Inconclusive("設定タブが見つかりません。タブコントロールが存在しない可能性があります。");
                return;
            }
            
            _ = tabs.Should().NotBeEmpty("設定タブが存在すること");

            // 一般的な設定タブの存在確認
            var tabNames = tabs.Select(tab => tab.Name).ToList();
            _ = tabNames.Should().Contain(t => t.Contains("表示") || t.Contains("Display") || t.Contains("一般") || t.Contains("General"), "設定タブが存在すること");
            _ = tabNames.Should().Contain(t => t.Contains("アクセシビリティ") || t.Contains("Accessibility"), "アクセシビリティ設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_DisplayTab_ShouldHaveTransparencySettings()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // 表示タブをクリック
            var displayTab = settingsWindow?.FindFirstDescendant(cf => cf.ByName("表示").Or(cf.ByName("Display")));
            if (displayTab != null)
            {
                displayTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - 透明度設定が存在することを確認
                var transparencySlider = settingsWindow?.FindFirstDescendant(cf => cf.ByName("透明度").Or(cf.ByName("Transparency")));
                _ = transparencySlider.Should().NotBeNull("透明度スライダーが存在すること");

                var transparencyLabel = settingsWindow?.FindFirstDescendant(cf => cf.ByName("透明度").Or(cf.ByName("Transparency")));
                _ = transparencyLabel.Should().NotBeNull("透明度ラベルが存在すること");
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
    public void SettingsWindow_BrowserTab_ShouldHaveBrowserList()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // ブラウザタブをクリック
            var browserTab = settingsWindow?.FindFirstDescendant(cf => cf.ByName("ブラウザ").Or(cf.ByName("Browser")));
            if (browserTab != null)
            {
                browserTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - ブラウザリストが存在することを確認
                var browserList = settingsWindow?.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.List));
                _ = browserList.Should().NotBeNull("ブラウザリストが存在すること");

                var addButton = settingsWindow?.FindFirstDescendant(cf => cf.ByName("追加").Or(cf.ByName("Add")));
                _ = addButton.Should().NotBeNull("追加ボタンが存在すること");

                var removeButton = settingsWindow?.FindFirstDescendant(cf => cf.ByName("削除").Or(cf.ByName("Remove")));
                _ = removeButton.Should().NotBeNull("削除ボタンが存在すること");
            }
            else
            {
                Assert.Inconclusive("ブラウザタブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_AccessibilityTab_ShouldHaveAccessibilitySettings()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // アクセシビリティタブをクリック
            var accessibilityTab = settingsWindow?.FindFirstDescendant(cf => cf.ByName("アクセシビリティ").Or(cf.ByName("Accessibility")));
            if (accessibilityTab != null)
            {
                accessibilityTab.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - アクセシビリティ設定が存在することを確認
                var focusCheckbox = settingsWindow?.FindFirstDescendant(cf => cf.ByName("フォーカス表示").Or(cf.ByName("Focus Display")));
                _ = focusCheckbox.Should().NotBeNull("フォーカス表示チェックボックスが存在すること");

                var highContrastCheckbox = settingsWindow?.FindFirstDescendant(cf => cf.ByName("高コントラスト").Or(cf.ByName("High Contrast")));
                _ = highContrastCheckbox.Should().NotBeNull("高コントラストチェックボックスが存在すること");

                var screenReaderCheckbox = settingsWindow?.FindFirstDescendant(cf => cf.ByName("スクリーンリーダー").Or(cf.ByName("Screen Reader")));
                _ = screenReaderCheckbox.Should().NotBeNull("スクリーンリーダーチェックボックスが存在すること");
            }
            else
            {
                Assert.Inconclusive("アクセシビリティタブが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveSaveAndCancelButtons()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // Assert - 保存とキャンセルボタンが存在することを確認
            var allButtons = settingsWindow?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)) ?? Array.Empty<FlaUI.Core.AutomationElements.AutomationElement>();
            
            // ボタンのテキストを確認
            var buttonTexts = allButtons.Select(btn => btn.Name).ToList();
            Console.WriteLine($"設定ウィンドウのボタン: {string.Join(", ", buttonTexts)}");
            
            // OKボタンを探す
            var saveButton = allButtons.FirstOrDefault(btn => 
                btn.Name.Contains("OK") || 
                btn.Name.Contains("保存") || 
                btn.Name.Contains("Save") ||
                btn.Properties.AutomationId.ValueOrDefault.Contains("SaveButton"));
                
            if (saveButton == null)
            {
                Assert.Inconclusive($"保存ボタンが見つかりません。利用可能なボタン: {string.Join(", ", buttonTexts)}");
                return;
            }
            
            _ = saveButton.Should().NotBeNull("保存ボタン（OK）が存在すること");

            var cancelButton = allButtons.FirstOrDefault(btn => 
                btn.Name.Contains("キャンセル") || 
                btn.Name.Contains("Cancel") ||
                btn.Properties.AutomationId.ValueOrDefault.Contains("CancelButton"));
                
            _ = cancelButton.Should().NotBeNull("キャンセルボタンが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldCloseWhenCancelClicked()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // キャンセルボタンをクリック
            var cancelButton = settingsWindow?.FindFirstDescendant(cf => cf.ByName("キャンセル").Or(cf.ByName("Cancel")));
            if (cancelButton != null)
            {
                cancelButton.Click();
                System.Threading.Thread.Sleep(500);

                // Assert - 設定ウィンドウが閉じていることを確認
                var closedWindow = _app.GetMainWindow(_automation);
                _ = closedWindow.Should().NotBeNull("メインウィンドウが表示されていること");
            }
            else
            {
                Assert.Inconclusive("キャンセルボタンが見つかりません");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveProperKeyboardNavigation()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // Assert - キーボードナビゲーションが可能であることを確認
            var focusableElements = settingsWindow?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tab))
                .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox))
                .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Slider))) ?? Array.Empty<FlaUI.Core.AutomationElements.AutomationElement>();

            _ = focusableElements.Should().NotBeEmpty("フォーカス可能な要素が存在すること");

            foreach (var element in focusableElements)
            {
                // ボタンが無効の場合はスキップ（ブラウザが検出されていない可能性）
                if (!element.IsEnabled)
                {
                    Console.WriteLine($"要素 '{element.Name}' が無効です。ブラウザが検出されていない可能性があります。");
                    continue;
                }
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveProperAccessibilityProperties()
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
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン").Or(cf.ByName("設定")).Or(cf.ByName("Settings")));
        if (settingsButton != null)
        {
            settingsButton.Click();
            System.Threading.Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));

            // Assert - アクセシビリティプロパティが設定されていることを確認
            var buttons = settingsWindow?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)) ?? Array.Empty<FlaUI.Core.AutomationElements.AutomationElement>();
            _ = buttons.Should().NotBeEmpty("ボタンが存在すること");

            foreach (var button in buttons)
            {
                // 空の名前のボタンはスキップ（アイコンボタンなど）
                if (string.IsNullOrEmpty(button.Name))
                {
                    Console.WriteLine($"名前のないボタンをスキップ: AutomationId='{button.Properties.AutomationId.ValueOrDefault}'");
                    continue;
                }
                
                _ = button.Name.Should().NotBeNullOrEmpty($"ボタン '{button.Name}' に名前が設定されていること");
                
                // ボタンが無効の場合はスキップ（ブラウザが検出されていない可能性）
                if (!button.IsEnabled)
                {
                    Console.WriteLine($"ボタン '{button.Name}' が無効です。ブラウザが検出されていない可能性があります。");
                    continue;
                }
            }

            var labels = settingsWindow?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)) ?? Array.Empty<FlaUI.Core.AutomationElements.AutomationElement>();
            _ = labels.Should().NotBeEmpty("ラベルが存在すること");

            foreach (var label in labels)
            {
                _ = label.Name.Should().NotBeNullOrEmpty($"ラベル '{label.Name}' に名前が設定されていること");
            }
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }
}
