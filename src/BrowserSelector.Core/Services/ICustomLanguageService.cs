// <copyright file="ICustomLanguageService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 繧ｫ繧ｹ繧ｿ繝險隱槭ヵ繧｡繧､繝ｫ邂｡逅・し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface ICustomLanguageService
{
    /// <summary>
    /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ險隱樔ｸ隕ｧ繧貞叙蠕・.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync();

    /// <summary>
    /// 繧ｫ繧ｹ繧ｿ繝險隱槭ヵ繧｡繧､繝ｫ繧定ｿｽ蜉.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> AddCustomLanguageAsync(string languageFilePath);

    /// <summary>
    /// 繧ｫ繧ｹ繧ｿ繝險隱槭ｒ蜑企勁.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> RemoveCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// 險隱槭ヵ繧｡繧､繝ｫ縺ｮ讀懆ｨｼ.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> ValidateLanguageFileAsync(string languageFilePath);

    /// <summary>
    /// 繧ｫ繧ｹ繧ｿ繝險隱槭ヵ繧ｩ繝ｫ繝縺ｮ繝代せ繧貞叙蠕・.
    /// </summary>
    /// <returns></returns>
    string GetCustomLanguageFolder();

    /// <summary>
    /// 繧ｫ繧ｹ繧ｿ繝險隱槭ヵ繧｡繧､繝ｫ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// 繧ｫ繧ｹ繧ｿ繝險隱槭ヵ繧｡繧､繝ｫ縺ｮ菫晏ｭ・.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources);

    /// <summary>
    /// 險隱槭ヵ繧｡繧､繝ｫ繝・Φ繝励Ξ繝ｼ繝医ｒ逕滓・.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> GenerateLanguageTemplateAsync(string cultureCode, string displayName);

    /// <summary>
    /// 蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｪ繝ｪ繧ｽ繝ｼ繧ｹ繧ｭ繝ｼ縺ｮ荳隕ｧ繧貞叙蠕・.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<string>> GetAvailableResourceKeysAsync();
}
