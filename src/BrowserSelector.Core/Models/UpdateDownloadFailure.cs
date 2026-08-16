// <copyright file="UpdateDownloadFailure.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models;

/// <summary>
/// ダウンロード失敗の分類（Phase H-1）.
/// UI側が「チェックサム不一致（＝危険。改竄の可能性を伝える）」と
/// 「ネットワーク断（＝無害。黙って次回リトライ）」を区別できるようにするためのenum.
/// </summary>
public enum UpdateDownloadFailure
{
    /// <summary>
    /// 失敗していない（成功時の値）.
    /// </summary>
    None = 0,

    /// <summary>
    /// 通信エラー・タイムアウト・HTTPステータス異常.
    /// </summary>
    Network = 1,

    /// <summary>
    /// SHA256が一致しなかった。ダウンロードしたファイルは削除済み.
    /// </summary>
    ChecksumMismatch = 2,

    /// <summary>
    /// SHA256SUMS.txtを取得できず検証が行えなかった。コード署名が無いため検証省略は許容しない.
    /// </summary>
    ChecksumUnavailable = 3,

    /// <summary>
    /// 呼び出し側によりキャンセルされた.
    /// </summary>
    Canceled = 4,

    /// <summary>
    /// ファイル入出力エラー（ディスク容量不足・書き込み権限不足・ZIP展開失敗等）.
    /// </summary>
    Io = 5,
}
