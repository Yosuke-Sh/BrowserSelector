// <copyright file="ApplyEngineTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Updater;
using FluentAssertions;

namespace BrowserSelector.UpdaterTests;

/// <summary>
/// <see cref="ApplyEngine"/> のテスト（Phase H-5）.
/// </summary>
/// <remarks>
/// 実ファイルI/Oを伴うため %TEMP%\BSUpdaterTest_{Guid} に作業ディレクトリを作り、確実に削除する.
/// </remarks>
public class ApplyEngineTests : IDisposable
{
    private readonly string _root;
    private readonly string _source;
    private readonly string _target;
    private readonly string _backupRoot;
    private readonly string _backup;

    public ApplyEngineTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"BSUpdaterTest_{Guid.NewGuid():N}");
        _source = Path.Combine(_root, "source");
        _target = Path.Combine(_root, "target");
        _backupRoot = Path.Combine(_root, "backup");
        _backup = Path.Combine(_backupRoot, "20260816_120000");

        Directory.CreateDirectory(_source);
        Directory.CreateDirectory(_target);
        Directory.CreateDirectory(_backupRoot);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // テストの後始末に失敗してもテスト結果には影響させない。
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Apply_NewVersion_ReplacesFilesAndReturnsSuccess()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_source, "BrowserSelector.Core.dll", "new-core");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, "BrowserSelector.Core.dll", "old-core");

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.Success);
        ReadFile(_target, "BrowserSelector.exe").Should().Be("new-exe");
        ReadFile(_target, "BrowserSelector.Core.dll").Should().Be("new-core");
    }

    [Fact]
    public void Apply_Success_RemovesOldTemporaryFiles()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.Success);
        File.Exists(Path.Combine(_target, "BrowserSelector.exe" + ApplyEngine.OldExtension)).Should().BeFalse();
    }

    [Fact]
    public void Apply_CreatesBackupOfExistingFiles()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");

        _ = ApplyEngine.Apply(CreateOptions());

        ReadFile(_backup, "BrowserSelector.exe").Should().Be("old-exe");
    }

    [Fact]
    public void Apply_ExcludesLogsDirectoryAndSettingsFromBackup()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, "settings.json", "{\"user\":true}");
        WriteFile(Path.Combine(_target, "logs"), "app.log", "log-line");

        _ = ApplyEngine.Apply(CreateOptions());

        File.Exists(Path.Combine(_backup, "settings.json")).Should().BeFalse();
        Directory.Exists(Path.Combine(_backup, "logs")).Should().BeFalse();
    }

    [Fact]
    public void Apply_DoesNotOverwriteUserSettings()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_source, "settings.json", "{\"default\":true}");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, "settings.json", "{\"user\":true}");

        _ = ApplyEngine.Apply(CreateOptions());

        ReadFile(_target, "settings.json").Should().Be("{\"user\":true}");
    }

    [Fact]
    public void Apply_UpdaterExecutable_IsPlacedAsPendingInsteadOfReplaced()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_source, ApplyEngine.UpdaterExecutableName, "new-updater");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, ApplyEngine.UpdaterExecutableName, "old-updater");

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.Success);

        // 自分自身は実行中でロックされるため置換せず .new として置く（1世代遅れを許容）。
        ReadFile(_target, ApplyEngine.UpdaterExecutableName).Should().Be("old-updater");
        ReadFile(_target, ApplyEngine.UpdaterExecutableName + ApplyEngine.PendingExtension).Should().Be("new-updater");
    }

    [Fact]
    public void Apply_CopiesNestedDirectories()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(Path.Combine(_source, "ja-JP"), "BrowserSelector.resources.dll", "new-resource");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.Success);
        ReadFile(Path.Combine(_target, "ja-JP"), "BrowserSelector.resources.dll").Should().Be("new-resource");
    }

    [Fact]
    public void Apply_LockedTargetFile_RollsBackAndKeepsOriginalContent()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_source, "Locked.dll", "new-locked");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, "Locked.dll", "old-locked");

        // バックアップ（読み取りコピー）は通り、置換のためのMoveだけが失敗する状態を作る。
        using FileStream locked = new(
            Path.Combine(_target, "Locked.dll"),
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.Read);

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.ApplyFailedRolledBack);

        // ロールバック後、ロックされていなかった側は元の内容に戻っていること。
        ReadFile(_target, "BrowserSelector.exe").Should().Be("old-exe");
    }

    [Fact]
    public void Apply_UnreadableTargetFile_FailsBackupWithoutModifyingAnything()
    {
        WriteFile(_source, "BrowserSelector.exe", "new-exe");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");
        WriteFile(_target, "Locked.dll", "old-locked");

        // FileShare.Noneだとバックアップのコピーすら読めない。まだ何も変更していないので
        // 適用へは進まず、安全に中断する。
        using FileStream locked = new(
            Path.Combine(_target, "Locked.dll"),
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.BackupFailed);
        ReadFile(_target, "BrowserSelector.exe").Should().Be("old-exe");
    }

    [Fact]
    public void Validate_MissingExecutableInSource_Fails()
    {
        WriteFile(_source, "SomethingElse.dll", "x");

        bool result = ApplyEngine.Validate(CreateOptions(), out string? error);

        result.Should().BeFalse();
        error.Should().Contain("BrowserSelector.exe");
    }

    [Fact]
    public void Validate_MissingSourceDirectory_Fails()
    {
        Directory.Delete(_source, recursive: true);

        bool result = ApplyEngine.Validate(CreateOptions(), out string? error);

        result.Should().BeFalse();
        error.Should().Contain("ソースディレクトリ");
    }

    [Fact]
    public void Apply_ValidationFailure_ReturnsValidationFailed()
    {
        // sourceにBrowserSelector.exeが無い＝適用しても壊れるだけなので何も変更せず中断する。
        WriteFile(_source, "OnlyThis.dll", "x");
        WriteFile(_target, "BrowserSelector.exe", "old-exe");

        UpdaterExitCode result = ApplyEngine.Apply(CreateOptions());

        result.Should().Be(UpdaterExitCode.ValidationFailed);
        ReadFile(_target, "BrowserSelector.exe").Should().Be("old-exe");
        Directory.Exists(_backup).Should().BeFalse();
    }

    [Fact]
    public void CleanupOldBackups_KeepsTwoMostRecentGenerations()
    {
        for (int i = 1; i <= 4; i++)
        {
            string generation = Path.Combine(_backupRoot, $"gen{i}");
            Directory.CreateDirectory(generation);
            Directory.SetLastWriteTimeUtc(generation, new DateTime(2026, 8, i, 0, 0, 0, DateTimeKind.Utc));
        }

        ApplyEngine.CleanupOldBackups(Path.Combine(_backupRoot, "gen4"));

        Directory.Exists(Path.Combine(_backupRoot, "gen4")).Should().BeTrue();
        Directory.Exists(Path.Combine(_backupRoot, "gen3")).Should().BeTrue();
        Directory.Exists(Path.Combine(_backupRoot, "gen2")).Should().BeFalse();
        Directory.Exists(Path.Combine(_backupRoot, "gen1")).Should().BeFalse();
    }

    [Theory]
    [InlineData("settings.json", true)]
    [InlineData("SETTINGS.JSON", true)]
    [InlineData(@"logs\app.log", true)]
    [InlineData(@"LOGS\nested\app.log", true)]
    [InlineData("BrowserSelector.exe", false)]
    [InlineData(@"ja-JP\BrowserSelector.resources.dll", false)]
    [InlineData(@"data\settings.json", false)]
    public void IsExcluded_ReturnsExpected(string relativePath, bool expected)
    {
        ApplyEngine.IsExcluded(relativePath).Should().Be(expected);
    }

    private static void WriteFile(string directory, string name, string content)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, name), content);
    }

    private static string ReadFile(string directory, string name) =>
        File.ReadAllText(Path.Combine(directory, name));

    private UpdaterOptions CreateOptions() => new()
    {
        Mode = "apply-zip",
        ProcessId = 1,
        Source = _source,
        Target = _target,
        Backup = _backup,
        ExecutableName = "BrowserSelector.exe",
        LogPath = Path.Combine(_root, "updater.log"),
        NoRelaunch = true,
    };
}
