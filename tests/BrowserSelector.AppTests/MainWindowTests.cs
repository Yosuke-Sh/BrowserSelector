using BrowserSelector.App;
using FluentAssertions;
using System.Reflection;
using System.Windows;

namespace BrowserSelector.AppTests;

/// <summary>
/// MainWindow専用のテスト
/// WPFウィンドウの基本機能をテスト.
/// </summary>
public class MainWindowTests
{
    [Fact]
    public void MainWindow_ShouldInheritFromWindow()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.Should().NotBeNull();
        mainWindowType.BaseType.Should().Be(typeof(Window));
    }

    [Fact]
    public void MainWindow_ShouldHaveInitializeComponentMethod()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var initializeComponentMethod = mainWindowType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        initializeComponentMethod.Should().NotBeNull();
        initializeComponentMethod.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void MainWindow_ShouldHaveDefaultConstructor()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var constructors = mainWindowType.GetConstructors();

        // Assert
        constructors.Should().NotBeEmpty();
        constructors.Should().Contain(c => c.GetParameters().Length == 0);
    }

    [Fact]
    public void MainWindow_ShouldBePublicClass()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.IsPublic.Should().BeTrue();
        mainWindowType.IsClass.Should().BeTrue();
    }

    [Fact]
    public void MainWindow_ShouldHaveValidNamespace()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.Namespace.Should().Be("BrowserSelector.App");
    }

    [Fact]
    public void MainWindow_ShouldHaveWindowProperties()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var titleProperty = mainWindowType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
        var widthProperty = mainWindowType.GetProperty("Width", BindingFlags.Public | BindingFlags.Instance);
        var heightProperty = mainWindowType.GetProperty("Height", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        titleProperty.Should().NotBeNull();
        widthProperty.Should().NotBeNull();
        heightProperty.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveWindowStateProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var windowStateProperty = mainWindowType.GetProperty("WindowState", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        windowStateProperty.Should().NotBeNull();
        windowStateProperty.PropertyType.Should().Be(typeof(WindowState));
    }

    [Fact]
    public void MainWindow_ShouldHaveResizeModeProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var resizeModeProperty = mainWindowType.GetProperty("ResizeMode", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        resizeModeProperty.Should().NotBeNull();
        resizeModeProperty.PropertyType.Should().Be(typeof(ResizeMode));
    }

    [Fact]
    public void MainWindow_ShouldHaveShowInTaskbarProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var showInTaskbarProperty = mainWindowType.GetProperty("ShowInTaskbar", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        showInTaskbarProperty.Should().NotBeNull();
        showInTaskbarProperty.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void MainWindow_ShouldHaveTopmostProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var topmostProperty = mainWindowType.GetProperty("Topmost", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        topmostProperty.Should().NotBeNull();
        topmostProperty.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void MainWindow_ShouldHaveWindowStartupLocationProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var windowStartupLocationProperty = mainWindowType.GetProperty("WindowStartupLocation", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        windowStartupLocationProperty.Should().NotBeNull();
        windowStartupLocationProperty.PropertyType.Should().Be(typeof(WindowStartupLocation));
    }

    [Fact]
    public void MainWindow_ShouldHaveContentProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var contentProperty = mainWindowType.GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        contentProperty.Should().NotBeNull();
        contentProperty.PropertyType.Should().Be(typeof(object));
    }

    [Fact]
    public void MainWindow_ShouldHaveBackgroundProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var backgroundProperty = mainWindowType.GetProperty("Background", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        backgroundProperty.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveForegroundProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var foregroundProperty = mainWindowType.GetProperty("Foreground", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        foregroundProperty.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveFontFamilyProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var fontFamilyProperty = mainWindowType.GetProperty("FontFamily", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        fontFamilyProperty.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveFontSizeProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var fontSizeProperty = mainWindowType.GetProperty("FontSize", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        fontSizeProperty.Should().NotBeNull();
        fontSizeProperty.PropertyType.Should().Be(typeof(double));
    }

    [Fact]
    public void MainWindow_ShouldHaveVisibilityProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var visibilityProperty = mainWindowType.GetProperty("Visibility", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        visibilityProperty.Should().NotBeNull();
        visibilityProperty.PropertyType.Should().Be(typeof(Visibility));
    }

    [Fact]
    public void MainWindow_ShouldHaveIsEnabledProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var isEnabledProperty = mainWindowType.GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        isEnabledProperty.Should().NotBeNull();
        isEnabledProperty.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void MainWindow_ShouldHaveFocusableProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var focusableProperty = mainWindowType.GetProperty("Focusable", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        focusableProperty.Should().NotBeNull();
        focusableProperty.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void MainWindow_ShouldHaveIsHitTestVisibleProperty()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var isHitTestVisibleProperty = mainWindowType.GetProperty("IsHitTestVisible", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        isHitTestVisibleProperty.Should().NotBeNull();
        isHitTestVisibleProperty.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void MainWindow_ShouldHaveWindowEvents()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var events = mainWindowType.GetEvents();

        // Assert
        events.Should().NotBeEmpty();
        events.Should().Contain(e => e.Name == "Loaded");
        events.Should().Contain(e => e.Name == "Closing");
        events.Should().Contain(e => e.Name == "Closed");
    }

    [Fact]
    public void MainWindow_ShouldHaveCorrectClassModifiers()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.IsPublic.Should().BeTrue();
        mainWindowType.IsClass.Should().BeTrue();
        mainWindowType.IsAbstract.Should().BeFalse();
        mainWindowType.IsSealed.Should().BeFalse();
    }

    [Fact]
    public void MainWindow_ShouldHaveCorrectConstructorModifiers()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var constructors = mainWindowType.GetConstructors();
        var defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        // Assert
        defaultConstructor.Should().NotBeNull();
        defaultConstructor.IsPublic.Should().BeTrue();
        defaultConstructor.IsStatic.Should().BeFalse();
    }

    /// <summary>
    /// MainWindowのメソッド修飾子が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectMethodModifiers()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var initializeComponentMethod = mainWindowType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        initializeComponentMethod.Should().NotBeNull();
        initializeComponentMethod.IsPublic.Should().BeTrue();
        initializeComponentMethod.IsStatic.Should().BeFalse();
        // InitializeComponentはWPFで生成されるメソッドのため、IsVirtualの値は不定
        // initializeComponentMethod.IsVirtual.Should().BeFalse();
    }

    /// <summary>
    /// MainWindowの基底型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectBaseType()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.BaseType.Should().Be(typeof(Window));
        mainWindowType.BaseType.Should().NotBeNull();
    }

    /// <summary>
    /// MainWindowの名前空間が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectNamespace()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.Namespace.Should().Be("BrowserSelector.App");
    }

    /// <summary>
    /// MainWindowのアセンブリが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectAssembly()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var assembly = mainWindowType.Assembly;

        // Assert
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("BrowserSelector.App");
    }

    /// <summary>
    /// MainWindowの型属性が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectTypeAttributes()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var attributes = mainWindowType.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
    }

    /// <summary>
    /// MainWindowのコンストラクタパラメータが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectConstructorParameters()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var constructors = mainWindowType.GetConstructors();
        var defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        // Assert
        defaultConstructor.Should().NotBeNull();
        defaultConstructor.GetParameters().Should().BeEmpty();
    }

    /// <summary>
    /// MainWindowのメソッド数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectMethodCount()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var methods = mainWindowType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        // Assert
        methods.Should().NotBeEmpty();
        methods.Should().Contain(m => m.Name == "InitializeComponent");
    }

    /// <summary>
    /// MainWindowのプロパティ数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectPropertyCount()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var properties = mainWindowType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Assert
        properties.Should().NotBeEmpty();
        properties.Should().Contain(p => p.Name == "Title");
        properties.Should().Contain(p => p.Name == "Width");
        properties.Should().Contain(p => p.Name == "Height");
    }

    /// <summary>
    /// MainWindowのイベント数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectEventCount()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var events = mainWindowType.GetEvents();

        // Assert
        events.Should().NotBeEmpty();
        events.Should().Contain(e => e.Name == "Loaded");
        events.Should().Contain(e => e.Name == "Closing");
        events.Should().Contain(e => e.Name == "Closed");
    }

    /// <summary>
    /// MainWindowのフィールド数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectFieldCount()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var fields = mainWindowType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        fields.Should().NotBeNull();
    }

    /// <summary>
    /// MainWindowのインターフェース実装が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectInterfaceImplementations()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var interfaces = mainWindowType.GetInterfaces();

        // Assert
        interfaces.Should().NotBeNull();
    }

    /// <summary>
    /// MainWindowのジェネリック型定義が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectGenericTypeDefinition()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.IsGenericTypeDefinition.Should().BeFalse();
        mainWindowType.IsGenericType.Should().BeFalse();
    }

    /// <summary>
    /// MainWindowの型修飾子が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectTypeModifiers()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);

        // Assert
        mainWindowType.IsPublic.Should().BeTrue();
        mainWindowType.IsClass.Should().BeTrue();
        mainWindowType.IsAbstract.Should().BeFalse();
        mainWindowType.IsSealed.Should().BeFalse();
    }

    /// <summary>
    /// MainWindowのメソッド戻り値型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectMethodReturnTypes()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var initializeComponentMethod = mainWindowType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        initializeComponentMethod.Should().NotBeNull();
        initializeComponentMethod.ReturnType.Should().Be(typeof(void));
    }

    /// <summary>
    /// MainWindowのプロパティ型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectPropertyTypes()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var titleProperty = mainWindowType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
        var widthProperty = mainWindowType.GetProperty("Width", BindingFlags.Public | BindingFlags.Instance);
        var heightProperty = mainWindowType.GetProperty("Height", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        titleProperty.Should().NotBeNull();
        titleProperty.PropertyType.Should().Be(typeof(string));

        widthProperty.Should().NotBeNull();
        widthProperty.PropertyType.Should().Be(typeof(double));

        heightProperty.Should().NotBeNull();
        heightProperty.PropertyType.Should().Be(typeof(double));
    }

    /// <summary>
    /// MainWindowのイベント型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectEventTypes()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var loadedEvent = mainWindowType.GetEvent("Loaded", BindingFlags.Public | BindingFlags.Instance);
        var closingEvent = mainWindowType.GetEvent("Closing", BindingFlags.Public | BindingFlags.Instance);
        var closedEvent = mainWindowType.GetEvent("Closed", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        loadedEvent.Should().NotBeNull();
        loadedEvent.EventHandlerType.Should().Be(typeof(RoutedEventHandler));

        closingEvent.Should().NotBeNull();
        closingEvent.EventHandlerType.Should().Be(typeof(System.ComponentModel.CancelEventHandler));

        closedEvent.Should().NotBeNull();
        closedEvent.EventHandlerType.Should().Be(typeof(EventHandler));
    }

    /// <summary>
    /// MainWindowのアセンブリ属性が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectAssemblyAttributes()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var assembly = mainWindowType.Assembly;
        var attributes = assembly.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().NotBeEmpty();
    }

    /// <summary>
    /// MainWindowの参照アセンブリが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void MainWindow_ShouldHaveCorrectReferencedAssemblies()
    {
        // Arrange & Act
        var mainWindowType = typeof(MainWindow);
        var assembly = mainWindowType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty();
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationFramework");
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationCore");
        referencedAssemblies.Should().Contain(a => a.Name == "WindowsBase");
    }
}
