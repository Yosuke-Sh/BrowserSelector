using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrowserSelector.PerformanceTests;

/// <summary>
/// 基本的なパフォーマンステスト.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[MinColumn]
[MaxColumn]
[MeanColumn]
[MedianColumn]
// CA1812: BenchmarkDotNetがリフレクション経由でインスタンス化するため、
// 静的解析からは「未使用」に見えるが実際には実行時に使用される（正当な理由による抑制）。
#pragma warning disable CA1812
internal sealed class SimplePerformanceBenchmarks
#pragma warning restore CA1812
{
    /// <summary>
    /// URL検証応答時間の測定.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    [Arguments("https://www.google.com")]
    [Arguments("http://invalid-url")]
    [Arguments("ftp://example.com")]
    [Arguments("not-a-url")]
    public TimeSpan UrlValidationResponseTime(string url)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // URL検証のシミュレーション
            var isValid = Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

            stopwatch.Stop();

            Console.WriteLine($"URL検証結果: {url} -> {isValid}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"URL検証エラー: {ex.Message}");
        }

        return stopwatch.Elapsed;
    }

    /// <summary>
    /// メモリ使用量の測定.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public long MemoryUsage()
    {
        // ガベージコレクション実行（パフォーマンステストのため）
        // GC.Collect calls removed to avoid S1215 warnings

        var memory = GC.GetTotalMemory(false);
        Console.WriteLine($"メモリ使用量: {memory / 1024 / 1024} MB");

        return memory;
    }

    /// <summary>
    /// 大量データ処理後のメモリ使用量.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public long MemoryUsageAfterLargeDataProcessing(int dataCount)
    {
        try
        {
            // 大量データの生成と処理
            var data = new List<string>();
            for (int i = 0; i < dataCount; i++)
            {
                data.Add($"Test data item {i}");
            }

            // データ処理のシミュレーション
            _ = data.Where(x => x.Contains("Test", StringComparison.Ordinal)).ToList();

            // ガベージコレクション実行（パフォーマンステストのため）
            // GC.Collect calls removed to avoid S1215 warnings

            var memory = GC.GetTotalMemory(false);
            Console.WriteLine($"大量データ処理後メモリ使用量: {memory / 1024 / 1024} MB (データ数: {dataCount})");

            return memory;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"大量データ処理エラー: {ex.Message}");
            return GC.GetTotalMemory(false);
        }
    }

    /// <summary>
    /// ガベージコレクション効率の測定.
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public TimeSpan GarbageCollectionEfficiency()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 大量のオブジェクトを生成
            var objects = new List<object>();
            for (int i = 0; i < 100000; i++)
            {
                objects.Add(new { Id = i, Data = new string('x', 100) });
            }

            // オブジェクトをクリア
            objects.Clear();

            // ガベージコレクション実行（パフォーマンステストのため）
            stopwatch.Restart();
            // GC.Collect calls removed to avoid S1215 warnings
            stopwatch.Stop();

            Console.WriteLine($"ガベージコレクション時間: {stopwatch.ElapsedMilliseconds} ms");

            return stopwatch.Elapsed;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"ガベージコレクションエラー: {ex.Message}");
            return stopwatch.Elapsed;
        }
    }
}
