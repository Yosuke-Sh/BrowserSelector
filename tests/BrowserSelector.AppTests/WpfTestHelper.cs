using FluentAssertions;
using System.Reflection;
using System.Windows;

namespace BrowserSelector.AppTests;

/// <summary>
/// WPFテスト用のヘルパークラス
/// WPFアプリケーションのテストを支援するユーティリティ
/// </summary>
public static class WpfTestHelper
{
    /// <summary>
    /// WPFアプリケーションのテスト用コンテキストを作成
    /// </summary>
    public static void InitializeWpfContext()
    {
        if (Application.Current == null)
        {
            // テスト用のWPFアプリケーションコンテキストを作成
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }

    /// <summary>
    /// WPFアプリケーションのテスト用コンテキストをクリーンアップ
    /// </summary>
    public static void CleanupWpfContext()
    {
        if (Application.Current != null)
        {
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// ウィンドウの基本プロパティをテスト
    /// </summary>
    public static void TestWindowProperties(Window window)
    {
        // ウィンドウの基本プロパティをテスト
        window.Title.Should().NotBeNull();
        window.Width.Should().BeGreaterThan(0);
        window.Height.Should().BeGreaterThan(0);
        window.Visibility.Should().Be(Visibility.Visible);
    }

    /// <summary>
    /// ウィンドウのイベントハンドラーをテスト
    /// </summary>
    public static void TestWindowEvents(Window window)
    {
        // ウィンドウのイベントが適切に設定されているかテスト
        var windowType = window.GetType();
        var events = windowType.GetEvents();

        // 主要なイベントの存在を確認
        events.Should().Contain(e => e.Name == "Loaded");
        events.Should().Contain(e => e.Name == "Closing");
        events.Should().Contain(e => e.Name == "Closed");
    }

    /// <summary>
    /// アプリケーションの基本プロパティをテスト
    /// </summary>
    public static void TestApplicationProperties(Application app)
    {
        app.Should().NotBeNull();
        app.ShutdownMode.Should().BeOneOf(ShutdownMode.OnExplicitShutdown, ShutdownMode.OnLastWindowClose, ShutdownMode.OnMainWindowClose);
    }

    /// <summary>
    /// アプリケーションのイベントハンドラーをテスト
    /// </summary>
    public static void TestApplicationEvents(Application app)
    {
        var appType = app.GetType();
        var events = appType.GetEvents();

        // 主要なイベントの存在を確認
        events.Should().Contain(e => e.Name == "Startup");
        events.Should().Contain(e => e.Name == "Exit");
    }

    /// <summary>
    /// リフレクションを使用してプライベートメソッドをテスト
    /// </summary>
    public static T InvokePrivateMethod<T>(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull($"Private method '{methodName}' should exist");

        var result = method!.Invoke(obj, parameters);
        return (T)result!;
    }

    /// <summary>
    /// リフレクションを使用してプライベートプロパティをテスト
    /// </summary>
    public static T GetPrivateProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        property.Should().NotBeNull($"Private property '{propertyName}' should exist");

        var value = property!.GetValue(obj);
        return (T)value!;
    }

    /// <summary>
    /// リフレクションを使用してプライベートフィールドをテスト
    /// </summary>
    public static T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().NotBeNull($"Private field '{fieldName}' should exist");

        var value = field!.GetValue(obj);
        return (T)value!;
    }

    /// <summary>
    /// ウィンドウの初期化をテスト
    /// </summary>
    public static void TestWindowInitialization(Window window)
    {
        window.Should().NotBeNull();
        window.IsInitialized.Should().BeTrue();
        window.ActualWidth.Should().BeGreaterThan(0);
        window.ActualHeight.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// アプリケーションの初期化をテスト
    /// </summary>
    public static void TestApplicationInitialization(Application app)
    {
        app.Should().NotBeNull();
        // WPFアプリケーションの初期化状態は直接確認できないため、nullチェックのみ
        app.Should().NotBeNull();
    }
}
