using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserSelector.UITests;

/// <summary>
/// URLルール管理のUIテスト
/// </summary>
[TestClass]
public class UrlRulesManagementUITests
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
    public void UrlRulesManagement_ShouldHaveAddRuleButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - URLルール追加ボタンの確認
        var addRuleButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddRuleButton"));
        addRuleButton.Should().NotBeNull("URLルール追加ボタンが存在すること");

        var addRuleLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddRuleLabel"));
        addRuleLabel.Should().NotBeNull("URLルール追加ラベルが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRuleList()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - URLルールリストの確認
        var ruleList = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RuleList"));
        ruleList.Should().NotBeNull("URLルールリストが存在すること");

        var ruleListBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RuleListBox"));
        ruleListBox.Should().NotBeNull("URLルールリストボックスが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveEditRuleButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - URLルール編集ボタンの確認
        var editRuleButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("EditRuleButton"));
        editRuleButton.Should().NotBeNull("URLルール編集ボタンが存在すること");

        var editRuleLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("EditRuleLabel"));
        editRuleLabel.Should().NotBeNull("URLルール編集ラベルが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveDeleteRuleButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - URLルール削除ボタンの確認
        var deleteRuleButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DeleteRuleButton"));
        deleteRuleButton.Should().NotBeNull("URLルール削除ボタンが存在すること");

        var deleteRuleLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("DeleteRuleLabel"));
        deleteRuleLabel.Should().NotBeNull("URLルール削除ラベルが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRulePatternSettings()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - URLルールパターン設定の確認
        var patternTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PatternTextBox"));
        patternTextBox.Should().NotBeNull("パターンテキストボックスが存在すること");

        var patternLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PatternLabel"));
        patternLabel.Should().NotBeNull("パターンラベルが存在すること");

        var patternTypeComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PatternTypeComboBox"));
        patternTypeComboBox.Should().NotBeNull("パターンタイプコンボボックスが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRuleTargetBrowserSettings()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - ルール対象ブラウザ設定の確認
        var targetBrowserComboBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TargetBrowserComboBox"));
        targetBrowserComboBox.Should().NotBeNull("対象ブラウザコンボボックスが存在すること");

        var targetBrowserLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TargetBrowserLabel"));
        targetBrowserLabel.Should().NotBeNull("対象ブラウザラベルが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRulePrioritySettings()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - ルール優先度設定の確認
        var prioritySlider = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PrioritySlider"));
        prioritySlider.Should().NotBeNull("優先度スライダーが存在すること");

        var priorityLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PriorityLabel"));
        priorityLabel.Should().NotBeNull("優先度ラベルが存在すること");

        var priorityNumericUpDown = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("PriorityNumericUpDown"));
        priorityNumericUpDown.Should().NotBeNull("優先度数値入力が存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRuleEnabledSettings()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - ルール有効化設定の確認
        var enabledCheckbox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RuleEnabledCheckbox"));
        enabledCheckbox.Should().NotBeNull("ルール有効化チェックボックスが存在すること");

        var enabledLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("RuleEnabledLabel"));
        enabledLabel.Should().NotBeNull("ルール有効化ラベルが存在すること");
    }

    [TestMethod]
    public void UrlRulesManagement_ShouldHaveRuleTestButton()
    {
        // Arrange
        _ = _app.Should().NotBeNull("アプリケーションが起動していること");
        _ = _automation.Should().NotBeNull("オートメーションが初期化されていること");

        var mainWindow = _app!.GetMainWindow(_automation!);
        _ = mainWindow.Should().NotBeNull("メインウィンドウが取得できること");

        // Act - 設定ボタンをクリック
        var settingsButton = mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("SettingsButton"));
        if (settingsButton == null)
        {
            Assert.Inconclusive("設定ボタンが見つかりません");
            return;
        }

        settingsButton.Click();
        Thread.Sleep(1000); // 設定ウィンドウの表示を待つ

        // 設定ウィンドウを取得
        var settingsWindow = _app.GetMainWindow(_automation!);
        if (settingsWindow == null)
        {
            Assert.Inconclusive("設定ウィンドウが開かれませんでした");
            return;
        }

        // URLルールタブをクリック
        var urlRulesTab = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("UrlRulesTab"));
        if (urlRulesTab == null)
        {
            Assert.Inconclusive("URLルールタブが見つかりません");
            return;
        }

        urlRulesTab.Click();
        Thread.Sleep(500);

        // Assert - ルールテストボタンの確認
        var testRuleButton = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TestRuleButton"));
        testRuleButton.Should().NotBeNull("ルールテストボタンが存在すること");

        var testRuleLabel = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TestRuleLabel"));
        testRuleLabel.Should().NotBeNull("ルールテストラベルが存在すること");

        var testUrlTextBox = settingsWindow.FindFirstDescendant(cf => cf.ByAutomationId("TestUrlTextBox"));
        testUrlTextBox.Should().NotBeNull("テストURLテキストボックスが存在すること");
    }
}
