using FluentAssertions;
using System.Reflection;
using System.Windows;

namespace BrowserSelector.AppTests;

/// <summary>
/// Appクラス専用のテスト
/// WPFアプリケーションの基本機能をテスト.
/// </summary>
public class AppClassTests
{
    [Fact]
    public void App_ShouldInheritFromApplication()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.Should().NotBeNull();
        appType.BaseType.Should().Be(typeof(Application));
    }

    [Fact]
    public void App_ShouldHaveOnStartupMethod()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onStartupMethod.Should().NotBeNull();
        onStartupMethod.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void App_ShouldHaveOnExitMethod()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        onExitMethod.Should().NotBeNull();
        onExitMethod.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void App_ShouldHaveInitializeComponentMethod()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var initializeComponentMethod = appType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        initializeComponentMethod.Should().NotBeNull();
        initializeComponentMethod.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void App_ShouldHaveMainWindowProperty()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var mainWindowProperty = appType.GetProperty("MainWindow", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        mainWindowProperty.Should().NotBeNull();
        mainWindowProperty.PropertyType.Should().Be(typeof(Window));
    }

    [Fact]
    public void App_ShouldHaveStartupUriProperty()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var startupUriProperty = appType.GetProperty("StartupUri", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        startupUriProperty.Should().NotBeNull();
        startupUriProperty.PropertyType.Should().Be(typeof(Uri));
    }

    [Fact]
    public void App_ShouldHaveShutdownModeProperty()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var shutdownModeProperty = appType.GetProperty("ShutdownMode", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        shutdownModeProperty.Should().NotBeNull();
        shutdownModeProperty.PropertyType.Should().Be(typeof(ShutdownMode));
    }

    [Fact]
    public void App_ShouldHaveAssemblyInfo()
    {
        // Arrange & Act
        var assembly = typeof(BrowserSelector.App.App).Assembly;
        var assemblyName = assembly.GetName();

        // Assert
        assembly.Should().NotBeNull();
        assemblyName.Name.Should().Be("BrowserSelector");
        assemblyName.Version.Should().NotBeNull();
    }

    [Fact]
    public void App_ShouldHaveValidAssemblyAttributes()
    {
        // Arrange & Act
        var assembly = typeof(BrowserSelector.App.App).Assembly;
        var attributes = assembly.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void App_ShouldHaveValidNamespace()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.Namespace.Should().Be("BrowserSelector.App");
    }

    [Fact]
    public void App_ShouldBePublicClass()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.IsPublic.Should().BeTrue();
        appType.IsClass.Should().BeTrue();
    }

    [Fact]
    public void App_ShouldHaveDefaultConstructor()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var constructors = appType.GetConstructors();

        // Assert
        constructors.Should().NotBeEmpty();
        constructors.Should().Contain(c => c.GetParameters().Length == 0);
    }

    [Fact]
    public void App_ShouldHaveRequiredUsingStatements()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty();
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationFramework");
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationCore");
    }

    [Fact]
    public void App_ShouldHavePrivateFields()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var fields = appType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        fields.Should().NotBeEmpty();
        fields.Should().Contain(f => f.Name == "_host");
        fields.Should().Contain(f => f.Name == "_logService");
    }

    [Fact]
    public void App_ShouldHaveCorrectClassModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);

        // Assert
        appType.IsPublic.Should().BeTrue();
        appType.IsClass.Should().BeTrue();
        appType.IsAbstract.Should().BeFalse();
        appType.IsSealed.Should().BeFalse();
    }

    [Fact]
    public void App_ShouldHaveCorrectConstructorModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var constructors = appType.GetConstructors();
        var defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        // Assert
        defaultConstructor.Should().NotBeNull();
        defaultConstructor.IsPublic.Should().BeTrue();
        defaultConstructor.IsStatic.Should().BeFalse();
    }

    [Fact]
    public void App_ShouldHaveCorrectMethodModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var onStartupMethod = appType.GetMethod("OnStartup", BindingFlags.NonPublic | BindingFlags.Instance);
        var onExitMethod = appType.GetMethod("OnExit", BindingFlags.NonPublic | BindingFlags.Instance);
        var initializeComponentMethod = appType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        onStartupMethod.Should().NotBeNull();
        onStartupMethod.IsPublic.Should().BeFalse();
        onStartupMethod.IsStatic.Should().BeFalse();
        onStartupMethod.IsVirtual.Should().BeTrue(); // Override method

        onExitMethod.Should().NotBeNull();
        onExitMethod.IsPublic.Should().BeFalse();
        onExitMethod.IsStatic.Should().BeFalse();
        onExitMethod.IsVirtual.Should().BeTrue(); // Override method

        initializeComponentMethod.Should().NotBeNull();
        initializeComponentMethod.IsPublic.Should().BeTrue();
        initializeComponentMethod.IsStatic.Should().BeFalse();
    }

    [Fact]
    public void App_ShouldHaveCorrectPropertyModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var mainWindowProperty = appType.GetProperty("MainWindow", BindingFlags.Public | BindingFlags.Instance);
        var startupUriProperty = appType.GetProperty("StartupUri", BindingFlags.Public | BindingFlags.Instance);
        var shutdownModeProperty = appType.GetProperty("ShutdownMode", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        mainWindowProperty.Should().NotBeNull();
        mainWindowProperty.CanRead.Should().BeTrue();
        mainWindowProperty.CanWrite.Should().BeTrue();

        startupUriProperty.Should().NotBeNull();
        startupUriProperty.CanRead.Should().BeTrue();
        startupUriProperty.CanWrite.Should().BeTrue();

        shutdownModeProperty.Should().NotBeNull();
        shutdownModeProperty.CanRead.Should().BeTrue();
        shutdownModeProperty.CanWrite.Should().BeTrue();
    }

    [Fact]
    public void App_ShouldHaveCorrectFieldModifiers()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var hostField = appType.GetField("_host", BindingFlags.NonPublic | BindingFlags.Instance);
        var logServiceField = appType.GetField("_logService", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        hostField.Should().NotBeNull();
        hostField.IsPublic.Should().BeFalse();
        hostField.IsStatic.Should().BeFalse();

        logServiceField.Should().NotBeNull();
        logServiceField.IsPublic.Should().BeFalse();
        logServiceField.IsStatic.Should().BeFalse();
    }

    [Fact]
    public void App_ShouldHaveCorrectAssemblyReferences()
    {
        // Arrange & Act
        var appType = typeof(BrowserSelector.App.App);
        var assembly = appType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty();

        // WPF関連のアセンブリが参照されていることを確認
        var wpfAssemblies = referencedAssemblies.Where(a =>
            a.Name == "PresentationFramework" ||
            a.Name == "PresentationCore" ||
            a.Name == "WindowsBase");

        wpfAssemblies.Should().HaveCount(3);

        // Microsoft.Extensions関連のアセンブリが参照されていることを確認
        var microsoftAssemblies = referencedAssemblies.Where(a =>
            a.Name.StartsWith("Microsoft.Extensions"));

        microsoftAssemblies.Should().NotBeEmpty();
    }
}
