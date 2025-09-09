// <copyright file="IProtocolHandler.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 繝励Ο繝医さ繝ｫ繝上Φ繝峨Λ繝ｼ繧堤ｮ｡逅・☆繧九し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface IProtocolHandler
{
    /// <summary>
    /// 繝励Ο繝医さ繝ｫ繧堤匳骭ｲ.
    /// </summary>
    /// <param name="applicationPath">繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ繝代せ.</param>
    /// <returns>逋ｻ骭ｲ縺梧・蜉溘＠縺溘°縺ｩ縺・°.</returns>
    bool RegisterProtocol(string applicationPath);

    /// <summary>
    /// 繝励Ο繝医さ繝ｫ繧堤匳骭ｲ隗｣髯､.
    /// </summary>
    /// <returns>逋ｻ骭ｲ隗｣髯､縺梧・蜉溘＠縺溘°縺ｩ縺・°.</returns>
    bool UnregisterProtocol();

    /// <summary>
    /// 繝励Ο繝医さ繝ｫ縺檎匳骭ｲ縺輔ｌ縺ｦ縺・ｋ縺九メ繧ｧ繝・け.
    /// </summary>
    /// <returns>逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺九←縺・°.</returns>
    bool IsProtocolRegistered();

    /// <summary>
    /// 繝励Ο繝医さ繝ｫURL縺九ｉ繝代Λ繝｡繝ｼ繧ｿ繧呈歓蜃ｺ.
    /// </summary>
    /// <param name="protocolUrl">繝励Ο繝医さ繝ｫURL.</param>
    /// <returns>謚ｽ蜃ｺ縺輔ｌ縺欟RL.</returns>
    string? ExtractUrlFromProtocol(string protocolUrl);

    /// <summary>
    /// 繝励Ο繝医さ繝ｫURL繧堤函謌・.
    /// </summary>
    /// <param name="url">蜈・・URL.</param>
    /// <returns>繝励Ο繝医さ繝ｫURL.</returns>
    string CreateProtocolUrl(string url);

    /// <summary>
    /// 繝励Ο繝医さ繝ｫ逋ｻ骭ｲ諠・ｱ繧貞叙蠕・.
    /// </summary>
    /// <returns>逋ｻ骭ｲ諠・ｱ.</returns>
    ProtocolRegistrationInfo? GetProtocolRegistrationInfo();
}
