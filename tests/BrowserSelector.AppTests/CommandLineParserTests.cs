// <copyright file="CommandLineParserTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.App.CommandLine;
using Xunit;

namespace BrowserSelector.AppTests;

/// <summary>
/// <see cref="CommandLineParser"/> のテスト（Phase D）.
/// </summary>
public class CommandLineParserTests
{
    [Fact]
    public void Parse_NoArgs_ReturnsDefaults()
    {
        CommandLineOptions options = CommandLineParser.Parse([]);

        Assert.Null(options.Delay);
        Assert.Null(options.BrowserId);
        Assert.False(options.Silent);
        Assert.False(options.AutoLaunch);
        Assert.False(options.ShowHelp);
        Assert.False(options.ShowVersion);
        Assert.False(options.TestMode);
        Assert.Null(options.Url);
        Assert.False(options.HasErrors);
    }

    [Theory]
    [InlineData("-d")]
    [InlineData("--delay")]
    public void Parse_Delay_SetsDelay(string flag)
    {
        CommandLineOptions options = CommandLineParser.Parse([flag, "10"]);

        Assert.Equal(10, options.Delay);
        Assert.False(options.HasErrors);
    }

    [Fact]
    public void Parse_DelayZero_IsValid()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-d", "0"]);

        Assert.Equal(0, options.Delay);
        Assert.False(options.HasErrors);
    }

    [Fact]
    public void Parse_DelayInvalidValue_RecordsError()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-d", "not-a-number"]);

        Assert.Null(options.Delay);
        Assert.True(options.HasErrors);
        Assert.Contains("-d", options.UnrecognizedArguments);
    }

    [Fact]
    public void Parse_DelayNegativeValue_RecordsError()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-d", "-5"]);

        Assert.Null(options.Delay);
        Assert.True(options.HasErrors);
    }

    [Fact]
    public void Parse_DelayMissingValue_RecordsError()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-d"]);

        Assert.True(options.HasErrors);
    }

    [Theory]
    [InlineData("-b")]
    [InlineData("--browser")]
    public void Parse_Browser_SetsBrowserId(string flag)
    {
        Guid id = Guid.NewGuid();
        CommandLineOptions options = CommandLineParser.Parse([flag, id.ToString()]);

        Assert.Equal(id, options.BrowserId);
        Assert.False(options.HasErrors);
    }

    [Fact]
    public void Parse_BrowserInvalidGuid_RecordsError()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-b", "not-a-guid"]);

        Assert.Null(options.BrowserId);
        Assert.True(options.HasErrors);
    }

    [Fact]
    public void Parse_Silent_SetsSilent()
    {
        CommandLineOptions options = CommandLineParser.Parse(["--silent"]);

        Assert.True(options.Silent);
    }

    [Fact]
    public void Parse_AutoLaunch_SetsDelayToZero()
    {
        CommandLineOptions options = CommandLineParser.Parse(["--auto-launch"]);

        Assert.True(options.AutoLaunch);
        Assert.Equal(0, options.Delay);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public void Parse_Help_SetsShowHelp(string flag)
    {
        CommandLineOptions options = CommandLineParser.Parse([flag]);

        Assert.True(options.ShowHelp);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    public void Parse_Version_SetsShowVersion(string flag)
    {
        CommandLineOptions options = CommandLineParser.Parse([flag]);

        Assert.True(options.ShowVersion);
    }

    [Fact]
    public void Parse_TestMode_SetsTestMode()
    {
        CommandLineOptions options = CommandLineParser.Parse(["--test-mode"]);

        Assert.True(options.TestMode);
    }

    [Fact]
    public void Parse_UnknownFlag_RecordsError()
    {
        CommandLineOptions options = CommandLineParser.Parse(["--not-a-real-option"]);

        Assert.True(options.HasErrors);
        Assert.Contains("--not-a-real-option", options.UnrecognizedArguments);
    }

    [Fact]
    public void Parse_PlainArgument_IsTreatedAsUrl()
    {
        CommandLineOptions options = CommandLineParser.Parse(["https://example.com/path"]);

        Assert.Equal("https://example.com/path", options.Url);
        Assert.False(options.HasErrors);
    }

    [Fact]
    public void Parse_UrlExceedingMaxLength_IsTruncated()
    {
        string longUrl = "https://example.com/" + new string('a', 9000);
        CommandLineOptions options = CommandLineParser.Parse([longUrl]);

        Assert.NotNull(options.Url);
        Assert.True(options.Url!.Length <= CommandLineOptions.MaxUrlLength);
    }

    [Fact]
    public void Parse_UrlWithPercentEncoding_IsDecoded()
    {
        CommandLineOptions options = CommandLineParser.Parse(["https://example.com/%E3%81%82"]);

        Assert.Equal("https://example.com/あ", options.Url);
    }

    [Fact]
    public void Parse_CombinedOptionsAndUrl_ParsesAllCorrectly()
    {
        CommandLineOptions options = CommandLineParser.Parse(["-d", "3", "--silent", "https://example.com"]);

        Assert.Equal(3, options.Delay);
        Assert.True(options.Silent);
        Assert.Equal("https://example.com", options.Url);
        Assert.False(options.HasErrors);
    }

    [Fact]
    public void HelpText_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(CommandLineParser.HelpText));
    }
}
