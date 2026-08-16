// <copyright file="CountdownController.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 既定ブラウザへのカウントダウン自動起動を制御する状態機械（Phase D）.
/// マウス移動・キー入力での一時停止、トレイ格納中の停止・復帰時のリセットをサポートする.
/// UIタイマーへの依存を避けるため、1秒ごとの外部からの<see cref="Tick"/>呼び出しで駆動する設計とし、
/// ユニットテストで実タイマー無しに検証できるようにしている.
/// </summary>
public sealed class CountdownController
{
    private int _remainingSeconds;

    // CA1003: 呼び出し側は残り秒数の整数値のみを必要とし、専用EventArgsを新設するほどの複雑さが無いため、
    // 既存の一部イベント（例: BrowserChangedEventArgs等とは異なり）単純なEventHandler<int>のまま公開する。
#pragma warning disable CA1003
    /// <summary>
    /// カウントダウン中に発火する。<see cref="RemainingSeconds"/>が更新される度に呼び出される.
    /// </summary>
    public event EventHandler<int>? TickOccurred;
#pragma warning restore CA1003

    /// <summary>
    /// カウントダウンが0に達し、自動起動すべきタイミングで発火する.
    /// </summary>
    public event EventHandler? Elapsed;

    /// <summary>
    /// Gets a value indicating whether カウントダウンが現在実行中かどうか（一時停止中・トレイ格納中はfalse）.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether トレイ格納によりカウントダウンが停止しているかどうか.
    /// BrowserChooser3 v0.1.5 の既知バグ（トレイ格納中に裏でブラウザが起動する）を避けるため、
    /// トレイ格納中は<see cref="Tick"/>を呼んでも一切進行しない.
    /// </summary>
    public bool IsSuspendedByTray { get; private set; }

    /// <summary>
    /// Gets 残り秒数.
    /// </summary>
    public int RemainingSeconds => _remainingSeconds;

    /// <summary>
    /// カウントダウンを開始する.
    /// </summary>
    /// <param name="delaySeconds">開始する秒数。0以下の場合は開始しない（無効化）.</param>
    public void Start(int delaySeconds)
    {
        if (delaySeconds <= 0)
        {
            IsRunning = false;
            _remainingSeconds = 0;
            return;
        }

        _remainingSeconds = delaySeconds;
        IsSuspendedByTray = false;
        IsRunning = true;
        TickOccurred?.Invoke(this, _remainingSeconds);
    }

    /// <summary>
    /// カウントダウンを一時停止する（マウス移動・キー入力検知時）.
    /// </summary>
    public void Pause()
    {
        IsRunning = false;
    }

    /// <summary>
    /// 一時停止中のカウントダウンを再開する。トレイ格納中は再開しない.
    /// </summary>
    public void Resume()
    {
        if (IsSuspendedByTray || _remainingSeconds <= 0)
        {
            return;
        }

        IsRunning = true;
    }

    /// <summary>
    /// カウントダウンを完全に停止しリセットする.
    /// </summary>
    public void Reset()
    {
        IsRunning = false;
        _remainingSeconds = 0;
    }

    /// <summary>
    /// トレイへ格納された際に呼び出す。カウントダウンを強制停止する（既知バグ対策）.
    /// </summary>
    public void SuspendForTray()
    {
        IsSuspendedByTray = true;
        IsRunning = false;
    }

    /// <summary>
    /// トレイから復帰した際に呼び出す。カウントダウンをリセットする（既知バグ対策: 自動再開はしない）.
    /// </summary>
    public void ResumeFromTray()
    {
        IsSuspendedByTray = false;
        Reset();
    }

    /// <summary>
    /// 1秒経過を通知する。<see cref="IsRunning"/>がfalse、または<see cref="IsSuspendedByTray"/>がtrueの場合は何もしない.
    /// </summary>
    public void Tick()
    {
        if (!IsRunning || IsSuspendedByTray)
        {
            return;
        }

        _remainingSeconds--;
        if (_remainingSeconds <= 0)
        {
            _remainingSeconds = 0;
            IsRunning = false;
            TickOccurred?.Invoke(this, _remainingSeconds);
            Elapsed?.Invoke(this, EventArgs.Empty);
            return;
        }

        TickOccurred?.Invoke(this, _remainingSeconds);
    }
}
