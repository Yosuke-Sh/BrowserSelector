// <copyright file="IUrlRuleService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// URLルール管琁E��ービスのインターフェース
/// </summary>
public interface IUrlRuleService
{
    /// <summary>
    /// すべてのURLルールを取征E
    /// </summary>
    /// <returns>URLルールの一覧</returns>
    Task<IEnumerable<UrlRule>> GetAllRulesAsync();

    /// <summary>
    /// 有効なURLルールを取征E
    /// </summary>
    /// <returns>有効なURLルールの一覧</returns>
    Task<IEnumerable<UrlRule>> GetEnabledRulesAsync();

    /// <summary>
    /// URLルールを追加
    /// </summary>
    /// <param name="rule">追加するルール</param>
    /// <returns>追加成功時true</returns>
    Task<bool> AddRuleAsync(UrlRule rule);

    /// <summary>
    /// URLルールを更新
    /// </summary>
    /// <param name="rule">更新するルール</param>
    /// <returns>更新成功時true</returns>
    Task<bool> UpdateRuleAsync(UrlRule rule);

    /// <summary>
    /// URLルールを削除
    /// </summary>
    /// <param name="ruleId">削除するルールのID</param>
    /// <returns>削除成功時true</returns>
    Task<bool> DeleteRuleAsync(Guid ruleId);

    /// <summary>
    /// URLルールの有効/無効を�Eり替ぁE
    /// </summary>
    /// <param name="ruleId">対象ルールのID</param>
    /// <param name="isEnabled">有効にするかどぁE��</param>
    /// <returns>刁E��替え�E功時true</returns>
    Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled);

    /// <summary>
    /// 持E��されたURLにマッチするブラウザを検索
    /// </summary>
    /// <param name="url">検索対象のURL</param>
    /// <param name="browsers">利用可能なブラウザの一覧</param>
    /// <returns>マッチするブラウザ�E�見つからなぁE��合�Enull�E�E/returns>
    Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers);

    /// <summary>
    /// ルールの優先度を変更
    /// </summary>
    /// <param name="ruleId">対象ルールのID</param>
    /// <param name="newPriority">新しい優先度</param>
    /// <returns>変更成功時true</returns>
    Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority);

    /// <summary>
    /// ルールの優先度を並び替ぁE
    /// </summary>
    /// <param name="ruleIds">優先度頁E��並べたルールIDの一覧</param>
    /// <returns>並び替え�E功時true</returns>
    Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds);
}

