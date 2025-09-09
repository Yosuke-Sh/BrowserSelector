// <copyright file="ISystemTrayService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 繧ｷ繧ｹ繝・Β繝医Ξ繧､讖溯・繧呈署萓帙☆繧九し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface ISystemTrayService
{
    /// <summary>
    /// 繧ｷ繧ｹ繝・Β繝医Ξ繧､繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺檎匱逕溘＠縺滓凾縺ｮ繧､繝吶Φ繝・
    /// </summary>
    event EventHandler<SystemTrayEventArgs>? SystemTrayAction;

    /// <summary>
    /// 繧ｷ繧ｹ繝・Β繝医Ξ繧､繧｢繧､繧ｳ繝ｳ繧貞・譛溷喧.
    /// </summary>
    /// <param name="iconPath">繧｢繧､繧ｳ繝ｳ繝輔ぃ繧､繝ｫ縺ｮ繝代せ.</param>
    /// <param name="tooltipText">繝・・繝ｫ繝√ャ繝励ユ繧ｭ繧ｹ繝・/param>.
    void InitializeSystemTray(string iconPath, string tooltipText);

    /// <summary>
    /// 繧ｷ繧ｹ繝・Β繝医Ξ繧､繧｢繧､繧ｳ繝ｳ繧定｡ｨ遉ｺ.
    /// </summary>
    void ShowSystemTray();

    /// <summary>
    /// 繧ｷ繧ｹ繝・Β繝医Ξ繧､繧｢繧､繧ｳ繝ｳ繧帝撼陦ｨ遉ｺ.
    /// </summary>
    void HideSystemTray();

    /// <summary>
    /// 繝舌Ν繝ｼ繝ｳ繝・ぅ繝・・繧定｡ｨ遉ｺ.
    /// </summary>
    /// <param name="title">繧ｿ繧､繝医Ν.</param>
    /// <param name="text">繝・く繧ｹ繝・/param>.
    /// <param name="icon">繧｢繧､繧ｳ繝ｳ繧ｿ繧､繝・/param>
    /// <param name="timeout">陦ｨ遉ｺ譎る俣・医Α繝ｪ遘抵ｼ・/param>
    void ShowBalloonTip(string title, string text, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info, int timeout = 3000);

    /// <summary>
    /// 繧ｳ繝ｳ繝・く繧ｹ繝医Γ繝九Η繝ｼ繧呈峩譁ｰ.
    /// </summary>
    /// <param name="menuItems">繝｡繝九Η繝ｼ繧｢繧､繝・Β.</param>
    void UpdateContextMenu(SystemTrayMenuItems menuItems);
}
