using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BrowserSelector.UITests;

/// <summary>
/// STAスレッドでUIテストを実行するためのテストランナー
/// </summary>
public static class STAThreadTestRunner
{
    /// <summary>
    /// STAスレッドでテストを実行します
    /// </summary>
    /// <param name="testAction">実行するテストアクション</param>
    /// <param name="timeoutMs">タイムアウト時間（ミリ秒）</param>
    public static void RunInSTA(Action testAction, int timeoutMs = 30000)
    {
        Exception? exception = null;
        var tcs = new TaskCompletionSource<bool>();
        
        var thread = new Thread(() =>
        {
            try
            {
                // STAスレッドとして設定
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                
                // テストを実行
                testAction();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                exception = ex;
                tcs.SetResult(false);
            }
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        
        // タイムアウト付きで待機
        if (!tcs.Task.Wait(timeoutMs))
        {
            thread.Abort();
            throw new TimeoutException($"テストがタイムアウトしました: {timeoutMs}ms");
        }
        
        if (exception != null)
        {
            throw exception;
        }
    }
    
    /// <summary>
    /// STAスレッドで非同期テストを実行します
    /// </summary>
    /// <param name="testAction">実行するテストアクション</param>
    /// <param name="timeoutMs">タイムアウト時間（ミリ秒）</param>
    public static async Task RunInSTAAsync(Func<Task> testAction, int timeoutMs = 30000)
    {
        Exception? exception = null;
        var tcs = new TaskCompletionSource<bool>();
        
        var thread = new Thread(async () =>
        {
            try
            {
                // STAスレッドとして設定
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                
                // テストを実行
                await testAction();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                exception = ex;
                tcs.SetResult(false);
            }
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        
        // タイムアウト付きで待機
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            await tcs.Task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            thread.Abort();
            throw new TimeoutException($"テストがタイムアウトしました: {timeoutMs}ms");
        }
        
        if (exception != null)
        {
            throw exception;
        }
    }
}
