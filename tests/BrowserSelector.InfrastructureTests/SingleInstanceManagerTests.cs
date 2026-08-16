using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// <see cref="SingleInstanceManager"/> のテストクラス.
/// Mutex取得、パイプ経由のURL往復、先行インスタンス不在時の通常起動を検証する.
/// </summary>
public sealed class SingleInstanceManagerTests
{
    /// <summary>
    /// 先行インスタンスが存在しない場合、TryAcquireがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public void TryAcquire_WithNoExistingInstance_ShouldReturnTrue()
    {
        // Arrange
        using var manager = new SingleInstanceManager();

        // Act
        bool acquired = manager.TryAcquire();

        // Assert
        acquired.Should().BeTrue();
    }

    /// <summary>
    /// 既に自プロセス内でMutexを取得済みの場合、2つ目のインスタンスはfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public void TryAcquire_WithAlreadyAcquiredInstance_ShouldReturnFalse()
    {
        // Arrange
        using var first = new SingleInstanceManager();
        using var second = new SingleInstanceManager();
        bool firstAcquired = first.TryAcquire();

        // Act
        bool secondAcquired = second.TryAcquire();

        // Assert
        firstAcquired.Should().BeTrue();
        secondAcquired.Should().BeFalse();
    }

    /// <summary>
    /// 先行インスタンスがリスナーを開始している状態でURLを送信すると、
    /// UrlReceivedイベントが送信したURLで発火することを確認するテスト.
    /// </summary>
    [Fact]
    public async Task TrySendToExistingInstanceAsync_WithListeningInstance_ShouldRaiseUrlReceived()
    {
        // Arrange
        using var manager = new SingleInstanceManager();
        bool acquired = manager.TryAcquire();
        acquired.Should().BeTrue();

        var url = new Uri("https://example.com/");
        UrlReceivedEventArgs? received = null;
        using SemaphoreSlim signal = new(0, 1);
        manager.UrlReceived += (_, e) =>
        {
            received = e;
            int previousCount = signal.Release();
            previousCount.Should().Be(0);
        };

        // Act
        bool sent = await SingleInstanceManager.TrySendToExistingInstanceAsync(url);
        bool signaled = await signal.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        sent.Should().BeTrue();
        signaled.Should().BeTrue();
        received.Should().NotBeNull();
        received!.Url.Should().Be(url.ToString());
    }

    /// <summary>
    /// リスナーを起動していない状態（先行インスタンス不在）でURL送信すると失敗することを確認するテスト.
    /// </summary>
    [Fact]
    public async Task TrySendToExistingInstanceAsync_WithNoListener_ShouldReturnFalse()
    {
        // Act
        bool sent = await SingleInstanceManager.TrySendToExistingInstanceAsync(new Uri("https://example.com/"));

        // Assert
        sent.Should().BeFalse();
    }
}
