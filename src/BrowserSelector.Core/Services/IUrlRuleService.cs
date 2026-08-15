// <copyright file="IUrlRuleService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// URLルール管理サービスのインターフェース.
/// </summary>
public interface IUrlRuleService
{
    /// <summary>
    /// すべてのURLルールを取得.
    /// </summary>
    /// <returns>URLルールの一覧.</returns>
    Task<IEnumerable<UrlRule>> GetAllRulesAsync();

    /// <summary>
    /// 有効なURLルールを取得.
    /// </summary>
    /// <returns>有効なURLルールの一覧.</returns>
    Task<IEnumerable<UrlRule>> GetEnabledRulesAsync();

    /// <summary>
    /// URLルールを追加.
    /// </summary>
    /// <param name="rule">追加するルール.</param>
    /// <returns>追加成功時true.</returns>
    Task<bool> AddRuleAsync(UrlRule rule);

    /// <summary>
    /// URLルールを更新.
    /// </summary>
    /// <param name="rule">更新するルール.</param>
    /// <returns>更新成功時true.</returns>
    Task<bool> UpdateRuleAsync(UrlRule rule);

    /// <summary>
    /// URLルールを削除.
    /// </summary>
    /// <param name="ruleId">削除するルールのID.</param>
    /// <returns>削除成功時true.</returns>
    Task<bool> DeleteRuleAsync(Guid ruleId);

    /// <summary>
    /// URLルールの有効/無効を切り替え.
    /// </summary>
    /// <param name="ruleId">対象ルールのID.</param>
    /// <param name="isEnabled">有効にするかどうか.</param>
    /// <returns>切り替え成功時true.</returns>
    Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled);

    /// <summary>
    /// 指定されたURLにマッチするブラウザを検索.
    /// </summary>
    /// <param name="url">検索対象のURL.</param>
    /// <param name="browsers">利用可能なブラウザの一覧.</param>
    /// <returns>マッチするブラウザ（見つからない場合はnull）.</returns>
    Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers);

    /// <summary>
    /// 指定されたURLにマッチするブラウザを検索（Uri版）.
    /// </summary>
    /// <param name="url">検索対象のURL.</param>
    /// <param name="browsers">利用可能なブラウザの一覧.</param>
    /// <returns>マッチするブラウザ（見つからない場合はnull）.</returns>
    Task<Browser?> FindMatchingBrowserAsync(Uri url, IEnumerable<Browser> browsers);

    /// <summary>
    /// ルールの優先度を変更.
    /// </summary>
    /// <param name="ruleId">対象ルールのID.</param>
    /// <param name="newPriority">新しい優先度.</param>
    /// <returns>変更成功時true.</returns>
    Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority);

    /// <summary>
    /// ルールの優先度を並び替え.
    /// </summary>
    /// <param name="ruleIds">優先度順に並べたルールIDの一覧.</param>
    /// <returns>並び替え成功時true.</returns>
    Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds);
}
