// <copyright file="IUrlRuleService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// URL繝ｫ繝ｼ繝ｫ邂｡逅・し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface IUrlRuleService
{
    /// <summary>
    /// 縺吶∋縺ｦ縺ｮURL繝ｫ繝ｼ繝ｫ繧貞叙蠕・.
    /// </summary>
    /// <returns>URL繝ｫ繝ｼ繝ｫ縺ｮ荳隕ｧ.</returns>
    Task<IEnumerable<UrlRule>> GetAllRulesAsync();

    /// <summary>
    /// 譛牙柑縺ｪURL繝ｫ繝ｼ繝ｫ繧貞叙蠕・.
    /// </summary>
    /// <returns>譛牙柑縺ｪURL繝ｫ繝ｼ繝ｫ縺ｮ荳隕ｧ.</returns>
    Task<IEnumerable<UrlRule>> GetEnabledRulesAsync();

    /// <summary>
    /// URL繝ｫ繝ｼ繝ｫ繧定ｿｽ蜉.
    /// </summary>
    /// <param name="rule">霑ｽ蜉縺吶ｋ繝ｫ繝ｼ繝ｫ.</param>
    /// <returns>霑ｽ蜉謌仙粥譎Ｕrue.</returns>
    Task<bool> AddRuleAsync(UrlRule rule);

    /// <summary>
    /// URL繝ｫ繝ｼ繝ｫ繧呈峩譁ｰ.
    /// </summary>
    /// <param name="rule">譖ｴ譁ｰ縺吶ｋ繝ｫ繝ｼ繝ｫ.</param>
    /// <returns>譖ｴ譁ｰ謌仙粥譎Ｕrue.</returns>
    Task<bool> UpdateRuleAsync(UrlRule rule);

    /// <summary>
    /// URL繝ｫ繝ｼ繝ｫ繧貞炎髯､.
    /// </summary>
    /// <param name="ruleId">蜑企勁縺吶ｋ繝ｫ繝ｼ繝ｫ縺ｮID.</param>
    /// <returns>蜑企勁謌仙粥譎Ｕrue.</returns>
    Task<bool> DeleteRuleAsync(Guid ruleId);

    /// <summary>
    /// URL繝ｫ繝ｼ繝ｫ縺ｮ譛牙柑/辟｡蜉ｹ繧貞・繧頑崛縺・.
    /// </summary>
    /// <param name="ruleId">蟇ｾ雎｡繝ｫ繝ｼ繝ｫ縺ｮID.</param>
    /// <param name="isEnabled">譛牙柑縺ｫ縺吶ｋ縺九←縺・°.</param>
    /// <returns>蛻・ｊ譖ｿ縺域・蜉滓凾true.</returns>
    Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled);

    /// <summary>
    /// 謖・ｮ壹＆繧後◆URL縺ｫ繝槭ャ繝√☆繧九ヶ繝ｩ繧ｦ繧ｶ繧呈､懃ｴ｢.
    /// </summary>
    /// <param name="url">讀懃ｴ｢蟇ｾ雎｡縺ｮURL.</param>
    /// <param name="browsers">蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繝悶Λ繧ｦ繧ｶ縺ｮ荳隕ｧ.</param>
    /// <returns>繝槭ャ繝√☆繧九ヶ繝ｩ繧ｦ繧ｶ・郁ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・null・・/returns>.
    Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers);

    /// <summary>
    /// 謖・ｮ壹＆繧後◆URL縺ｫ繝槭ャ繝√☆繧九ヶ繝ｩ繧ｦ繧ｶ繧呈､懃ｴ｢（Uri版）.
    /// </summary>
    /// <param name="url">讀懃ｴ｢蟇ｾ雎｡縺ｮURL.</param>
    /// <param name="browsers">蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繝悶Λ繧ｦ繧ｶ縺ｮ荳隕ｧ.</param>
    /// <returns>繝槭ャ繝√☆繧九ヶ繝ｩ繧ｦ繧ｶ・郁ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・null・・/returns>.
    Task<Browser?> FindMatchingBrowserAsync(Uri url, IEnumerable<Browser> browsers);

    /// <summary>
    /// 繝ｫ繝ｼ繝ｫ縺ｮ蜆ｪ蜈亥ｺｦ繧貞､画峩.
    /// </summary>
    /// <param name="ruleId">蟇ｾ雎｡繝ｫ繝ｼ繝ｫ縺ｮID.</param>
    /// <param name="newPriority">譁ｰ縺励＞蜆ｪ蜈亥ｺｦ.</param>
    /// <returns>螟画峩謌仙粥譎Ｕrue.</returns>
    Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority);

    /// <summary>
    /// 繝ｫ繝ｼ繝ｫ縺ｮ蜆ｪ蜈亥ｺｦ繧剃ｸｦ縺ｳ譖ｿ縺・.
    /// </summary>
    /// <param name="ruleIds">蜆ｪ蜈亥ｺｦ鬆・↓荳ｦ縺ｹ縺溘Ν繝ｼ繝ｫID縺ｮ荳隕ｧ.</param>
    /// <returns>荳ｦ縺ｳ譖ｿ縺域・蜉滓凾true.</returns>
    Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds);
}
