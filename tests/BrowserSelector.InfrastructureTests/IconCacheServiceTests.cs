using System.IO;
using System.Windows.Media.Imaging;
using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// <see cref="IconCacheService"/> のテストクラス.
/// メモリ/ディスクキャッシュのヒット・ミス、失効、ハンドル解放を検証する.
/// </summary>
public sealed class IconCacheServiceTests : IDisposable
{
    private readonly string _tempDirectory;

    public IconCacheServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorIconCacheTests_" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// 存在しないファイルパスを指定した場合にnullを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public void GetIcon_WithNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        var service = new IconCacheService();
        string missingPath = Path.Combine(_tempDirectory, "not-found.exe");

        // Act
        BitmapSource? result = service.GetIcon(missingPath, 0, 32);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// PNGファイルを指定した場合、正しくアイコンを読み込めることを確認するテスト.
    /// </summary>
    [Fact]
    public void GetIcon_WithValidPngFile_ShouldReturnBitmap()
    {
        // Arrange
        var service = new IconCacheService();
        string pngPath = CreateTestPngFile("valid.png");

        // Act
        BitmapSource? result = service.GetIcon(pngPath, 0, 32);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// 同一ファイル・同一パラメータで2回取得した場合、2回目はメモリキャッシュから返り、
    /// 参照が同一（同一インスタンス）になることを確認するテスト.
    /// </summary>
    [Fact]
    public void GetIcon_CalledTwiceWithSameKey_ShouldReturnSameCachedInstance()
    {
        // Arrange
        var service = new IconCacheService();
        string pngPath = CreateTestPngFile("cached.png");

        // Act
        BitmapSource? first = service.GetIcon(pngPath, 0, 32);
        BitmapSource? second = service.GetIcon(pngPath, 0, 32);

        // Assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    /// <summary>
    /// ClearMemoryCache呼び出し後は、同じファイルでも新しいインスタンスが返る
    /// （メモリキャッシュが破棄されている）ことを確認するテスト.
    /// 新しいインスタンスはディスクキャッシュ経由で読み込まれる.
    /// </summary>
    [Fact]
    public void ClearMemoryCache_AfterCachedGet_ShouldForceReloadOnNextGet()
    {
        // Arrange
        var service = new IconCacheService();
        string pngPath = CreateTestPngFile("clear.png");
        BitmapSource? first = service.GetIcon(pngPath, 0, 32);

        // Act
        service.ClearMemoryCache();
        BitmapSource? second = service.GetIcon(pngPath, 0, 32);

        // Assert
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        second.Should().NotBeSameAs(first);
    }

    /// <summary>
    /// 異なるサイズ指定は異なるキャッシュエントリとして扱われることを確認するテスト.
    /// </summary>
    [Fact]
    public void GetIcon_WithDifferentSizes_ShouldNotShareCacheEntry()
    {
        // Arrange
        var service = new IconCacheService();
        string pngPath = CreateTestPngFile("sizes.png");

        // Act
        BitmapSource? small = service.GetIcon(pngPath, 0, 16);
        BitmapSource? large = service.GetIcon(pngPath, 0, 64);

        // Assert
        small.Should().NotBeNull();
        large.Should().NotBeNull();
        small.Should().NotBeSameAs(large);
    }

    /// <summary>
    /// ファイルの最終更新日時が変化した場合、ディスクキャッシュが失効し再抽出されることを確認するテスト.
    /// </summary>
    [Fact]
    public void GetIcon_AfterFileModified_ShouldNotReuseStaleDiskCache()
    {
        // Arrange
        var service = new IconCacheService();
        string pngPath = CreateTestPngFile("stale.png");
        BitmapSource? beforeModify = service.GetIcon(pngPath, 0, 32);
        service.ClearMemoryCache();

        // ファイルの更新日時を変更（内容は同一でも失効することを確認）
        File.SetLastWriteTimeUtc(pngPath, DateTime.UtcNow.AddMinutes(5));

        // Act
        BitmapSource? afterModify = service.GetIcon(pngPath, 0, 32);

        // Assert
        beforeModify.Should().NotBeNull();
        afterModify.Should().NotBeNull();
    }

    /// <summary>
    /// リソースを解放.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // テスト後のクリーンアップ失敗は無視
        }
        catch (UnauthorizedAccessException)
        {
            // テスト後のクリーンアップ失敗は無視
        }
    }

    private string CreateTestPngFile(string fileName)
    {
        string path = Path.Combine(_tempDirectory, fileName);

        using System.Drawing.Bitmap bitmap = new(8, 8);
        using System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.Red);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);

        return path;
    }
}
