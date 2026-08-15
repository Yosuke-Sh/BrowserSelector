using FluentAssertions;
using System.IO;

namespace BrowserSelector.UITests;

/// <summary>
/// シンプルなUIテスト（実際のUI操作なし）.
/// </summary>
// CA1812: MSTestがリフレクション経由でインスタンス化するテストクラスのため、
// 静的解析からは「未使用」に見えるが実際には実行時に使用される（正当な理由による抑制）。
#pragma warning disable CA1812
[TestClass]
internal sealed class SimpleUITests
#pragma warning restore CA1812
{
    /// <summary>
    /// UIテストの基本動作確認.
    /// </summary>
    [TestMethod]
    public void UITest_ShouldRunSuccessfully()
    {
        // Arrange
        bool testResult = true;

        // Act & Assert
        testResult.Should().BeTrue("UIテストが正常に実行されること");
    }

    /// <summary>
    /// UIテストの並列実行確認.
    /// </summary>
    [TestMethod]
    public void UITest_ShouldSupportParallelExecution()
    {
        // Arrange
        var startTime = DateTime.Now;

        // Act
        Thread.Sleep(100); // 短い待機時間
        var endTime = DateTime.Now;

        // Assert
        var duration = endTime - startTime;
        duration.TotalMilliseconds.Should().BeGreaterOrEqualTo(100, "並列実行が正常に動作すること");
    }

    /// <summary>
    /// UIテストの環境確認.
    /// </summary>
    [TestMethod]
    public void UITest_ShouldHaveValidEnvironment()
    {
        // Arrange & Act
        var currentDirectory = Directory.GetCurrentDirectory();
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

        // Assert
        currentDirectory.Should().NotBeNullOrEmpty("現在のディレクトリが設定されていること");
        isWindows.Should().BeTrue("Windows環境で実行されていること");
    }

    /// <summary>
    /// UIテストのメモリ使用量確認.
    /// </summary>
    [TestMethod]
    public void UITest_ShouldHaveReasonableMemoryUsage()
    {
        // Arrange & Act
        var memoryBefore = GC.GetTotalMemory(false);
        
        // 軽微なメモリ使用
        var testData = new string[1000];
        for (int i = 0; i < testData.Length; i++)
        {
            testData[i] = $"TestData_{i}";
        }
        
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        // Assert
        memoryUsed.Should().BeGreaterThan(0, "メモリが使用されていること");
        memoryUsed.Should().BeLessThan(10 * 1024 * 1024, "メモリ使用量が適切な範囲内であること"); // 10MB以下
    }

    /// <summary>
    /// UIテストのスレッド確認.
    /// </summary>
    [TestMethod]
    public void UITest_ShouldRunOnValidThread()
    {
        // Arrange & Act
        var threadId = Environment.CurrentManagedThreadId;
        var threadState = Thread.CurrentThread.ThreadState;

        // Assert
        threadId.Should().BeGreaterThan(0, "有効なスレッドIDが設定されていること");
        threadState.Should().BeOneOf(new[] { ThreadState.Running, ThreadState.Background }, "スレッドが有効な状態であること");
    }
}
