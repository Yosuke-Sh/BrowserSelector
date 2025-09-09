// <copyright file="IRegistryService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// Windows繝ｬ繧ｸ繧ｹ繝医Μ縺九ｉ繝悶Λ繧ｦ繧ｶ諠・ｱ繧貞叙蠕励☆繧九し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// 繝ｬ繧ｸ繧ｹ繝医Μ縺九ｉ繝悶Λ繧ｦ繧ｶ繧呈､懷・.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync();
}
