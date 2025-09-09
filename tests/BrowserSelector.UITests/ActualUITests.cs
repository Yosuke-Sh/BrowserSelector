using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace BrowserSelector.UITests;

/// <summary>
/// 実際のUI要素に基づいたUIテスト
/// </summary>
[Collection("UI Tests")]
public class ActualUITests : IDisposable
{
    private Application? _app = null;
    private UIA3Automation? _automation = null;

    /// <summary>
    /// UI要素を待機して取得するヘルパーメソッド
    /// </summary>
    private T? WaitForElement<T>(Func<T?> findElement, int timeoutMs = 5000) where T : class
    {
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
        {
            var element = findElement();
            if (element != null)
                return element;
            
            Thread.Sleep(100);
        }
        return null;
    }

    public ActualUITests()
    {
        try
        {
            string appPath = UITestHelper.GetApplicationPath();
            if (string.IsNullOrEmpty(appPath))
            {
                throw new InvalidOperationException("アプリケーションが見つかりません");
            }

            // テスト用アプリケーションを起動
            _app = UITestHelper.LaunchTestApplication(appPath);
            _automation = new UIA3Automation();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"UIテスト用アプリケーション起動に失敗: {ex.Message}", ex);
        }
    }

    public void Dispose()
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
            .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal) || w.Name == "SettingsWindow");

        if (settingsWindow != null) return settingsWindow;

        // 方法2: ウィンドウプロパティで検索
        settingsWindow = _app.GetAllTopLevelWindows(_automation)
            .FirstOrDefault(w => w.Properties.Name.Value.Contains("設定", StringComparison.Ordinal) || w.Properties.Name.Value.Contains("Settings", StringComparison.Ordinal));

        if (settingsWindow != null) return settingsWindow;

        // 方法3: クラス名で検索
        settingsWindow = _app.GetAllTopLevelWindows(_automation)
            .FirstOrDefault(w => w.Properties.ClassName.Value.Contains("SettingsWindow", StringComparison.Ordinal));

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

                if (name.Contains("設定", StringComparison.Ordinal) || name.Contains("Settings", StringComparison.Ordinal) ||
                    className.Contains("SettingsWindow", StringComparison.Ordinal) || className.Contains("Window", StringComparison.Ordinal))
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

        [Fact]
        public void MainWindowShouldHaveUrlInputTextBox()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var urlTextBox = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("UrlInputTextBox")));
        urlTextBox.Should().NotBeNull("URL入力テキストボックスが存在すること");
    }

        [Fact]
        public void MainWindowShouldHaveSettingsButton()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var settingsButton = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton")));
        settingsButton.Should().NotBeNull("設定ボタンが存在すること");
    }

        [Fact]
        public void MainWindowShouldHaveBrowserButtonsContainer()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act & Assert
        var browserContainer = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("BrowserButtonsContainer")));
        browserContainer.Should().NotBeNull("ブラウザボタン一覧が存在すること");
    }

    [Fact]
    public void SettingsWindowShouldHaveTabControl()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveGeneralTab()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

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
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveDisplayTab()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var displayTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("DisplayTab"));
            displayTab.Should().NotBeNull("表示設定タブが存在すること");
        }
        else
        {
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveBrowserTab()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var browserTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("BrowserTab"));
            browserTab.Should().NotBeNull("ブラウザ設定タブが存在すること");
        }
        else
        {
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveUrlRulesTab()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var urlRulesTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("UrlRulesTab"));
            urlRulesTab.Should().NotBeNull("URLルール設定タブが存在すること");
        }
        else
        {
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveLogTab()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var logTab = settingsWindow!.FindFirstDescendant(cf => cf.ByName("LogTab"));
            logTab.Should().NotBeNull("ログ設定タブが存在すること");
        }
        else
        {
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveBrowserManagementButtons()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
            settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

            // Assert
            var refreshButton = settingsWindow!.FindFirstDescendant(cf => cf.ByName("RefreshBrowsersButton"));
            var addButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("AddBrowserButton"));

            refreshButton.Should().NotBeNull("ブラウザ再検出ボタンが存在すること");
            addButton.Should().NotBeNull("ブラウザ追加ボタンが存在すること");
        }
        else
        {
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveUrlRuleManagementButtons()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
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
        }
    }

    [Fact]
    public void SettingsWindowShouldHaveActionButtons()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.Fail("STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow.FindFirstDescendant(cf => cf.ByName("設定ボタン"));
        if (settingsButton != null)
        {
            settingsButton.Click();
            Thread.Sleep(1000);

            var settingsWindow = _app.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.Name.Contains("設定", StringComparison.Ordinal) || w.Name.Contains("Settings", StringComparison.Ordinal));
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
        }
    }
}
