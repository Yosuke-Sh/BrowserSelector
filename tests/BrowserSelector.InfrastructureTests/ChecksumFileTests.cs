using BrowserSelector.Infrastructure.Updates;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-2: SHA256SUMS.txtの解析テスト.
/// コード署名が無いためチェックサム照合が唯一の完全性検証手段であり、
/// ここの解析が静かに失敗すると「検証したつもり」になるのが最も危険なパターン.
/// </summary>
public class ChecksumFileTests
{
    private const string HashA = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private const string HashB = "5891b5b522d5df086d0ff0b110fbd9d21bb4fc7163af34d08286a2e846f6be03";

    // Parseはハッシュを大文字へ正規化して返す（CA1308回避。照合側も同じ正規化を行う）。
    private static readonly string ExpectedA = HashA.ToUpperInvariant();
    private static readonly string ExpectedB = HashB.ToUpperInvariant();

    [Fact]
    public void Parse_ShouldReadReleaseWorkflowFormat()
    {
        // release.ymlが生成する形式（ハッシュ + スペース2つ + ファイル名）。
        string content = $"{HashA}  BrowserSelector-Setup-v0.3.0.exe\n{HashB}  BrowserSelector-v0.3.0-win-x64.zip\n";

        IReadOnlyDictionary<string, string> result = ChecksumFile.Parse(content);

        result.Should().HaveCount(2);
        result["BrowserSelector-Setup-v0.3.0.exe"].Should().Be(ExpectedA);
        result["BrowserSelector-v0.3.0-win-x64.zip"].Should().Be(ExpectedB);
    }

    [Fact]
    public void Parse_ShouldAcceptSingleSpaceSeparator()
    {
        ChecksumFile.Parse($"{HashA} file.exe")["file.exe"].Should().Be(ExpectedA);
    }

    [Fact]
    public void Parse_ShouldAcceptTabSeparator()
    {
        ChecksumFile.Parse($"{HashA}\tfile.exe")["file.exe"].Should().Be(ExpectedA);
    }

    [Fact]
    public void Parse_ShouldStripBinaryModeAsterisk()
    {
        // GNU coreutilsのバイナリモード表記。
        ChecksumFile.Parse($"{HashA} *file.exe")["file.exe"].Should().Be(ExpectedA);
    }

    [Fact]
    public void Parse_ShouldHandleCrLfAndLf()
    {
        ChecksumFile.Parse($"{HashA}  a.exe\r\n{HashB}  b.zip\r\n").Should().HaveCount(2);
        ChecksumFile.Parse($"{HashA}  a.exe\n{HashB}  b.zip\n").Should().HaveCount(2);
    }

    [Fact]
    public void Parse_ShouldSkipBlankLines()
    {
        ChecksumFile.Parse($"\n\n{HashA}  a.exe\n\n   \n").Should().ContainSingle();
    }

    [Theory]
    [InlineData("nothexnothexnothexnothexnothexnothexnothexnothexnothexnothexnotx  a.exe")]  // 非hex
    [InlineData("abc123  a.exe")]                                                            // 桁数不足
    [InlineData("garbage line without hash")]
    [InlineData("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]         // ファイル名なし
    public void Parse_ShouldSkipMalformedLines(string line)
    {
        ChecksumFile.Parse(line).Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldNormalizeHashCaseConsistently()
    {
        // 小文字表記でも大文字表記でも同じ正規化済みハッシュになること。
        ChecksumFile.Parse($"{HashA}  a.exe")["a.exe"]
            .Should().Be(ChecksumFile.Parse($"{HashA.ToUpperInvariant()}  a.exe")["a.exe"]);
    }

    [Fact]
    public void Parse_ShouldMatchFileNameCaseInsensitively()
    {
        ChecksumFile.Parse($"{HashA}  BrowserSelector-Setup-v0.3.0.exe")
            .Should().ContainKey("browserselector-setup-v0.3.0.exe");
    }

    [Fact]
    public void Parse_ShouldPreserveFileNamesContainingSpaces()
    {
        ChecksumFile.Parse($"{HashA}  my file.exe")["my file.exe"].Should().Be(ExpectedA);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_ShouldReturnEmptyForNullOrBlank(string? content)
    {
        ChecksumFile.Parse(content).Should().BeEmpty();
    }
}
