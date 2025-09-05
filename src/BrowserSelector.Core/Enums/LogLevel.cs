namespace BrowserSelector.Core.Enums;

/// <summary>
/// ログレベルを定義する列挙型
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// トレース情報（最も詳細なレベル）
    /// </summary>
    Trace = 0,

    /// <summary>
    /// デバッグ情報
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 一般的な情報
    /// </summary>
    Information = 2,

    /// <summary>
    /// 警告
    /// </summary>
    Warning = 3,

    /// <summary>
    /// エラー
    /// </summary>
    Error = 4,

    /// <summary>
    /// 致命的なエラー
    /// </summary>
    Critical = 5
}
