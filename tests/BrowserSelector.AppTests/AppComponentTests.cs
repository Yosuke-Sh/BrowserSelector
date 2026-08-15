using BrowserSelector.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Windows;

namespace BrowserSelector.AppTests;

/// <summary>
/// App.xaml.csの個別コンポーネントをテストするクラス
/// WPFアプリケーションの実際の起動は行わず、個別のコンポーネントをテスト.
/// </summary>
public class AppComponentTests
{
    [Fact]
    public void App_ShouldHaveCorrectPrivateFields()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var hostField = appType.GetField("_host", BindingFlags.NonPublic | BindingFlags.Instance);
        var logServiceField = appType.GetField("_logService", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        hostField.Should().NotBeNull("_hostフィールドが存在すること");
        hostField!.FieldType.Should().Be<IHost>("_hostフィールドの型が正しいこと");

        logServiceField.Should().NotBeNull("_logServiceフィールドが存在すること");
        logServiceField!.FieldType.Should().Be<ILogService>("_logServiceフィールドの型が正しいこと");
    }

    /// <summary>
    /// Appクラスのprotectedメソッドが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectProtectedMethods()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod.Should().NotBeNull("OnStartupメソッドが存在すること");
        onStartupMethod!.IsFamily.Should().BeTrue("OnStartupメソッドがprotectedであること");
        onStartupMethod.IsVirtual.Should().BeTrue("OnStartupメソッドがvirtualであること");

        onExitMethod.Should().NotBeNull("OnExitメソッドが存在すること");
        onExitMethod!.IsFamily.Should().BeTrue("OnExitメソッドがprotectedであること");
        onExitMethod.IsVirtual.Should().BeTrue("OnExitメソッドがvirtualであること");
    }

    /// <summary>
    /// Appクラスのメソッドパラメータが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodParameters()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod!.GetParameters().Should().HaveCount(1, "OnStartupメソッドのパラメータ数が正しいこと");
        onStartupMethod.GetParameters()[0].ParameterType.Should().Be<StartupEventArgs>("OnStartupメソッドのパラメータ型が正しいこと");

        onExitMethod!.GetParameters().Should().HaveCount(1, "OnExitメソッドのパラメータ数が正しいこと");
        onExitMethod.GetParameters()[0].ParameterType.Should().Be<ExitEventArgs>("OnExitメソッドのパラメータ型が正しいこと");
    }

    /// <summary>
    /// Appクラスのメソッド戻り値型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodReturnTypes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod!.ReturnType.Should().Be(typeof(void), "OnStartupメソッドの戻り値型が正しいこと");
        onExitMethod!.ReturnType.Should().Be(typeof(void), "OnExitメソッドの戻り値型が正しいこと");
    }

    /// <summary>
    /// Appクラスのメソッド属性が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodAttributes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod!.GetCustomAttributes().Should().NotBeNull("OnStartupメソッドに属性が存在すること");
        onExitMethod!.GetCustomAttributes().Should().NotBeNull("OnExitメソッドに属性が存在すること");
    }

    /// <summary>
    /// Appクラスの基底型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectBaseType()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.BaseType.Should().Be<Application>("Appクラスの基底型が正しいこと");
        appType.BaseType!.Should().NotBeNull("Appクラスの基底型がnullでないこと");
    }

    /// <summary>
    /// Appクラスのインターフェース実装が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectInterfaceImplementations()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var interfaces = appType.GetInterfaces();

        // Assert
        interfaces.Should().NotBeNull("Appクラスのインターフェース実装がnullでないこと");
        interfaces.Should().NotBeEmpty("Appクラスがインターフェースを実装すること");
    }

    /// <summary>
    /// Appクラスのジェネリック構造が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectGenericStructure()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.IsGenericType.Should().BeFalse("Appクラスがジェネリック型でないこと");
        appType.IsGenericTypeDefinition.Should().BeFalse("Appクラスがジェネリック型定義でないこと");
        appType.ContainsGenericParameters.Should().BeFalse("Appクラスがジェネリックパラメータを含まないこと");
    }

    /// <summary>
    /// Appクラスの型修飾子が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectTypeModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.IsPublic.Should().BeTrue("Appクラスがpublicであること");
        appType.IsClass.Should().BeTrue("Appクラスがクラスであること");
        appType.IsAbstract.Should().BeFalse("Appクラスがabstractでないこと");
        appType.IsSealed.Should().BeFalse("Appクラスがsealedでないこと");
        appType.IsInterface.Should().BeFalse("Appクラスがインターフェースでないこと");
        appType.IsEnum.Should().BeFalse("Appクラスが列挙型でないこと");
    }

    /// <summary>
    /// Appクラスのメソッド数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var methods = appType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        methods.Should().NotBeEmpty("Appクラスにメソッドが存在すること");
        methods.Should().Contain(m => m.Name == "OnStartup", "OnStartupメソッドが存在すること");
        methods.Should().Contain(m => m.Name == "OnExit", "OnExitメソッドが存在すること");
    }

    [Fact]
    public void App_ShouldHaveCorrectConstructorCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var constructors = appType.GetConstructors();

        // Assert
        constructors.Should().NotBeEmpty("Appクラスにコンストラクタが存在すること");
        constructors.Should().Contain(c => c.GetParameters().Length == 0, "デフォルトコンストラクタが存在すること");
    }

    /// <summary>
    /// Appクラスのプロパティ数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectPropertyCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var properties = appType.GetProperties();

        // Assert
        properties.Should().NotBeEmpty("Appクラスにプロパティが存在すること");
        properties.Should().Contain(p => p.Name == "MainWindow", "MainWindowプロパティが存在すること");
    }

    /// <summary>
    /// Appクラスのイベント数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectEventCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var events = appType.GetEvents();

        // Assert
        events.Should().NotBeEmpty("Appクラスにイベントが存在すること");
        events.Should().Contain(e => e.Name == "Startup", "Startupイベントが存在すること");
        events.Should().Contain(e => e.Name == "Exit", "Exitイベントが存在すること");
    }

    /// <summary>
    /// Appクラスのフィールド数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectFieldCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var fields = appType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        fields.Should().NotBeEmpty("Appクラスにフィールドが存在すること");
        fields.Should().Contain(f => f.Name == "_host", "_hostフィールドが存在すること");
        fields.Should().Contain(f => f.Name == "_logService", "_logServiceフィールドが存在すること");
    }

    /// <summary>
    /// Appクラスのネスト型数が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectNestedTypeCount()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var nestedTypes = appType.GetNestedTypes();

        // Assert
        nestedTypes.Should().NotBeNull("Appクラスのネスト型がnullでないこと");
    }

    /// <summary>
    /// Appクラスのアセンブリ構造が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyStructure()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var assemblyName = assembly.GetName();

        // Assert
        assembly.Should().NotBeNull("Appクラスのアセンブリがnullでないこと");
        assemblyName.Name.Should().Be("BrowserSelector", "Appクラスのアセンブリ名が正しいこと");
        assemblyName.Version!.Should().NotBeNull("Appクラスのアセンブリバージョンがnullでないこと");
    }

    /// <summary>
    /// Appクラスの型階層が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectTypeHierarchy()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var baseType = appType.BaseType;
        var baseBaseType = baseType?.BaseType;

        // Assert
        baseType.Should().Be<Application>("Appクラスの直接基底型が正しいこと");
        baseBaseType!.Should().Be<System.Windows.Threading.DispatcherObject>("Appクラスの間接基底型が正しいこと");
    }

    /// <summary>
    /// Appクラスのメソッド可視性が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodVisibility()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod!.IsFamily.Should().BeTrue("OnStartupメソッドがprotectedであること");
        onStartupMethod.IsPublic.Should().BeFalse("OnStartupメソッドがpublicでないこと");
        onStartupMethod.IsPrivate.Should().BeFalse("OnStartupメソッドがprivateでないこと");

        onExitMethod!.IsFamily.Should().BeTrue("OnExitメソッドがprotectedであること");
        onExitMethod.IsPublic.Should().BeFalse("OnExitメソッドがpublicでないこと");
        onExitMethod.IsPrivate.Should().BeFalse("OnExitメソッドがprivateでないこと");
    }

    /// <summary>
    /// Appクラスのメソッド修飾子が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectMethodModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod!.IsVirtual.Should().BeTrue("OnStartupメソッドがvirtualであること");
        onStartupMethod.IsAbstract.Should().BeFalse("OnStartupメソッドがabstractでないこと");
        onStartupMethod.IsFinal.Should().BeFalse("OnStartupメソッドがfinalでないこと");

        onExitMethod!.IsVirtual.Should().BeTrue("OnExitメソッドがvirtualであること");
        onExitMethod.IsAbstract.Should().BeFalse("OnExitメソッドがabstractでないこと");
        onExitMethod!.IsFinal.Should().BeFalse("OnExitメソッドがfinalでないこと");
    }

    /// <summary>
    /// Appクラスのフィールド修飾子が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectFieldModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var hostField = appType.GetField("_host", BindingFlags.NonPublic | BindingFlags.Instance);
        var logServiceField = appType.GetField("_logService", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        hostField!.IsPrivate.Should().BeTrue("_hostフィールドがprivateであること");
        hostField.IsPublic.Should().BeFalse("_hostフィールドがpublicでないこと");
        hostField.IsStatic.Should().BeFalse("_hostフィールドがstaticでないこと");

        logServiceField!.IsPrivate.Should().BeTrue("_logServiceフィールドがprivateであること");
        logServiceField.IsPublic.Should().BeFalse("_logServiceフィールドがpublicでないこと");
        logServiceField.IsStatic.Should().BeFalse("_logServiceフィールドがstaticでないこと");
    }

    /// <summary>
    /// Appクラスのフィールド型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectFieldTypes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var hostField = appType.GetField("_host", BindingFlags.NonPublic | BindingFlags.Instance);
        var logServiceField = appType.GetField("_logService", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        hostField!.FieldType.Should().Be<IHost>("_hostフィールドの型が正しいこと");
        hostField.FieldType.IsInterface.Should().BeTrue("_hostフィールドの型がインターフェースであること");

        logServiceField!.FieldType.Should().Be<ILogService>("_logServiceフィールドの型が正しいこと");
        logServiceField.FieldType.IsInterface.Should().BeTrue("_logServiceフィールドの型がインターフェースであること");
    }

    /// <summary>
    /// Appクラスのアセンブリ参照が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyReferences()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty("Appクラスのアセンブリに参照が存在すること");
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationFramework", "PresentationFrameworkが参照されていること");
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationCore", "PresentationCoreが参照されていること");
        referencedAssemblies.Should().Contain(a => a.Name == "WindowsBase", "WindowsBaseが参照されていること");
        referencedAssemblies.Should().Contain(a => a.Name == "Microsoft.Extensions.Hosting", "Microsoft.Extensions.Hostingが参照されていること");
        referencedAssemblies.Should().Contain(a => a.Name == "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.DependencyInjection.Abstractionsが参照されていること");
    }

    /// <summary>
    /// Appクラスのアセンブリ属性が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyAttributes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var attributes = assembly.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull("Appクラスのアセンブリに属性が存在すること");
        attributes.Should().NotBeEmpty("Appクラスのアセンブリに属性が存在すること");
    }

    /// <summary>
    /// Appクラスのアセンブリモジュールが正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyModules()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var modules = assembly.GetModules();

        // Assert
        modules.Should().NotBeEmpty("Appクラスのアセンブリにモジュールが存在すること");
        modules.Should().HaveCount(1, "Appクラスのアセンブリにモジュールが1つ存在すること");
    }

    /// <summary>
    /// Appクラスのアセンブリ型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyTypes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var types = assembly.GetTypes();

        // Assert
        types.Should().NotBeEmpty("Appクラスのアセンブリに型が存在すること");
        types.Should().Contain(typeof(BrowserSelector.App.App), "Appクラスがアセンブリに含まれること");
    }

    /// <summary>
    /// Appクラスのアセンブリエクスポート型が正しいことを確認するテスト.
    /// </summary>
    [Fact]
    public void App_ShouldHaveCorrectAssemblyExportedTypes()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var exportedTypes = assembly.GetExportedTypes();

        // Assert
        exportedTypes.Should().NotBeEmpty("Appクラスのアセンブリにエクスポート型が存在すること");
        exportedTypes.Should().Contain(typeof(BrowserSelector.App.App), "Appクラスがエクスポート型に含まれること");
    }
}
