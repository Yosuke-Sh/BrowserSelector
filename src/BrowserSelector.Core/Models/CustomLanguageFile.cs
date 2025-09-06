namespace BrowserSelector.Core.Models;

/// <summary>
/// カスタム言語ファイルのモデル
/// </summary>
public class CustomLanguageFile
{
    /// <summary>
    /// カルチャーコード（例: zh-CN, ko-KR）
    /// </summary>
    public string CultureCode { get; set; } = string.Empty;

    /// <summary>
    /// 表示名（例: 中文 (简体), 한국어）
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// リソース辞書（キー: リソースキー, 値: 翻訳文字列）
    /// </summary>
    public Dictionary<string, string> Resources { get; set; } = [];

    /// <summary>
    /// ファイルの作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// ファイルの更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// ファイルのバージョン
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 作成者
    /// </summary>
    public string? Author { get; set; }
}
