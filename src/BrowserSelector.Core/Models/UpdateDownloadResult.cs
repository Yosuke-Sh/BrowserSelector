// <copyright file="UpdateDownloadResult.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models;

/// <summary>
/// ダウンロード（＋完全性検証、Portableの場合はZIP展開まで）の結果（Phase H-1）.
/// bool返しでは失敗理由を区別できないため結果型にしている.
/// </summary>
/// <remarks>
/// 位置指定レコード構文を避けている理由は<see cref="UpdateAsset"/>と同じ（SA1313の誤検知回避）.
/// </remarks>
public sealed record UpdateDownloadResult
{
    /// <summary>
    /// Gets a value indicating whether 成功したかどうか.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets 成功時の成果物パス。Installerはインストーラexe、Portableは展開済みディレクトリ.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets 失敗の分類。成功時は<see cref="UpdateDownloadFailure.None"/>.
    /// </summary>
    public UpdateDownloadFailure Failure { get; init; }

    /// <summary>
    /// 成功結果を生成する.
    /// </summary>
    /// <param name="filePath">成果物のパス.</param>
    /// <returns>成功を表す結果.</returns>
    public static UpdateDownloadResult Succeeded(string filePath) =>
        new() { Success = true, FilePath = filePath, Failure = UpdateDownloadFailure.None };

    /// <summary>
    /// キャンセルを表す結果を生成する.
    /// </summary>
    /// <returns>キャンセルを表す結果.</returns>
    public static UpdateDownloadResult Canceled() =>
        Failed(UpdateDownloadFailure.Canceled);

    /// <summary>
    /// 失敗結果を生成する.
    /// </summary>
    /// <param name="failure">失敗の分類.</param>
    /// <returns>失敗を表す結果.</returns>
    public static UpdateDownloadResult Failed(UpdateDownloadFailure failure) =>
        new() { Success = false, FilePath = null, Failure = failure };
}
