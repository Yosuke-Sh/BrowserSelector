// <copyright file="ArgumentParserTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Updater;
using FluentAssertions;

namespace BrowserSelector.UpdaterTests;

/// <summary>
/// <see cref="ArgumentParser"/> のテスト（Phase H-5）.
/// </summary>
public class ArgumentParserTests
{
    [Fact]
    public void TryParse_AllArguments_ParsesEveryValue()
    {
        string[] args =
        [
            "--mode", "apply-zip",
            "--pid", "1234",
            "--source", @"C:\src\extracted",
            "--target", @"C:\app",
            "--backup", @"C:\backup\1",
            "--exe", "Custom.exe",
            "--log", @"C:\logs\updater.log",
            "--relaunch-args", "--silent",
            "--no-relaunch",
        ];

        bool result = ArgumentParser.TryParse(args, out UpdaterOptions? options, out string? error);

        result.Should().BeTrue();
        error.Should().BeNull();
        options.Should().NotBeNull();
        options!.Mode.Should().Be("apply-zip");
        options.ProcessId.Should().Be(1234);
        options.Source.Should().Be(@"C:\src\extracted");
        options.Target.Should().Be(@"C:\app");
        options.Backup.Should().Be(@"C:\backup\1");
        options.ExecutableName.Should().Be("Custom.exe");
        options.LogPath.Should().Be(@"C:\logs\updater.log");
        options.RelaunchArguments.Should().Be("--silent");
        options.NoRelaunch.Should().BeTrue();
    }

    [Fact]
    public void TryParse_OnlyRequiredArguments_AppliesDefaults()
    {
        string[] args = MinimalArgs();

        bool result = ArgumentParser.TryParse(args, out UpdaterOptions? options, out string? error);

        result.Should().BeTrue();
        error.Should().BeNull();
        options!.ExecutableName.Should().Be("BrowserSelector.exe");
        options.LogPath.Should().NotBeNullOrWhiteSpace();
        options.RelaunchArguments.Should().BeEmpty();
        options.NoRelaunch.Should().BeFalse();
    }

    [Theory]
    [InlineData("--mode")]
    [InlineData("--pid")]
    [InlineData("--source")]
    [InlineData("--target")]
    [InlineData("--backup")]
    public void TryParse_MissingRequiredArgument_Fails(string missing)
    {
        List<string> args = [];
        string[] minimal = MinimalArgs();

        for (int i = 0; i < minimal.Length; i += 2)
        {
            if (minimal[i] == missing)
            {
                continue;
            }

            args.Add(minimal[i]);
            args.Add(minimal[i + 1]);
        }

        bool result = ArgumentParser.TryParse([.. args], out UpdaterOptions? options, out string? error);

        result.Should().BeFalse();
        options.Should().BeNull();
        error.Should().Contain(missing);
    }

    [Fact]
    public void TryParse_UnknownArgument_Fails()
    {
        string[] args = [.. MinimalArgs(), "--unknown", "x"];

        bool result = ArgumentParser.TryParse(args, out UpdaterOptions? options, out string? error);

        result.Should().BeFalse();
        options.Should().BeNull();
        error.Should().Contain("--unknown");
    }

    [Fact]
    public void TryParse_ArgumentWithoutValue_Fails()
    {
        string[] args = ["--mode", "apply-zip", "--pid"];

        bool result = ArgumentParser.TryParse(args, out UpdaterOptions? options, out string? error);

        result.Should().BeFalse();
        options.Should().BeNull();
        error.Should().Contain("--pid");
    }

    [Fact]
    public void TryParse_PathWithSpaces_IsPreservedAsSingleValue()
    {
        string[] args =
        [
            "--mode", "apply-zip",
            "--pid", "42",
            "--source", @"C:\Program Files\Browser Selector\extracted",
            "--target", @"C:\Program Files\Browser Selector",
            "--backup", @"C:\Users\Test User\backup 1",
        ];

        bool result = ArgumentParser.TryParse(args, out UpdaterOptions? options, out _);

        result.Should().BeTrue();
        options!.Source.Should().Be(@"C:\Program Files\Browser Selector\extracted");
        options.Target.Should().Be(@"C:\Program Files\Browser Selector");
        options.Backup.Should().Be(@"C:\Users\Test User\backup 1");
    }

    [Fact]
    public void TryParse_PositionalArgument_Fails()
    {
        string[] args = ["apply-zip", "--pid", "1"];

        bool result = ArgumentParser.TryParse(args, out _, out string? error);

        result.Should().BeFalse();
        error.Should().Contain("位置引数");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public void TryParse_InvalidPid_Fails(string pid)
    {
        string[] args = ["--mode", "apply-zip", "--pid", pid, "--source", "s", "--target", "t", "--backup", "b"];

        bool result = ArgumentParser.TryParse(args, out _, out string? error);

        result.Should().BeFalse();
        error.Should().Contain("--pid");
    }

    [Fact]
    public void TryParse_UnsupportedMode_Fails()
    {
        string[] args = ["--mode", "rollback", "--pid", "1", "--source", "s", "--target", "t", "--backup", "b"];

        bool result = ArgumentParser.TryParse(args, out _, out string? error);

        result.Should().BeFalse();
        error.Should().Contain("rollback");
    }

    [Fact]
    public void TryParse_DuplicateArgument_Fails()
    {
        string[] args = [.. MinimalArgs(), "--pid", "999"];

        bool result = ArgumentParser.TryParse(args, out _, out string? error);

        result.Should().BeFalse();
        error.Should().Contain("重複");
    }

    private static string[] MinimalArgs() =>
    [
        "--mode", "apply-zip",
        "--pid", "1234",
        "--source", @"C:\src",
        "--target", @"C:\app",
        "--backup", @"C:\backup",
    ];
}
