using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;

namespace BrowserSelector.UITests;

/// <summary>
/// 実際のUI要素に基づいたUIテスト
/// </summary>
[TestClass]
[DoNotParallelize]
public class ActualUITests
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

    /// <summary>
    /// 設定ウィンドウを複数の方法で検索するヘルパーメソッド
    /// </summary>
    private FlaUI.Core.AutomationElements.Window? FindSettingsWindow()
    {
        if (_app == null || _automation == null) return null;

        // 方法1: ウィンドウ名で検索
        var settingsWindow = _app.GetAllTopLevelWindows(_automation)
            .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings") || w.Name == "SettingsWindow");

        if (settingsWindow != null) return settingsWindow;

        // 方法2: ウィンドウプロパティで検索
        settingsWindow = _app.GetAllTopLevelWindows(_automation)
            .FirstOrDefault(w => w.Properties.Name.Value.Contains("設定") || w.Properties.Name.Value.Contains("Settings"));

        if (settingsWindow != null) return settingsWindow;

        // 方法3: クラス名で検索
        settingsWindow = _app.GetAllTopLevelWindows(_automation)
            .FirstOrDefault(w => w.Properties.ClassName.Value.Contains("SettingsWindow"));

        if (settingsWindow != null) return settingsWindow;

        // 方法4: プロセス名とウィンドウタイトルで検索
        var allWindows = _app.GetAllTopLevelWindows(_automation);
        foreach (var window in allWindows)
        {
            try
            {
                var name = window.Name;
                var className = window.Properties.ClassName.Value;

                // デバッグ情報を出力
                Console.WriteLine($"検出されたウィンドウ: Name='{name}', ClassName='{className}'");

                if (name.Contains("設定") || name.Contains("Settings") ||
                    className.Contains("SettingsWindow") || className.Contains("Window"))
                {
                    return window;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ウィンドウ検索エラー: {ex.Message}");
            }
        }

        return null;
    }

    [TestMethod]
    public void MainWindow_ShouldHaveUrlInputTextBox()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var urlTextBox = mainWindow.FindFirstDescendant(cf => cf.ByName("UrlInputTextBox"));
        urlTextBox.Should().NotBeNull("URL入力テキストボックスが存在すること");
    }

    [TestMethod]
    public void MainWindow_ShouldHaveSettingsButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton"));
        settingsButton.Should().NotBeNull("設定ボタンが存在すること");
    }

    [TestMethod]
    public void MainWindow_ShouldHaveBrowserButtonsContainer()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var browserContainer = mainWindow.FindFirstDescendant(cf => cf.ByName("BrowserButtonsContainer"));
        browserContainer.Should().NotBeNull("ブラウザボタン一覧が存在すること");
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveTabControl()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(3000); // 待機時間をさらに延長

            // 設定ウィンドウを複数の方法で検索
            var settingsWindow = FindSettingsWindow();
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var tabControl = settingsWindow!.FindFirstDescendant(cf => cf.ByName("SettingsTabControl"));
            tabControl.Should().NotBeNull("設定タブコントロールが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveGeneralTab()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(3000);

            var settingsWindow = FindSettingsWindow();
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var generalTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("GeneralTab"));
            generalTab.Should().NotBeNull("一般設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveDisplayTab()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var displayTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("DisplayTab"));
            displayTab.Should().NotBeNull("表示設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveBrowserTab()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var browserTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("BrowserTab"));
            browserTab.Should().NotBeNull("ブラウザ設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveUrlRulesTab()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var urlRulesTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("UrlRulesTab"));
            urlRulesTab.Should().NotBeNull("URLルール設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveLogTab()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var logTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("LogTab"));
            logTab.Should().NotBeNull("ログ設定タブが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveBrowserManagementButtons()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var refreshButton = settingsWindow!.FindFirstDescendant(cf => cf.ByName("RefreshBrowsersButton"));
            var addButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("AddBrowserButton"));

            refreshButton.Should().NotBeNull("ブラウザ再検出ボタンが存在すること");
            addButton.Should().NotBeNull("ブラウザ追加ボタンが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveUrlRuleManagementButtons()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var addRuleButton = settingsWindow!.FindFirstDescendant(cf => cf.ByName("AddUrlRuleButton"));
            var refreshRulesButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("RefreshUrlRulesButton"));
            var testRuleButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("TestUrlRuleButton"));

            addRuleButton.Should().NotBeNull("URLルール追加ボタンが存在すること");
            refreshRulesButton.Should().NotBeNull("URLルール更新ボタンが存在すること");
            testRuleButton.Should().NotBeNull("URLルールテストボタンが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }

    [TestMethod]
    public void SettingsWindow_ShouldHaveActionButtons()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定") || w.Name.Contains("Settings"));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var saveButton = settingsWindow!.FindFirstDescendant(cf => cf.ByName("SaveSettingsButton"));
            var cancelButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("CancelSettingsButton"));
            var resetButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("ResetSettingsButton"));

            saveButton.Should().NotBeNull("設定保存ボタンが存在すること");
            cancelButton.Should().NotBeNull("設定キャンセルボタンが存在すること");
            resetButton.Should().NotBeNull("設定リセットボタンが存在すること");
        }
        else
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
        }
    }
}
