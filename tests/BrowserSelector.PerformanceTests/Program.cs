using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System;

namespace BrowserSelector.PerformanceTests;

/// <summary>
/// パフォーマンステスト実行プログラム.
/// </summary>
internal static class Program
{
    /// <summary>
    /// メインエントリーポイント.
    /// </summary>
    /// <param name="args">コマンドライン引数.</param>
    public static void Main(string[] args)
    {
        Console.WriteLine("=== BrowserSelector パフォーマンステスト ===");
        Console.WriteLine();

        // 設定の作成
        var config = DefaultConfig.Instance
            .AddExporter(MarkdownExporter.GitHub)
            .AddExporter(HtmlExporter.Default)
            .AddLogger(ConsoleLogger.Default);

        try
        {
            // 基本的なパフォーマンステストの実行
            Console.WriteLine("1. 基本パフォーマンステストを実行中...");
            _ = BenchmarkRunner.Run<SimplePerformanceBenchmarks>(config);
            Console.WriteLine("基本パフォーマンステスト完了");
            Console.WriteLine();

            Console.WriteLine("=== 全パフォーマンステスト完了 ===");
            Console.WriteLine("結果は BenchmarkDotNet.Artifacts フォルダに保存されました。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"パフォーマンステスト実行エラー: {ex.Message}");
            Console.WriteLine($"スタックトレース: {ex.StackTrace}");
        }
    }
}
