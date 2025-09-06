using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Text.Json;

namespace BrowserSelector.IntegrationTests;

/// <summary>
/// テスト用のURLルールサービス実装
/// 実際のファイルシステムではなく、テスト用の一時ディレクトリを使用
/// </summary>
public class TestUrlRuleService : IUrlRuleService
{
    private readonly List<UrlRule> _rules = [];
    private readonly string _rulesFilePath;
    private readonly object _lockObject = new();
    private readonly ILogService? _logService;

    public TestUrlRuleService(ILogService? logService, string tempDirectory)
    {
        _logService = logService;
        _rulesFilePath = Path.Combine(tempDirectory, "urlrules.json");

        // 初期化時にルールを読み込む
        LoadRulesSync();
    }

    private void LoadRulesSync()
    {
        try
        {
            if (File.Exists(_rulesFilePath))
            {
                string json = File.ReadAllText(_rulesFilePath);
                List<UrlRule>? rules = JsonSerializer.Deserialize<List<UrlRule>>(json);
                if (rules != null)
                {
                    lock (_lockObject)
                    {
                        _rules.Clear();
                        _rules.AddRange(rules);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルールの読み込みエラー: {ex.Message}", "TestUrlRuleService", ex);
        }
    }

    public Task<IEnumerable<UrlRule>> GetAllRulesAsync()
    {
        _logService?.LogTrace("URLルール一覧取得開始", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                List<UrlRule> result = [.. _rules];
                _logService?.LogTrace($"URLルール一覧取得完了: {result.Count}件", "TestUrlRuleService");
                return Task.FromResult<IEnumerable<UrlRule>>(result);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール一覧取得エラー: {ex.Message}", "TestUrlRuleService", ex);
            return Task.FromResult<IEnumerable<UrlRule>>([]);
        }
    }

    public Task<IEnumerable<UrlRule>> GetEnabledRulesAsync()
    {
        _logService?.LogTrace("有効URLルール一覧取得開始", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                List<UrlRule> result = _rules.Where(r => r.IsEnabled).ToList();
                _logService?.LogTrace($"有効URLルール一覧取得完了: {result.Count}件", "TestUrlRuleService");
                return Task.FromResult<IEnumerable<UrlRule>>(result);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"有効URLルール一覧取得エラー: {ex.Message}", "TestUrlRuleService", ex);
            return Task.FromResult<IEnumerable<UrlRule>>([]);
        }
    }

    public async Task<bool> AddRuleAsync(UrlRule rule)
    {
        _logService?.LogTrace($"URLルール追加開始: {rule.Pattern}", "TestUrlRuleService");
        try
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.Pattern))
            {
                _logService?.LogWarning("無効なURLルール", "TestUrlRuleService");
                return false;
            }

            lock (_lockObject)
            {
                // 重複チェック
                if (_rules.Any(r => r.Pattern == rule.Pattern))
                {
                    _logService?.LogWarning($"重複するURLルール: {rule.Pattern}", "TestUrlRuleService");
                    return false;
                }

                _rules.Add(rule);
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール追加完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール追加エラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    public async Task<bool> UpdateRuleAsync(UrlRule rule)
    {
        _logService?.LogTrace($"URLルール更新開始: {rule.Pattern}", "TestUrlRuleService");
        try
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.Pattern))
            {
                _logService?.LogWarning("無効なURLルール", "TestUrlRuleService");
                return false;
            }

            lock (_lockObject)
            {
                UrlRule? existingRule = _rules.FirstOrDefault(r => r.Id == rule.Id);
                if (existingRule == null)
                {
                    _logService?.LogWarning($"更新対象のURLルールが見つかりません: {rule.Id}", "TestUrlRuleService");
                    return false;
                }

                int index = _rules.IndexOf(existingRule);
                _rules[index] = rule;
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール更新完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール更新エラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    public async Task<bool> DeleteRuleAsync(Guid ruleId)
    {
        _logService?.LogTrace($"URLルール削除開始: {ruleId}", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    _logService?.LogWarning($"削除対象のURLルールが見つかりません: {ruleId}", "TestUrlRuleService");
                    return false;
                }

                _ = _rules.Remove(rule);
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール削除完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール削除エラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    public Task<UrlRule?> GetRuleByIdAsync(Guid ruleId)
    {
        _logService?.LogTrace($"URLルール取得開始: {ruleId}", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                _logService?.LogTrace($"URLルール取得完了: {(rule != null ? "見つかった" : "見つからない")}", "TestUrlRuleService");
                return Task.FromResult(rule);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール取得エラー: {ex.Message}", "TestUrlRuleService", ex);
            return Task.FromResult<UrlRule?>(null);
        }
    }

    public async Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled)
    {
        _logService?.LogTrace($"URLルール切り替え開始: {ruleId}, Enabled={isEnabled}", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    _logService?.LogWarning($"切り替え対象のURLルールが見つかりません: {ruleId}", "TestUrlRuleService");
                    return false;
                }

                rule.IsEnabled = isEnabled;
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール切り替え完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール切り替えエラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    public Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers)
    {
        _logService?.LogTrace($"URLマッチング開始: {url}", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                IOrderedEnumerable<UrlRule> enabledRules = _rules.Where(r => r.IsEnabled).OrderBy(r => r.Priority);

                foreach (UrlRule? rule in enabledRules)
                {
                    if (rule.IsMatch(url))
                    {
                        Browser? browser = browsers.FirstOrDefault(b => b.Name == rule.BrowserName);
                        if (browser != null)
                        {
                            _logService?.LogTrace($"URLマッチング完了: {rule.Pattern} -> {browser.Name}", "TestUrlRuleService");
                            return Task.FromResult<Browser?>(browser);
                        }
                    }
                }

                _logService?.LogTrace("URLマッチング完了: マッチするルールなし", "TestUrlRuleService");
                return Task.FromResult<Browser?>(null);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLマッチングエラー: {ex.Message}", "TestUrlRuleService", ex);
            return Task.FromResult<Browser?>(null);
        }
    }

    public async Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority)
    {
        _logService?.LogTrace($"URLルール優先度変更開始: {ruleId}, Priority={newPriority}", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                if (rule == null)
                {
                    _logService?.LogWarning($"優先度変更対象のURLルールが見つかりません: {ruleId}", "TestUrlRuleService");
                    return false;
                }

                rule.Priority = newPriority;
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール優先度変更完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール優先度変更エラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    public async Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds)
    {
        _logService?.LogTrace("URLルール並び替え開始", "TestUrlRuleService");
        try
        {
            lock (_lockObject)
            {
                List<Guid> ruleIdList = ruleIds.ToList();
                List<UrlRule> reorderedRules = [];

                foreach (Guid ruleId in ruleIdList)
                {
                    UrlRule? rule = _rules.FirstOrDefault(r => r.Id == ruleId);
                    if (rule != null)
                    {
                        reorderedRules.Add(rule);
                    }
                }

                // 残りのルールを追加
                foreach (UrlRule? rule in _rules.Where(r => !ruleIdList.Contains(r.Id)))
                {
                    reorderedRules.Add(rule);
                }

                _rules.Clear();
                _rules.AddRange(reorderedRules);
            }

            await SaveRulesAsync();
            _logService?.LogTrace("URLルール並び替え完了", "TestUrlRuleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール並び替えエラー: {ex.Message}", "TestUrlRuleService", ex);
            return false;
        }
    }

    private Task SaveRulesAsync()
    {
        try
        {
            lock (_lockObject)
            {
                string json = JsonSerializer.Serialize(_rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_rulesFilePath, json);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール保存エラー: {ex.Message}", "TestUrlRuleService", ex);
        }
        return Task.CompletedTask;
    }
}

