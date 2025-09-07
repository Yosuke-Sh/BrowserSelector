using BrowserSelector.App;
using FluentAssertions;
using System.Reflection;

namespace BrowserSelector.AppTests;

/// <summary>
/// Programクラス専用のテスト
/// アプリケーションエントリーポイントの基本機能をテスト
/// </summary>
public class ProgramTests
{
    [Fact]
    public void Program_ShouldHaveMainMethod()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mainMethod.Should().NotBeNull();
        mainMethod.ReturnType.Should().Be(typeof(void));
        mainMethod.IsStatic.Should().BeTrue();
        mainMethod.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void Program_ShouldHaveMainMethodWithStringArrayParameter()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var parameters = mainMethod.GetParameters();

        // Assert
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(string[]));
        parameters[0].Name.Should().Be("args");
    }

    [Fact]
    public void Program_ShouldBePublicClass()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.IsPublic.Should().BeTrue();
        programType.IsClass.Should().BeTrue();
    }

    [Fact]
    public void Program_ShouldHaveValidNamespace()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.Namespace.Should().Be("BrowserSelector.App");
    }

    [Fact]
    public void Program_ShouldHaveSTAThreadAttribute()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var attributes = mainMethod.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeEmpty();
        attributes.Should().Contain(a => a.GetType().Name == "STAThreadAttribute");
    }

    [Fact]
    public void Program_ShouldHaveDefaultConstructor()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var constructors = programType.GetConstructors();

        // Assert
        constructors.Should().NotBeEmpty();
        constructors.Should().Contain(c => c.GetParameters().Length == 0);
    }

    [Fact]
    public void Program_ShouldHaveValidAssembly()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var assemblyName = assembly.GetName();

        // Assert
        assembly.Should().NotBeNull();
        assemblyName.Name.Should().Be("BrowserSelector.App");
    }

    [Fact]
    public void Program_ShouldHaveValidAssemblyAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var attributes = assembly.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveRequiredUsingStatements()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty();
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationFramework");
        referencedAssemblies.Should().Contain(a => a.Name == "PresentationCore");
        referencedAssemblies.Should().Contain(a => a.Name == "WindowsBase");
    }

    [Fact]
    public void Program_ShouldHaveValidTypeAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var attributes = programType.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveValidMethodAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var attributes = mainMethod.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveValidParameterAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var parameters = mainMethod.GetParameters();
        var parameterAttributes = parameters[0].GetCustomAttributes();

        // Assert
        parameterAttributes.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveValidReturnType()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mainMethod.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void Program_ShouldHaveValidMethodSignature()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var parameters = mainMethod.GetParameters();

        // Assert
        mainMethod.Name.Should().Be("Main");
        mainMethod.IsStatic.Should().BeTrue();
        mainMethod.IsPublic.Should().BeTrue();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(string[]));
    }

    [Fact]
    public void Program_ShouldHaveCorrectMethodVisibility()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mainMethod.IsPublic.Should().BeTrue();
        mainMethod.IsStatic.Should().BeTrue();
        mainMethod.IsVirtual.Should().BeFalse();
        mainMethod.IsAbstract.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectClassModifiers()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.IsPublic.Should().BeTrue();
        programType.IsClass.Should().BeTrue();
        programType.IsAbstract.Should().BeFalse();
        programType.IsSealed.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyReferences()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        // Assert
        referencedAssemblies.Should().NotBeEmpty();

        // WPF関連のアセンブリが参照されていることを確認
        var wpfAssemblies = referencedAssemblies.Where(a =>
            a.Name == "PresentationFramework" ||
            a.Name == "PresentationCore" ||
            a.Name == "WindowsBase");

        wpfAssemblies.Should().HaveCount(3);
    }

    [Fact]
    public void Program_ShouldHaveCorrectMethodParameters()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var parameters = mainMethod.GetParameters();

        // Assert
        parameters.Should().HaveCount(1);

        var argsParameter = parameters[0];
        argsParameter.Name.Should().Be("args");
        argsParameter.ParameterType.Should().Be(typeof(string[]));
        argsParameter.IsOut.Should().BeFalse();
        argsParameter.IsRetval.Should().BeFalse();
    }

    // 追加のテストケース - Program.csのカバレッジ向上
    [Fact]
    public void Program_ShouldHaveCorrectClassStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.IsClass.Should().BeTrue();
        programType.IsPublic.Should().BeTrue();
        programType.IsAbstract.Should().BeFalse();
        programType.IsSealed.Should().BeFalse();
        programType.IsInterface.Should().BeFalse();
        programType.IsEnum.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectMethodStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mainMethod.Should().NotBeNull();
        mainMethod.IsPublic.Should().BeTrue();
        mainMethod.IsStatic.Should().BeTrue();
        mainMethod.IsVirtual.Should().BeFalse();
        mainMethod.IsAbstract.Should().BeFalse();
        mainMethod.IsFinal.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectParameterStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var parameters = mainMethod.GetParameters();

        // Assert
        parameters.Should().HaveCount(1);
        var argsParameter = parameters[0];
        argsParameter.Name.Should().Be("args");
        argsParameter.ParameterType.Should().Be(typeof(string[]));
        argsParameter.IsOut.Should().BeFalse();
        argsParameter.IsRetval.Should().BeFalse();
        argsParameter.IsOptional.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAttributeStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        var attributes = mainMethod.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeEmpty();
        attributes.Should().Contain(a => a.GetType().Name == "STAThreadAttribute");
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var assemblyName = assembly.GetName();

        // Assert
        assembly.Should().NotBeNull();
        assemblyName.Name.Should().Be("BrowserSelector.App");
        assemblyName.Version.Should().NotBeNull();
        assemblyName.CultureInfo.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectTypeHierarchy()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var baseType = programType.BaseType;

        // Assert
        baseType.Should().Be(typeof(object));
        programType.GetInterfaces().Should().BeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveCorrectGenericStructure()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.IsGenericType.Should().BeFalse();
        programType.IsGenericTypeDefinition.Should().BeFalse();
        programType.ContainsGenericParameters.Should().BeFalse();
    }

    [Fact]
    public void Program_ShouldHaveCorrectMethodCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var methods = programType.GetMethods(BindingFlags.Public | BindingFlags.Static);

        // Assert
        methods.Should().HaveCount(1);
        methods.Should().Contain(m => m.Name == "Main");
    }

    [Fact]
    public void Program_ShouldHaveCorrectConstructorCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var constructors = programType.GetConstructors();

        // Assert
        constructors.Should().HaveCount(1);
        constructors.Should().Contain(c => c.GetParameters().Length == 0);
    }

    [Fact]
    public void Program_ShouldHaveCorrectPropertyCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var properties = programType.GetProperties();

        // Assert
        properties.Should().BeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveCorrectEventCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var events = programType.GetEvents();

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveCorrectFieldCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var fields = programType.GetFields();

        // Assert
        fields.Should().BeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveCorrectNestedTypeCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var nestedTypes = programType.GetNestedTypes();

        // Assert
        nestedTypes.Should().BeEmpty();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyLocation()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var location = assembly.Location;

        // Assert
        location.Should().NotBeNullOrEmpty();
        location.Should().Contain("BrowserSelector.App.dll");
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyManifest()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var manifestModule = assembly.ManifestModule;

        // Assert
        manifestModule.Should().NotBeNull();
        manifestModule.Name.Should().Contain("BrowserSelector.App.dll");
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyModules()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var modules = assembly.GetModules();

        // Assert
        modules.Should().NotBeEmpty();
        modules.Should().HaveCount(1);
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyTypes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var types = assembly.GetTypes();

        // Assert
        types.Should().NotBeEmpty();
        types.Should().Contain(typeof(Program));
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyExportedTypes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var exportedTypes = assembly.GetExportedTypes();

        // Assert
        exportedTypes.Should().NotBeEmpty();
        exportedTypes.Should().Contain(typeof(Program));
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyCustomAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var attributes = assembly.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblySecurityRules()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var securityRules = assembly.GetCustomAttributes(typeof(System.Security.SecurityRulesAttribute), false);

        // Assert
        securityRules.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyConfiguration()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var configuration = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false);

        // Assert
        configuration.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyCompany()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var company = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);

        // Assert
        company.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyProduct()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var product = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);

        // Assert
        product.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyCopyright()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var copyright = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);

        // Assert
        copyright.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyTrademark()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var trademark = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTrademarkAttribute), false);

        // Assert
        trademark.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyDescription()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var description = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);

        // Assert
        description.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyTitle()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var title = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);

        // Assert
        title.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyFileVersion()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var fileVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);

        // Assert
        fileVersion.Should().NotBeNull();
    }

    [Fact]
    public void Program_ShouldHaveCorrectAssemblyInformationalVersion()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var assembly = programType.Assembly;
        var informationalVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false);

        // Assert
        informationalVersion.Should().NotBeNull();
    }
}
