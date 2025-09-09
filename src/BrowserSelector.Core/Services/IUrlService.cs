// <copyright file="IUrlService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Core.Services;

/// <summary>
/// URL蜃ｦ逅・し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// URL繧呈ｭ｣隕丞喧縺吶ｋ.
    /// </summary>
    /// <param name="url">豁｣隕丞喧縺吶ｋURL.</param>
    /// <returns>豁｣隕丞喧縺輔ｌ縺欟RL.</returns>
    Task<string> NormalizeUrlAsync(string url);

    /// <summary>
    /// URL縺梧怏蜉ｹ縺九←縺・°繧呈､懆ｨｼ縺吶ｋ.
    /// </summary>
    /// <param name="url">讀懆ｨｼ縺吶ｋURL.</param>
    /// <returns>譛牙柑縺ｪ蝣ｴ蜷医・true.</returns>
    Task<bool> ValidateUrlAsync(string url);

    /// <summary>
    /// URL縺九ｉ繝峨Γ繧､繝ｳ繧呈歓蜃ｺ縺吶ｋ.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <returns>繝峨Γ繧､繝ｳ蜷・/returns>.
    string ExtractDomain(string url);

    /// <summary>
    /// 繝励Ο繝医さ繝ｫ繧定ｿｽ蜉縺吶ｋ・亥ｿ・ｦ√↓蠢懊§縺ｦ・・.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <returns>繝励Ο繝医さ繝ｫ縺瑚ｿｽ蜉縺輔ｌ縺欟RL.</returns>
    string AddProtocolIfNeeded(string url);
}
