// <copyright file="SingleInstanceManager.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// アプリケーションの多重起動を検知し、既存インスタンスへURLを引き継ぐためのマネージャー.
/// 名前付きMutexで先行インスタンスの有無を判定し、名前付きパイプでURLを転送する.
/// </summary>
public sealed class SingleInstanceManager : IDisposable
{
    private const string MutexName = "BrowserSelector_SingleInstance_Mutex";
    private const string PipeName = "BrowserSelector_SingleInstance_Pipe";
    private const int PipeConnectTimeoutMs = 2000;

    private readonly ILogService? _logService;
    private Mutex? _mutex;
    private CancellationTokenSource? _listenerCancellation;
    private Task? _listenerTask;
    private bool _disposed;

    /// <summary>
    /// <see cref="SingleInstanceManager"/> クラスの新しいインスタンスを初期化します.
    /// </summary>
    /// <param name="logService">ログサービス（省略可）.</param>
    public SingleInstanceManager(ILogService? logService = null)
    {
        _logService = logService;
    }

    /// <summary>
    /// 先行インスタンスからURLを受信した際に発火するイベント.
    /// </summary>
    public event EventHandler<UrlReceivedEventArgs>? UrlReceived;

    /// <summary>
    /// 既存の先行インスタンスへ名前付きパイプ経由でURLを送信します.
    /// </summary>
    /// <param name="url">転送するURL（未指定の場合は空文字を送信し、単にウィンドウ復元のみを要求する）.</param>
    /// <returns>送信に成功した場合は true.</returns>
    public static async Task<bool> TrySendToExistingInstanceAsync(Uri? url)
    {
        try
        {
            using NamedPipeClientStream client = new(".", PipeName, PipeDirection.Out);
            using CancellationTokenSource cts = new(PipeConnectTimeoutMs);
            await client.ConnectAsync(cts.Token).ConfigureAwait(false);

            byte[] payload = Encoding.UTF8.GetBytes(url?.ToString() ?? string.Empty);
            await client.WriteAsync(payload).ConfigureAwait(false);
            await client.FlushAsync().ConfigureAwait(false);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// 自プロセスが最初のインスタンスかどうかを判定し、そうであれば以後の起動を受け付けるリスナーを開始します.
    /// 先行インスタンスが存在する場合は false を返すので、呼び出し側は起動処理を中断し、
    /// <see cref="TrySendToExistingInstanceAsync"/> でURLを転送してから即終了すること.
    /// </summary>
    /// <returns>自プロセスが最初のインスタンスであれば true.</returns>
    public bool TryAcquire()
    {
        _mutex = new Mutex(initiallyOwned: true, name: MutexName, createdNew: out bool createdNew);

        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
            return false;
        }

        StartListener();
        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _listenerCancellation?.Cancel();
        try
        {
            _listenerTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // シャットダウン時のリスナー終了待機失敗は無視
        }

        _listenerCancellation?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
    }

    private void StartListener()
    {
        _listenerCancellation = new CancellationTokenSource();
        CancellationToken token = _listenerCancellation.Token;
        _listenerTask = Task.Run(() => ListenLoopAsync(token), token);
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream server = new(
                    PipeName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                using MemoryStream buffer = new();
                await server.CopyToAsync(buffer, token).ConfigureAwait(false);
                string url = Encoding.UTF8.GetString(buffer.ToArray());

                UrlReceived?.Invoke(this, new UrlReceivedEventArgs(url));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException ex)
            {
                _logService?.LogWarning($"単一インスタンスパイプ通信でエラーが発生しました: {ex.Message}", nameof(SingleInstanceManager), ex);
            }
        }
    }
}
