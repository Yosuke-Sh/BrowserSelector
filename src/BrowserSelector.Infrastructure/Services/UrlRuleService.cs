using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Diagnostics;
using System.IO;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// URLルール管理サービスの実装
/// </summary>
public class UrlRuleService : IUrlRuleService
{
    private readonly List<UrlRule> _rules = new();
    private readonly string _rulesFilePath;
    private readonly object _lockObject = new();

    public UrlRuleService()
    {
        // 設定ファイルのパスを設定
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "BrowserSelector");
        Directory.CreateDirectory(appFolder);
        _rulesFilePath = Path.Combine(appFolder, "urlrules.json");

        // 初期化時は同期的にルールを読み込む（DI解決時のデッドロック回避のため）
        LoadRulesSync();
    }

    public Task<IEnumerable<UrlRule>> GetAllRulesAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_rules.OrderByDescending(r => r.Priority).AsEnumerable());
        }
    }

    public Task<IEnumerable<UrlRule>> GetEnabledRulesAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority).AsEnumerable());
        }
    }

    public async Task<bool> AddRuleAsync(UrlRule rule)
    {
        try
        {
            lock (_lockObject)
            {
                // 重複チェック
                if (_rules.Any(r => r.Pattern.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"UrlRuleService: パターン '{rule.Pattern}' は既に存在します");
                    return false;
                }

                _rules.Add(rule);
                Debug.WriteLine($"UrlRuleService: ルールを追加しました - {rule.DisplayName}");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール追加エラー - {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateRuleAsync(UrlRule rule)
    {
        try
        {
            lock (_lockObject)
            {
                var existingRule = _rules.FirstOrDefault(r => r.Id == rule.Id);
                if (existingRule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {rule.Id}");
                    return false;
                }

                // 更新
                existingRule.Pattern = rule.Pattern;
                existingRule.BrowserName = rule.BrowserName;
                existingRule.Priority = rule.Priority;
                existingRule.IsEnabled = rule.IsEnabled;
                existingRule.Description = rule.Description;
                existingRule.UpdatedAt = DateTime.Now;

                Debug.WriteLine($"UrlRuleService: ルールを更新しました - {rule.DisplayName}");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール更新エラー - {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteRuleAsync(Guid ruleId)
    {
        try
        {
            lock (_lockObject)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                _rules.Remove(rule);
                Debug.WriteLine($"UrlRuleService: ルールを削除しました - {rule.DisplayName}");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール削除エラー - {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled)
    {
        try
        {
            lock (_lockObject)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                rule.IsEnabled = isEnabled;
                rule.UpdatedAt = DateTime.Now;
                Debug.WriteLine($"UrlRuleService: ルールの状態を変更しました - {rule.DisplayName} -> {(isEnabled ? "有効" : "無効")}");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール状態変更エラー - {ex.Message}");
            return false;
        }
    }

    public Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers)
    {
        try
        {
            lock (_lockObject)
            {
                // 有効なルールを優先度順に並べて検索
                var enabledRules = _rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority);

                foreach (var rule in enabledRules)
                {
                    if (rule.IsMatch(url))
                    {
                        var browser = browsers.FirstOrDefault(b => b.Name.Equals(rule.BrowserName, StringComparison.OrdinalIgnoreCase));
                        if (browser != null)
                        {
                            Debug.WriteLine($"UrlRuleService: URL '{url}' にマッチするルール '{rule.Pattern}' が見つかりました -> {browser.Name}");
                            return Task.FromResult<Browser?>(browser);
                        }
                        else
                        {
                            Debug.WriteLine($"UrlRuleService: ルール '{rule.Pattern}' で指定されたブラウザ '{rule.BrowserName}' が見つかりません");
                        }
                    }
                }

                Debug.WriteLine($"UrlRuleService: URL '{url}' にマッチするルールが見つかりませんでした");
                return Task.FromResult<Browser?>(null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ブラウザ検索エラー - {ex.Message}");
            return Task.FromResult<Browser?>(null);
        }
    }

    public async Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority)
    {
        try
        {
            lock (_lockObject)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                rule.Priority = newPriority;
                rule.UpdatedAt = DateTime.Now;
                Debug.WriteLine($"UrlRuleService: ルールの優先度を変更しました - {rule.DisplayName} -> {newPriority}");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: 優先度変更エラー - {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds)
    {
        try
        {
            lock (_lockObject)
            {
                var ruleIdList = ruleIds.ToList();
                var maxPriority = ruleIdList.Count;

                for (int i = 0; i < ruleIdList.Count; i++)
                {
                    var rule = _rules.FirstOrDefault(r => r.Id == ruleIdList[i]);
                    if (rule != null)
                    {
                        rule.Priority = maxPriority - i;
                        rule.UpdatedAt = DateTime.Now;
                    }
                }

                Debug.WriteLine($"UrlRuleService: {ruleIdList.Count} 個のルールの優先度を並び替えました");
            }

            await SaveRulesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール並び替えエラー - {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ルールをファイルから同期的に読み込み（DI解決時のハング回避用）
    /// </summary>
    private void LoadRulesSync()
    {
        try
        {
            if (!File.Exists(_rulesFilePath))
            {
                // デフォルトルールを作成して保存
                CreateDefaultRulesSync();
                return;
            }

            var json = File.ReadAllText(_rulesFilePath);
            var rules = System.Text.Json.JsonSerializer.Deserialize<List<UrlRule>>(json);

            lock (_lockObject)
            {
                _rules.Clear();
                if (rules != null)
                {
                    _rules.AddRange(rules);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー - {ex.Message}");
            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
    }

    /// <summary>
    /// ルールをファイルに保存
    /// </summary>
    private async Task SaveRulesAsync()
    {
        try
        {
            List<UrlRule> rulesToSave;
            lock (_lockObject)
            {
                rulesToSave = _rules.ToList();
            }

            var json = System.Text.Json.JsonSerializer.Serialize(rulesToSave, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_rulesFilePath, json);

            Debug.WriteLine($"UrlRuleService: {rulesToSave.Count} 個のルールを保存しました");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー - {ex.Message}");
        }
    }

    /// <summary>
    /// デフォルトルールを同期的に作成・保存
    /// </summary>
    private void CreateDefaultRulesSync()
    {
        var defaultRules = new List<UrlRule>
        {
            new UrlRule
            {
                Pattern = "*.google.com",
                BrowserName = "Chrome",
                Priority = 80,
                Description = "Google系サイトはChromeで開く",
                IsEnabled = true
            },
            new UrlRule
            {
                Pattern = "*.microsoft.com",
                BrowserName = "Edge",
                Priority = 80,
                Description = "Microsoft系サイトはEdgeで開く",
                IsEnabled = true
            },
            new UrlRule
            {
                Pattern = "http*",
                BrowserName = "Chrome",
                Priority = 60,
                Description = "HTTPサイトはChromeで開く",
                IsEnabled = true
            }
        };

        lock (_lockObject)
        {
            _rules.Clear();
            _rules.AddRange(defaultRules);
        }

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_rules, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_rulesFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー - {ex.Message}");
        }
    }
}
