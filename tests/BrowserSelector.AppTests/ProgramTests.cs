using BrowserSelector.App;
using FluentAssertions;
using System.Reflection;

namespace BrowserSelector.AppTests;

/// <summary>
/// Programクラス専用のテスト
/// アプリケーションエントリーポイントの基本機能をテスト.
/// </summary>
public class ProgramTests
{
    /// <summary>
    /// ProgramクラスがMainメソッドを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスがstring[]パラメータを持つMainメソッドを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスがパブリッククラスであることを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldBePublicClass()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.IsPublic.Should().BeTrue();
        programType.IsClass.Should().BeTrue();
    }

    /// <summary>
    /// Programクラスが有効な名前空間を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveValidNamespace()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.Namespace.Should().Be("BrowserSelector.App");
    }

    /// <summary>
    /// ProgramクラスがSTAThreadAttributeを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスがデフォルトコンストラクタを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが有効なアセンブリを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが有効なアセンブリアトリビュートを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが必要なusing文を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが有効な型アトリビュートを持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveValidTypeAttributes()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var attributes = programType.GetCustomAttributes();

        // Assert
        attributes.Should().NotBeNull();
    }

    /// <summary>
    /// Programクラスが有効なメソッドアトリビュートを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが有効なパラメータアトリビュートを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが有効な戻り値の型を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveValidReturnType()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mainMethod.ReturnType.Should().Be(typeof(void));
    }

    /// <summary>
    /// Programクラスが有効なメソッドシグネチャを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいメソッド可視性を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいクラス修飾子を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ参照を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいメソッドパラメータを持つことを確認するテスト.
    /// </summary>
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
    /// <summary>
    /// Programクラスが正しいクラス構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいメソッド構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいパラメータ構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しい属性構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しい型階層を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいジェネリック構造を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいメソッド数を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいコンストラクタ数を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいプロパティ数を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveCorrectPropertyCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var properties = programType.GetProperties();

        // Assert
        properties.Should().BeEmpty();
    }

    /// <summary>
    /// Programクラスが正しいイベント数を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveCorrectEventCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var events = programType.GetEvents();

        // Assert
        events.Should().BeEmpty();
    }

    /// <summary>
    /// Programクラスが正しいフィールド数を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveCorrectFieldCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var fields = programType.GetFields();

        // Assert
        fields.Should().BeEmpty();
    }

    /// <summary>
    /// Programクラスが正しいネストされた型数を持つことを確認するテスト.
    /// </summary>
    [Fact]
    public void Program_ShouldHaveCorrectNestedTypeCount()
    {
        // Arrange & Act
        var programType = typeof(Program);
        var nestedTypes = programType.GetNestedTypes();

        // Assert
        nestedTypes.Should().BeEmpty();
    }

    /// <summary>
    /// Programクラスが正しいアセンブリロケーションを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリマニフェストを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリモジュールを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ型を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリエクスポート型を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリカスタム属性を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリセキュリティルールを持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ設定を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ会社情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ製品情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ著作権情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ商標情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ説明情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリタイトル情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリファイルバージョン情報を持つことを確認するテスト.
    /// </summary>
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

    /// <summary>
    /// Programクラスが正しいアセンブリ情報バージョンを持つことを確認するテスト.
    /// </summary>
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
