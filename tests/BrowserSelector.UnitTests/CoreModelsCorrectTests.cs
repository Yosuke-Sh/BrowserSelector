using BrowserSelector.Core.Models;
using FluentAssertions;

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
        Browser browser = new();

        // Assert
        _ = browser.Name.Should().BeEmpty();
        _ = browser.ExecutablePath.Should().BeEmpty();
        _ = browser.IconPath.Should().BeEmpty();
        _ = browser.Arguments.Should().BeEmpty();
        _ = browser.IsDefault.Should().BeFalse();
        _ = browser.IsEnabled.Should().BeTrue();
        _ = browser.DisplayOrder.Should().Be(0);
        _ = browser.LastUsed.Should().Be(DateTime.MinValue);
        _ = browser.UseCount.Should().Be(0);
        _ = browser.Id.Should().NotBe(Guid.Empty);
        _ = browser.Type.Should().Be(BrowserType.Custom);
    }

    [Fact]
    public void Browser_Properties_ShouldBeSettable()
    {
        // Arrange
        Browser browser = new();
        string testName = "Test Browser";
        string testPath = @"C:\Test\browser.exe";
        string testIconPath = @"C:\Icons\browser.ico";
        string testArguments = "--test-arg";

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
        _ = browser.Name.Should().Be(testName);
        _ = browser.ExecutablePath.Should().Be(testPath);
        _ = browser.IconPath.Should().Be(testIconPath);
        _ = browser.Arguments.Should().Be(testArguments);
        _ = browser.IsDefault.Should().BeTrue();
        _ = browser.IsEnabled.Should().BeFalse();
        _ = browser.DisplayOrder.Should().Be(5);
        _ = browser.Type.Should().Be(BrowserType.Chrome);
    }

    [Fact]
    public void Browser_IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Valid Browser",
            ExecutablePath = @"C:\Valid\browser.exe"
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Browser_IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = @"C:\Valid\browser.exe"
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_IsValid_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Valid Browser",
            ExecutablePath = ""
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Browser_DisplayName_WithValidName_ShouldReturnName()
    {
        // Arrange
        Browser browser = new() { Name = "Test Browser" };

        // Act & Assert
        _ = browser.DisplayName.Should().Be("Test Browser");
    }

    [Fact]
    public void Browser_DisplayName_WithEmptyName_ShouldReturnUnknown()
    {
        // Arrange
        Browser browser = new() { Name = "" };

        // Act & Assert
        _ = browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncrementCountAndUpdateLastUsed()
    {
        // Arrange
        Browser browser = new();
        int initialCount = browser.UseCount;
        DateTime initialLastUsed = browser.LastUsed;

        // Act
        browser.IncrementUseCount();

        // Assert
        _ = browser.UseCount.Should().Be(initialCount + 1);
        _ = browser.LastUsed.Should().BeAfter(initialLastUsed);
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        Browser originalBrowser = new()
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
        Browser clonedBrowser = originalBrowser.Clone();

        // Assert
        _ = clonedBrowser.Should().NotBeSameAs(originalBrowser);
        _ = clonedBrowser.Id.Should().NotBe(originalBrowser.Id);
        _ = clonedBrowser.Name.Should().Be(originalBrowser.Name);
        _ = clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        _ = clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        _ = clonedBrowser.Arguments.Should().Be(originalBrowser.Arguments);
        _ = clonedBrowser.IsDefault.Should().BeFalse(); // 複製時はfalse
        _ = clonedBrowser.IsEnabled.Should().Be(originalBrowser.IsEnabled);
        _ = clonedBrowser.DisplayOrder.Should().Be(originalBrowser.DisplayOrder);
        _ = clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    #endregion

    #region BrowserType Tests

    [Fact]
    public void BrowserType_Values_ShouldBeDefined()
    {
        // Act & Assert
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Custom);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Chrome);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Firefox);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Edge);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Safari);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Opera);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.InternetExplorer);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Brave);
        _ = Enum.GetValues<BrowserType>().Should().Contain(BrowserType.Vivaldi);
    }

    #endregion

    #region CustomLanguageFile Tests

    [Fact]
    public void CustomLanguageFile_Constructor_ShouldSetDefaultValues()
    {
        // Act
        CustomLanguageFile languageFile = new();

        // Assert
        _ = languageFile.CultureCode.Should().BeEmpty();
        _ = languageFile.DisplayName.Should().BeEmpty();
        _ = languageFile.Resources.Should().NotBeNull();
        _ = languageFile.Resources.Should().BeEmpty();
        _ = languageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = languageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = languageFile.Version.Should().Be("1.0");
        _ = languageFile.Description.Should().BeNull();
        _ = languageFile.Author.Should().BeNull();
    }

    [Fact]
    public void CustomLanguageFile_Properties_ShouldBeSettable()
    {
        // Arrange
        CustomLanguageFile languageFile = new();
        string testCultureCode = "zh-CN";
        string testDisplayName = "中文 (简体)";
        string testVersion = "2.0";
        string testDescription = "Test Description";
        string testAuthor = "Test Author";
        Dictionary<string, string> testResources = new()
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
        _ = languageFile.CultureCode.Should().Be(testCultureCode);
        _ = languageFile.DisplayName.Should().Be(testDisplayName);
        _ = languageFile.Version.Should().Be(testVersion);
        _ = languageFile.Description.Should().Be(testDescription);
        _ = languageFile.Author.Should().Be(testAuthor);
        _ = languageFile.Resources.Should().BeEquivalentTo(testResources);
    }

    [Fact]
    public void CustomLanguageFile_Resources_ShouldBeMutable()
    {
        // Arrange
        CustomLanguageFile languageFile = new();

        // Act
        languageFile.Resources["key1"] = "value1";
        languageFile.Resources["key2"] = "value2";

        // Assert
        _ = languageFile.Resources.Should().HaveCount(2);
        _ = languageFile.Resources["key1"].Should().Be("value1");
        _ = languageFile.Resources["key2"].Should().Be("value2");
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public void Browser_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Browser with Special Chars: !@#$%^&*()",
            ExecutablePath = @"C:\Program Files\Browser with Spaces\browser.exe",
            Arguments = "--arg with spaces --another-arg"
        };

        // Act & Assert
        _ = browser.Name.Should().Be("Browser with Special Chars: !@#$%^&*()");
        _ = browser.ExecutablePath.Should().Be(@"C:\Program Files\Browser with Spaces\browser.exe");
        _ = browser.Arguments.Should().Be("--arg with spaces --another-arg");
        _ = browser.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Browser_WithVeryLongStrings_ShouldHandleCorrectly()
    {
        // Arrange
        string longString = new('A', 1000);
        Browser browser = new()
        {
            Name = longString,
            ExecutablePath = longString,
            Arguments = longString
        };

        // Act & Assert
        _ = browser.Name.Should().Be(longString);
        _ = browser.ExecutablePath.Should().Be(longString);
        _ = browser.Arguments.Should().Be(longString);
    }

    [Fact]
    public void CustomLanguageFile_WithManyResources_ShouldHandleCorrectly()
    {
        // Arrange
        CustomLanguageFile languageFile = new();
        Dictionary<string, string> resources = [];

        // Act
        for (int i = 0; i < 1000; i++)
        {
            resources[$"key{i}"] = $"value{i}";
        }
        languageFile.Resources = resources;

        // Assert
        _ = languageFile.Resources.Should().HaveCount(1000);
        _ = languageFile.Resources["key0"].Should().Be("value0");
        _ = languageFile.Resources["key999"].Should().Be("value999");
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void Browser_PropertyChanges_ShouldWorkCorrectly()
    {
        // Arrange
        Browser browser = new()
        {
            // Act & Assert
            Name = "Initial Name"
        };
        _ = browser.Name.Should().Be("Initial Name");

        browser.Name = "Updated Name";
        _ = browser.Name.Should().Be("Updated Name");

        browser.IsDefault = true;
        _ = browser.IsDefault.Should().BeTrue();

        browser.IsDefault = false;
        _ = browser.IsDefault.Should().BeFalse();

        browser.Type = BrowserType.Chrome;
        _ = browser.Type.Should().Be(BrowserType.Chrome);

        browser.Type = BrowserType.Firefox;
        _ = browser.Type.Should().Be(BrowserType.Firefox);
    }

    [Fact]
    public void CustomLanguageFile_PropertyChanges_ShouldWorkCorrectly()
    {
        // Arrange
        CustomLanguageFile languageFile = new()
        {
            // Act & Assert
            CultureCode = "en-US"
        };
        _ = languageFile.CultureCode.Should().Be("en-US");

        languageFile.CultureCode = "ja-JP";
        _ = languageFile.CultureCode.Should().Be("ja-JP");

        languageFile.Version = "1.0";
        _ = languageFile.Version.Should().Be("1.0");

        languageFile.Version = "2.0";
        _ = languageFile.Version.Should().Be("2.0");
    }

    #endregion

    #region Equality and Comparison Tests

    [Fact]
    public void Browser_WithSameName_ShouldBeComparable()
    {
        // Arrange
        Browser browser1 = new() { Name = "Same Browser" };
        Browser browser2 = new() { Name = "Same Browser" };

        // Act & Assert
        _ = browser1.Name.Should().Be(browser2.Name);
        _ = browser1.Id.Should().NotBe(browser2.Id); // IDは異なる
    }

    [Fact]
    public void CustomLanguageFile_WithSameCultureCode_ShouldBeComparable()
    {
        // Arrange
        CustomLanguageFile file1 = new() { CultureCode = "zh-CN" };
        CustomLanguageFile file2 = new() { CultureCode = "zh-CN" };

        // Act & Assert
        _ = file1.CultureCode.Should().Be(file2.CultureCode);
    }

    #endregion
}
