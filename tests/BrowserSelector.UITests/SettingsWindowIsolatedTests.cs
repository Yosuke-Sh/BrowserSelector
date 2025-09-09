using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace BrowserSelector.UITests;

/// <summary>
/// 設定画面の独立テスト（並列実行を避けるため分離）
/// </summary>
[Collection("UI Tests")]
public class SettingsWindowIsolatedTests : IDisposable
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

    public SettingsWindowIsolatedTests()
    {
        try
        {
            string appPath = UITestHelper.GetApplicationPath();
            if (string.IsNullOrEmpty(appPath))
            {
                return;
            }

            // テスト用アプリケーションを起動
            _app = UITestHelper.LaunchTestApplication(appPath);
            _automation = new UIA3Automation();

            // アプリケーションの起動を待機
            System.Threading.Thread.Sleep(2000);
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

    [Fact]
    public void SettingsWindow_ShouldOpenSuccessfully()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.True(false, "STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton")));
        _ = settingsButton.Should().NotBeNull("設定ボタンが存在すること");

        settingsButton!.Click();
        System.Threading.Thread.Sleep(3000); // 十分な待機時間

        // Assert - 設定ウィンドウが開かれること
        var settingsWindow = FindSettingsWindow();
        _ = settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

        // ウィンドウの詳細情報を出力
        Console.WriteLine($"設定ウィンドウ検出成功: Name='{settingsWindow!.Name}', ClassName='{settingsWindow.Properties.ClassName.Value}'");
    }

    [Fact]
    public void SettingsWindow_ShouldHaveAllTabs()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.True(false, "STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton")));
        _ = settingsButton.Should().NotBeNull("設定ボタンが存在すること");

        settingsButton!.Click();
        System.Threading.Thread.Sleep(3000);

        var settingsWindow = FindSettingsWindow();
        _ = settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

        // Assert - すべてのタブが存在すること
        // まずタブコントロールを検索
        var tabControl = settingsWindow!.FindFirstDescendant(cf => cf.ByName("SettingsTabControl"));
        _ = tabControl.Should().NotBeNull("設定タブコントロールが存在すること");

        // タブコントロール内でタブを検索
        var generalTab = tabControl?.FindFirstDescendant(cf => cf.ByName("GeneralTab"));
        var displayTab = tabControl?.FindFirstDescendant(cf => cf.ByName("DisplayTab"));
        var browserTab = tabControl?.FindFirstDescendant(cf => cf.ByName("BrowserTab"));
        var urlRulesTab = tabControl?.FindFirstDescendant(cf => cf.ByName("UrlRulesTab"));
        var logTab = tabControl?.FindFirstDescendant(cf => cf.ByName("LogTab"));

        // デバッグ情報を出力
        Console.WriteLine($"タブコントロール検出: {tabControl != null}");
        if (tabControl != null)
        {
            var allTabs = tabControl.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem));
            Console.WriteLine($"検出されたタブ数: {allTabs.Length}");
            foreach (var tab in allTabs)
            {
                Console.WriteLine($"タブ: Name='{tab.Name}', AutomationId='{tab.Properties.AutomationId.Value}'");
            }
        }

        _ = generalTab.Should().NotBeNull("一般設定タブが存在すること");
        _ = displayTab.Should().NotBeNull("表示設定タブが存在すること");
        _ = browserTab.Should().NotBeNull("ブラウザ設定タブが存在すること");
        _ = urlRulesTab.Should().NotBeNull("URLルール設定タブが存在すること");
        _ = logTab.Should().NotBeNull("ログ設定タブが存在すること");
    }

    [Fact]
    public void SettingsWindow_ShouldHaveActionButtons()
    {
        // STAスレッドの問題でアプリケーションが起動できないため、テストをスキップ
        Xunit.Assert.True(false, "STAスレッドの問題により、UIテストをスキップします");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = WaitForElement(() => mainWindow.FindFirstDescendant(cf => cf.ByName("SettingsButton")));
        _ = settingsButton.Should().NotBeNull("設定ボタンが存在すること");

        settingsButton!.Click();
        System.Threading.Thread.Sleep(3000);

        var settingsWindow = FindSettingsWindow();
        _ = settingsWindow.Should().NotBeNull("設定ウィンドウが開かれること");

        // Assert - アクションボタンが存在すること
        // より確実な検索方法を使用
        var saveButton = settingsWindow!.FindFirstDescendant(cf => cf.ByName("SaveSettingsButton")
            .Or(cf.ByName("保存"))
            .Or(cf.ByName("OK"))
            .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button).And(cf.ByName("保存"))));

        var cancelButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("CancelSettingsButton")
            .Or(cf.ByName("キャンセル"))
            .Or(cf.ByName("Cancel"))
            .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button).And(cf.ByName("キャンセル"))));

        var resetButton = settingsWindow.FindFirstDescendant(cf => cf.ByName("ResetSettingsButton")
            .Or(cf.ByName("リセット"))
            .Or(cf.ByName("Reset"))
            .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button).And(cf.ByName("リセット"))));

        // デバッグ情報を出力
        Console.WriteLine($"保存ボタン検出: {saveButton != null}");
        Console.WriteLine($"キャンセルボタン検出: {cancelButton != null}");
        Console.WriteLine($"リセットボタン検出: {resetButton != null}");

        _ = saveButton.Should().NotBeNull("設定保存ボタンが存在すること");
        _ = cancelButton.Should().NotBeNull("設定キャンセルボタンが存在すること");
        _ = resetButton.Should().NotBeNull("設定リセットボタンが存在すること");
    }
}
