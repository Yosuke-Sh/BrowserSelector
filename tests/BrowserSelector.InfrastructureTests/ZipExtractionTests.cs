using System.IO;
using System.IO.Compression;
using System.Text;
using BrowserSelector.Infrastructure.Updates;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-4: ポータブルZIP展開のテスト（Zip Slip・ZIP爆弾対策）.
/// 展開をUpdater.exeではなく本体側で行うのは、検証ロジックをテスト可能な場所に置き、
/// Updaterには「展開済みディレクトリのコピー」だけを担わせるため.
/// </summary>
public sealed class ZipExtractionTests : IDisposable
{
    private readonly string _workDirectory;

    public ZipExtractionTests()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), $"BSZipTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_workDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workDirectory))
            {
                Directory.Delete(_workDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void TryExtract_ShouldExtractValidArchive()
    {
        Dictionary<string, string> entries = new()
        {
            ["BrowserSelector.exe"] = "exe",
            ["BrowserSelector.dll"] = "dll",
            [@"runtimes\win\native.dll"] = "native",
        };
        string zip = CreateZip("valid.zip", entries);
        string destination = Path.Combine(_workDirectory, "out");

        ZipExtractor.TryExtract(zip, destination, out string? reason).Should().BeTrue();
        reason.Should().BeNull();

        File.Exists(Path.Combine(destination, "BrowserSelector.exe")).Should().BeTrue();
        File.Exists(Path.Combine(destination, "runtimes", "win", "native.dll")).Should().BeTrue();
    }

    [Fact]
    public void TryExtract_ShouldFailWhenExecutableIsMissing()
    {
        // BrowserSelector.exeが無い成果物を適用すると起動不能になるため、展開段階で弾く。
        string zip = CreateZip("noexe.zip", new() { ["readme.txt"] = "hello" });

        ZipExtractor.TryExtract(zip, Path.Combine(_workDirectory, "out"), out string? reason).Should().BeFalse();
        reason.Should().Contain("BrowserSelector.exe");
    }

    [Fact]
    public void TryExtract_ShouldRejectPathTraversalEntry()
    {
        string zip = CreateZipRaw("slip.zip", ("../evil.exe", "pwned"));

        ZipExtractor.TryExtract(zip, Path.Combine(_workDirectory, "out"), out string? reason).Should().BeFalse();
        reason.Should().NotBeNull();

        // 展開先の外にファイルが作られていないこと。
        File.Exists(Path.Combine(_workDirectory, "evil.exe")).Should().BeFalse();
    }

    [Fact]
    public void TryExtract_ShouldRejectNestedPathTraversalEntry()
    {
        string zip = CreateZipRaw("slip2.zip", ("sub/../../evil.exe", "pwned"));

        ZipExtractor.TryExtract(zip, Path.Combine(_workDirectory, "out"), out _).Should().BeFalse();
        File.Exists(Path.Combine(_workDirectory, "evil.exe")).Should().BeFalse();
    }

    [Theory]
    [InlineData(@"C:\Windows\System32\evil.dll")]
    [InlineData(@"\absolute\evil.exe")]
    [InlineData("..")]
    [InlineData("../x")]
    [InlineData(@"..\x")]
    [InlineData("file.txt:hidden")]
    [InlineData("")]
    [InlineData(null)]
    public void IsEntryNameSafe_ShouldRejectDangerousNames(string? entryName)
    {
        ZipExtractor.IsEntryNameSafe(entryName).Should().BeFalse();
    }

    [Theory]
    [InlineData("BrowserSelector.exe")]
    [InlineData("runtimes/win-x64/native/x.dll")]
    [InlineData(@"sub\dir\file.txt")]
    [InlineData("name..with..dots.txt")]
    public void IsEntryNameSafe_ShouldAcceptNormalNames(string entryName)
    {
        ZipExtractor.IsEntryNameSafe(entryName).Should().BeTrue();
    }

    [Fact]
    public void TryExtract_ShouldRejectTooManyEntries()
    {
        Dictionary<string, string> entries = [];
        for (int i = 0; i <= ZipExtractor.MaxEntryCount; i++)
        {
            entries[$"f{i}.txt"] = "x";
        }

        string zip = CreateZip("bomb.zip", entries);

        ZipExtractor.TryExtract(zip, Path.Combine(_workDirectory, "out"), out string? reason).Should().BeFalse();
        reason.Should().Contain("エントリ数");
    }

    [Fact]
    public void TryExtract_ShouldFailForCorruptArchive()
    {
        string zip = Path.Combine(_workDirectory, "corrupt.zip");
        File.WriteAllText(zip, "this is not a zip file");

        ZipExtractor.TryExtract(zip, Path.Combine(_workDirectory, "out"), out string? reason).Should().BeFalse();
        reason.Should().NotBeNull();
    }

    [Fact]
    public void TryExtract_ShouldNotWriteOutsideDestinationWithSimilarPrefix()
    {
        // "out" と "outside" が前方一致で誤判定されないこと（区切り文字込みで比較している）。
        string zip = CreateZip("ok.zip", new() { ["BrowserSelector.exe"] = "exe" });
        string destination = Path.Combine(_workDirectory, "out");

        ZipExtractor.TryExtract(zip, destination, out _).Should().BeTrue();
        Directory.Exists(Path.Combine(_workDirectory, "outside")).Should().BeFalse();
    }

    private string CreateZip(string name, Dictionary<string, string> entries)
    {
        string path = Path.Combine(_workDirectory, name);
        using FileStream fs = File.Create(path);
        using ZipArchive archive = new(fs, ZipArchiveMode.Create);

        foreach ((string entryName, string content) in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName);
            using Stream stream = entry.Open();
            stream.Write(Encoding.UTF8.GetBytes(content));
        }

        return path;
    }

    private string CreateZipRaw(string name, params (string EntryName, string Content)[] entries)
    {
        string path = Path.Combine(_workDirectory, name);
        using FileStream fs = File.Create(path);
        using ZipArchive archive = new(fs, ZipArchiveMode.Create);

        foreach ((string entryName, string content) in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName);
            using Stream stream = entry.Open();
            stream.Write(Encoding.UTF8.GetBytes(content));
        }

        return path;
    }
}
