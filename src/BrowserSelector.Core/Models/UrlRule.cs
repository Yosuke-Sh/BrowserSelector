using System.ComponentModel.DataAnnotations;

namespace BrowserSelector.Core.Models;

/// <summary>
/// URLパターンに基づくブラウザ振り分けルール
/// </summary>
public class UrlRule
{
    /// <summary>
    /// ルールの一意識別子
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// URLパターン（例: "*.google.com", "http*", "*.github.com/*"）
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// 対象ブラウザの名前
    /// </summary>
    [Required]
    [StringLength(100)]
    public string BrowserName { get; set; } = string.Empty;

    /// <summary>
    /// ルールの優先度（数値が大きいほど優先）
    /// </summary>
    [Range(1, 100)]
    public int Priority { get; set; } = 50;

    /// <summary>
    /// ルールが有効かどうか
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// ルールの説明
    /// </summary>
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// ルールが指定されたURLにマッチするかを判定
    /// </summary>
    /// <param name="url">判定対象のURL</param>
    /// <returns>マッチする場合true</returns>
    public bool IsMatch(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(Pattern))
            return false;

        // パターンを小文字に変換
        var pattern = Pattern.ToLowerInvariant();
        var targetUrl = url.ToLowerInvariant();

        // ワイルドカードパターンの処理
        if (pattern.Contains('*'))
        {
            return IsWildcardMatch(pattern, targetUrl);
        }

        // 完全一致
        return targetUrl == pattern;
    }

    /// <summary>
    /// ワイルドカードパターンのマッチング
    /// </summary>
    /// <param name="pattern">ワイルドカードを含むパターン</param>
    /// <param name="url">判定対象のURL</param>
    /// <returns>マッチする場合true</returns>
    private bool IsWildcardMatch(string pattern, string url)
    {
        // パターンをワイルドカードで分割
        var parts = pattern.Split('*', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 0)
            return true; // パターンが "*" のみの場合

        if (parts.Length == 1)
        {
            // パターンが "*text" または "text*" の場合
            if (pattern.StartsWith("*"))
                return url.EndsWith(parts[0]);
            if (pattern.EndsWith("*"))
                return url.StartsWith(parts[0]);
        }

        // 複数のワイルドカードがある場合
        var currentIndex = 0;
        foreach (var part in parts)
        {
            var foundIndex = url.IndexOf(part, currentIndex);
            if (foundIndex == -1)
                return false;
            currentIndex = foundIndex + part.Length;
        }

        return true;
    }

    /// <summary>
    /// ルールの表示名を取得
    /// </summary>
    public string DisplayName => $"{Pattern} → {BrowserName} (優先度: {Priority})";

    /// <summary>
    /// ルールの詳細情報を取得
    /// </summary>
    public string GetDetails()
    {
        var status = IsEnabled ? "有効" : "無効";
        var desc = string.IsNullOrWhiteSpace(Description) ? "説明なし" : Description;
        return $"パターン: {Pattern}\nブラウザ: {BrowserName}\n優先度: {Priority}\n状態: {status}\n説明: {desc}";
    }
}

