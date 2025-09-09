using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Diagnostics;
using System.IO;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// URLルール管理サービスの実装.
/// </summary>
public class UrlRuleService : IUrlRuleService
{
    private readonly List<UrlRule> _rules= [];
    private readonly string _rulesFilePath;
    private readonly object _lockObject = new();
    private readonly ILogService? _logService;

    public UrlRuleService(ILogService? logService = null)
    {
        _logService = logService;

        // 設定ファイルのパスを設定
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appDataPath, "BrowserSelector");
        _ = Directory.CreateDirectory(appFolder);
        _rulesFilePath = Path.Combine(appFolder, "urlrules.json");

        // 初期化時は同期的にルールを読み込む（DI解決時のデッドロック回避のため）
        LoadRulesSync();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<UrlRule>> GetAllRulesAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_rules.OrderByDescending(r => r.Priority).AsEnumerable());
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<UrlRule>> GetEnabledRulesAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority).AsEnumerable());
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AddRuleAsync(UrlRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
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

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール追加エラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール追加エラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール追加エラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール追加エラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateRuleAsync(UrlRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        try
        {
            lock (_lockObject)
            {
                UrlRule? existingRule = _rules.Find(r => r.Id == rule.Id);
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

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール更新エラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール更新エラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール更新エラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール更新エラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteRuleAsync(Guid ruleId)
    {
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.Find(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                _ = _rules.Remove(rule);
                Debug.WriteLine($"UrlRuleService: ルールを削除しました - {rule.DisplayName}");
            }

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール削除エラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール削除エラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール削除エラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール削除エラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled)
    {
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.Find(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                rule.IsEnabled = isEnabled;
                rule.UpdatedAt = DateTime.Now;
                Debug.WriteLine($"UrlRuleService: ルールの状態を変更しました - {rule.DisplayName} -> {(isEnabled ? "有効" : "無効")}");
            }

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール状態変更エラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール状態変更エラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール状態変更エラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール状態変更エラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers)
    {
        try
        {
            lock (_lockObject)
            {
                // 有効なルールを優先度順に並べて検索
                IOrderedEnumerable<UrlRule> enabledRules = _rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority);

                foreach (UrlRule? rule in enabledRules)
                {
                    if (rule.IsMatch(url))
                    {
                        Browser? browser = browsers.FirstOrDefault(b => b.Name.Equals(rule.BrowserName, StringComparison.OrdinalIgnoreCase));
                        if (browser != null)
                        {
                            _logService?.LogInformation($"URL '{url}' にマッチするルール '{rule.Pattern}' が見つかりました -> {browser.Name}", "UrlRuleService");
                            return Task.FromResult<Browser?>(browser);
                        }
                        else
                        {
                            _logService?.LogWarning($"ルール '{rule.Pattern}' で指定されたブラウザ '{rule.BrowserName}' が見つかりません", "UrlRuleService");
                        }
                    }
                }

                _logService?.LogDebug($"URL '{url}' にマッチするルールが見つかりませんでした", "UrlRuleService");
                return Task.FromResult<Browser?>(null);
            }
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"ブラウザ検索エラー（引数例外） - {ex.Message}", "UrlRuleService", ex);
            return Task.FromResult<Browser?>(null);
        }
        catch (InvalidOperationException ex)
        {
            _logService?.LogError($"ブラウザ検索エラー（無効な操作例外） - {ex.Message}", "UrlRuleService", ex);
            return Task.FromResult<Browser?>(null);
        }
    }

    /// <inheritdoc/>
    public Task<Browser?> FindMatchingBrowserAsync(Uri url, IEnumerable<Browser> browsers)
    {
        return FindMatchingBrowserAsync(url?.ToString() ?? string.Empty, browsers);
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority)
    {
        try
        {
            lock (_lockObject)
            {
                UrlRule? rule = _rules.Find(r => r.Id == ruleId);
                if (rule == null)
                {
                    Debug.WriteLine($"UrlRuleService: ルールが見つかりません - {ruleId}");
                    return false;
                }

                rule.Priority = newPriority;
                rule.UpdatedAt = DateTime.Now;
                Debug.WriteLine($"UrlRuleService: ルールの優先度を変更しました - {rule.DisplayName} -> {newPriority}");
            }

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: 優先度変更エラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: 優先度変更エラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: 優先度変更エラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: 優先度変更エラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds)
    {
        try
        {
            lock (_lockObject)
            {
                List<Guid> ruleIdList = ruleIds.ToList();
                int maxPriority = ruleIdList.Count;

                for (int i = 0; i < ruleIdList.Count; i++)
                {
                    UrlRule? rule = _rules.Find(r => r.Id == ruleIdList[i]);
                    if (rule != null)
                    {
                        rule.Priority = maxPriority - i;
                        rule.UpdatedAt = DateTime.Now;
                    }
                }

                Debug.WriteLine($"UrlRuleService: {ruleIdList.Count} 個のルールの優先度を並び替えました");
            }

            await SaveRulesAsync().ConfigureAwait(false);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール並び替えエラー（アクセス権限なし） - {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール並び替えエラー（セキュリティ例外） - {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール並び替えエラー（引数例外） - {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール並び替えエラー（I/O例外） - {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ルールをファイルから同期的に読み込み（DI解決時のハング回避用）.
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

            string json = File.ReadAllText(_rulesFilePath);
            List<UrlRule>? rules = System.Text.Json.JsonSerializer.Deserialize<List<UrlRule>>(json);

            lock (_lockObject)
            {
                _rules.Clear();
                if (rules != null)
                {
                    _rules.AddRange(rules);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー（アクセス権限なし） - {ex.Message}");

            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー（セキュリティ例外） - {ex.Message}");

            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー（引数例外） - {ex.Message}");

            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー（I/O例外） - {ex.Message}");

            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"UrlRuleService: 同期読み込みエラー（JSON例外） - {ex.Message}");

            // 失敗時はデフォルトルールで初期化
            CreateDefaultRulesSync();
        }
    }

    /// <summary>
    /// ルールをファイルに保存.
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

            string json = System.Text.Json.JsonSerializer.Serialize(rulesToSave, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_rulesFilePath, json).ConfigureAwait(false);

            Debug.WriteLine($"UrlRuleService: {rulesToSave.Count} 個のルールを保存しました");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー（アクセス権限なし） - {ex.Message}");
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー（セキュリティ例外） - {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー（引数例外） - {ex.Message}");
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー（I/O例外） - {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"UrlRuleService: ルール保存エラー（JSON例外） - {ex.Message}");
        }
    }

    /// <summary>
    /// デフォルトルールを同期的に作成・保存.
    /// </summary>
    private void CreateDefaultRulesSync()
    {
        // サンプルルールは不要のため、空のルールリストで初期化
        lock (_lockObject)
        {
            _rules.Clear();
        }

        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(_rules, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_rulesFilePath, json);
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー（アクセス権限なし） - {ex.Message}");
        }
        catch (System.Security.SecurityException ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー（セキュリティ例外） - {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー（引数例外） - {ex.Message}");
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー（I/O例外） - {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"UrlRuleService: デフォルトルール保存エラー（JSON例外） - {ex.Message}");
        }
    }
}
