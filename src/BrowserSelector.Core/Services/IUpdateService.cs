// <copyright file="IUpdateService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 閾ｪ蜍輔い繝・・繝・・繝域ｩ溯・繧呈署萓帙☆繧九し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface IUpdateService : IDisposable
{
    /// <summary>
    /// 繧｢繝・・繝・・繝医′蛻ｩ逕ｨ蜿ｯ閭ｽ縺ｫ縺ｪ縺｣縺滓凾縺ｮ繧､繝吶Φ繝・
    /// </summary>
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// 繧｢繝・・繝・・繝医ｒ繝√ぉ繝・け.
    /// </summary>
    /// <returns>繧｢繝・・繝・・繝域ュ蝣ｱ.</returns>
    Task<UpdateInfo?> CheckForUpdatesAsync();

    /// <summary>
    /// 繧｢繝・・繝・・繝医ｒ繝繧ｦ繝ｳ繝ｭ繝ｼ繝・.
    /// </summary>
    /// <param name="updateInfo">繧｢繝・・繝・・繝域ュ蝣ｱ.</param>
    /// 
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns><param name="progress">騾ｲ謐怜ｱ蜻・/param>.
    /// <returns>繝繧ｦ繝ｳ繝ｭ繝ｼ繝峨′謌仙粥縺励◆縺九←縺・°</returns>
    Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);

    /// <summary>
    /// 繧｢繝・・繝・・繝医ｒ繧､繝ｳ繧ｹ繝医・繝ｫ.
    /// </summary>
    /// <param name="updateInfo">繧｢繝・・繝・・繝域ュ蝣ｱ.</param>
    /// <returns>繧､繝ｳ繧ｹ繝医・繝ｫ縺梧・蜉溘＠縺溘°縺ｩ縺・°.</returns>
    Task<bool> InstallUpdateAsync(UpdateInfo updateInfo);

    /// <summary>
    /// 繧｢繝・・繝・・繝医ｒ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ.
    /// </summary>
    /// <returns>繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ縺梧・蜉溘＠縺溘°縺ｩ縺・°.</returns>
    Task<bool> RollbackUpdateAsync();

    /// <summary>
    /// 繝舌ャ繧ｯ繧｢繝・・繧剃ｽ懈・.
    /// </summary>
    /// <returns>繝舌ャ繧ｯ繧｢繝・・縺梧・蜉溘＠縺溘°縺ｩ縺・°.</returns>
    bool CreateBackup();
}
