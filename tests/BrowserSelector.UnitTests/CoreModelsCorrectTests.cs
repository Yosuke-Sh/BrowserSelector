using BrowserSelector.Core.Models;
using FluentAssertions;
using Xunit;

namespace BrowserSelector.UnitTests;

/// <summary>
/// Coreプロジェクトのモデルクラスの正しいテスト
/// </summary>
public class CoreModelsCorrectTests
{
    #region Browser Tests

    [Fact]
    public void Browser_Constructor_ShouldSetDefaultValues()
    {
        // Act
        var browser = new Browser();

        // Assert
        browser.Name.Should().BeEmpty();
        browser.ExecutablePath.Should().BeEmpty();
        browser.IconPath.Should().BeEmpty();
        browser.Arguments.Should().BeEmpty();
        browser.IsDefault.Should().BeFalse();
        browser.IsEnabled.Should().BeTrue();
        browser.DisplayOrder.Should().Be(0);
        browser.LastUsed.Should().Be(DateTime.MinValue);
        browser.UseCount.Should().Be(0);
        browser.Id.Should().NotBe(Guid.Empty);
        browser.Type.Should().Be(BrowserType.Custom);
    }

    [Fact]
    public void Browser_Properties_ShouldBeSettable()
    {
        // Arrange
        var browser = new Browser();
        var testName = "Test Browser";
        var testPath = @"C:\Test\browser.exe";
        var testIconPath = @"C:\Icons\browser.ico";
        var testArguments = "--test-arg";

        // Act
        browser.Name = testName;
        browser.ExecutablePath = testPath;
        browser.IconPath = testIconPath;
        browser.Arguments = testArguments;
        browser.IsDefault = true;
        browser.IsEnabled = false;
        browser.DisplayOrder = 5;
        browser.Type = BrowserType.Chrome;

        // Assert
        browser.Name.Should().Be(testName);
        browser.ExecutablePath.Should().Be(testPath);
        browser.IconPath.Should().Be(testIconPath);
        browser.Arguments.Should().Be(testArguments);
        browser.IsDefault.Should().BeTrue();
        browser.IsEnabled.Should().BeFalse();
        browser.DisplayOrder.Should().Be(5);
        browser.Type.Should().Be(BrowserType.Chrome);
    }

    [Fact]
    public void Browser_IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Valid Browser",
            ExecutablePath = @"C:\Valid\browser.exe"
        };

        // Act & Assert
        browser.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Browser_IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = @"C:\Valid\browser.exe"
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_IsValid_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Valid Browser",
            ExecutablePath = ""
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_DisplayName_WithValidName_ShouldReturnName()
    {
        // Arrange
        var browser = new Browser { Name = "Test Browser" };

        // Act & Assert
        browser.DisplayName.Should().Be("Test Browser");
    }

    [Fact]
    public void Browser_DisplayName_WithEmptyName_ShouldReturnUnknown()
    {
        // Arrange
        var browser = new Browser { Name = "" };

        // Act & Assert
        browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncrementCountAndUpdateLastUsed()
    {
        // Arrange
        var browser = new Browser();
        var initialCount = browser.UseCount;
        var initialLastUsed = browser.LastUsed;

        // Act
        browser.IncrementUseCount();

        // Assert
        browser.UseCount.Should().Be(initialCount + 1);
        browser.LastUsed.Should().BeAfter(initialLastUsed);
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        var originalBrowser = new Browser
        {
            Name = "Original Browser",
            ExecutablePath = @"C:\Original\browser.exe",
            IconPath = @"C:\Icons\original.ico",
            Arguments = "--original",
            IsDefault = true,
            IsEnabled = true,
            DisplayOrder = 10,
            Type = BrowserType.Firefox
        };

        // Act
        var clonedBrowser = originalBrowser.Clone();

        // Assert
        clonedBrowser.Should().NotBeSameAs(originalBrowser);
        clonedBrowser.Id.Should().NotBe(originalBrowser.Id);
        clonedBrowser.Name.Should().Be(originalBrowser.Name);
        clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        clonedBrowser.Arguments.Should().Be(originalBrowser.Arguments);
        clonedBrowser.IsDefault.Should().BeFalse(); // 複製時はfalse
        clonedBrowser.IsEnabled.Should().Be(originalBrowser.IsEnabled);
        clonedBrowser.DisplayOrder.Should().Be(originalBrowser.DisplayOrder);
        clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    #endregion

    #region BrowserType Tests

    [Fact]
    public void BrowserType_Values_ShouldBeDefined()
    {
        // Act & Assert
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Custom);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Chrome);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Firefox);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Edge);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Safari);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Opera);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.InternetExplorer);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Brave);
        Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Vivaldi);
    }

    #endregion

    #region CustomLanguageFile Tests

    [Fact]
    public void CustomLanguageFile_Constructor_ShouldSetDefaultValues()
    {
        // Act
        var languageFile = new CustomLanguageFile();

        // Assert
        languageFile.CultureCode.Should().BeEmpty();
        languageFile.DisplayName.Should().BeEmpty();
        languageFile.Resources.Should().NotBeNull();
        languageFile.Resources.Should().BeEmpty();
        languageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        languageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        languageFile.Version.Should().Be("1.0");
        languageFile.Description.Should().BeNull();
        languageFile.Author.Should().BeNull();
    }

    [Fact]
    public void CustomLanguageFile_Properties_ShouldBeSettable()
    {
        // Arrange
        var languageFile = new CustomLanguageFile();
        var testCultureCode = "zh-CN";
        var testDisplayName = "中文 (简体)";
        var testVersion = "2.0";
        var testDescription = "Test Description";
        var testAuthor = "Test Author";
        var testResources = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        languageFile.CultureCode = testCultureCode;
        languageFile.DisplayName = testDisplayName;
        languageFile.Version = testVersion;
        languageFile.Description = testDescription;
        languageFile.Author = testAuthor;
        languageFile.Resources = testResources;

        // Assert
        languageFile.CultureCode.Should().Be(testCultureCode);
        languageFile.DisplayName.Should().Be(testDisplayName);
        languageFile.Version.Should().Be(testVersion);
        languageFile.Description.Should().Be(testDescription);
        languageFile.Author.Should().Be(testAuthor);
        languageFile.Resources.Should().BeEquivalentTo(testResources);
    }

    [Fact]
    public void CustomLanguageFile_Resources_ShouldBeMutable()
    {
        // Arrange
        var languageFile = new CustomLanguageFile();

        // Act
        languageFile.Resources["key1"] = "value1";
        languageFile.Resources["key2"] = "value2";

        // Assert
        languageFile.Resources.Should().HaveCount(2);
        languageFile.Resources["key1"].Should().Be("value1");
        languageFile.Resources["key2"].Should().Be("value2");
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public void Browser_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Browser with Special Chars: !@#$%^&*()",
            ExecutablePath = @"C:\Program Files\Browser with Spaces\browser.exe",
            Arguments = "--arg with spaces --another-arg"
        };

        // Act & Assert
        browser.Name.Should().Be("Browser with Special Chars: !@#$%^&*()");
        browser.ExecutablePath.Should().Be(@"C:\Program Files\Browser with Spaces\browser.exe");
        browser.Arguments.Should().Be("--arg with spaces --another-arg");
        browser.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Browser_WithVeryLongStrings_ShouldHandleCorrectly()
    {
        // Arrange
        var longString = new string('A', 1000);
        var browser = new Browser
        {
            Name = longString,
            ExecutablePath = longString,
            Arguments = longString
        };

        // Act & Assert
        browser.Name.Should().Be(longString);
        browser.ExecutablePath.Should().Be(longString);
        browser.Arguments.Should().Be(longString);
    }

    [Fact]
    public void CustomLanguageFile_WithManyResources_ShouldHandleCorrectly()
    {
        // Arrange
        var languageFile = new CustomLanguageFile();
        var resources = new Dictionary<string, string>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            resources[$"key{i}"] = $"value{i}";
        }
        languageFile.Resources = resources;

        // Assert
        languageFile.Resources.Should().HaveCount(1000);
        languageFile.Resources["key0"].Should().Be("value0");
        languageFile.Resources["key999"].Should().Be("value999");
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void Browser_PropertyChanges_ShouldWorkCorrectly()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        browser.Name = "Initial Name";
        browser.Name.Should().Be("Initial Name");

        browser.Name = "Updated Name";
        browser.Name.Should().Be("Updated Name");

        browser.IsDefault = true;
        browser.IsDefault.Should().BeTrue();

        browser.IsDefault = false;
        browser.IsDefault.Should().BeFalse();

        browser.Type = BrowserType.Chrome;
        browser.Type.Should().Be(BrowserType.Chrome);

        browser.Type = BrowserType.Firefox;
        browser.Type.Should().Be(BrowserType.Firefox);
    }

    [Fact]
    public void CustomLanguageFile_PropertyChanges_ShouldWorkCorrectly()
    {
        // Arrange
        var languageFile = new CustomLanguageFile();

        // Act & Assert
        languageFile.CultureCode = "en-US";
        languageFile.CultureCode.Should().Be("en-US");

        languageFile.CultureCode = "ja-JP";
        languageFile.CultureCode.Should().Be("ja-JP");

        languageFile.Version = "1.0";
        languageFile.Version.Should().Be("1.0");

        languageFile.Version = "2.0";
        languageFile.Version.Should().Be("2.0");
    }

    #endregion

    #region Equality and Comparison Tests

    [Fact]
    public void Browser_WithSameName_ShouldBeComparable()
    {
        // Arrange
        var browser1 = new Browser { Name = "Same Browser" };
        var browser2 = new Browser { Name = "Same Browser" };

        // Act & Assert
        browser1.Name.Should().Be(browser2.Name);
        browser1.Id.Should().NotBe(browser2.Id); // IDは異なる
    }

    [Fact]
    public void CustomLanguageFile_WithSameCultureCode_ShouldBeComparable()
    {
        // Arrange
        var file1 = new CustomLanguageFile { CultureCode = "zh-CN" };
        var file2 = new CustomLanguageFile { CultureCode = "zh-CN" };

        // Act & Assert
        file1.CultureCode.Should().Be(file2.CultureCode);
    }

    #endregion
}
