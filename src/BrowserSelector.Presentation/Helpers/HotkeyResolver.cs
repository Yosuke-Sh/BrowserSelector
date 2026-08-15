// <copyright file="HotkeyResolver.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Windows.Input;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// ブラウザ選択のホットキー（1-9 / A-Z）を解決するヘルパー.
/// BrowserChooser3 の教訓どおり、数字キーの比較を <see cref="Key.ToString"/> で行うと
/// <c>"D5"</c> のような文字列になり単純な文字比較と食い違うため、
/// <see cref="Key"/> を明示的に数値・英字へ正規化してから比較する（Phase C-4）.
/// </summary>
public static class HotkeyResolver
{
    /// <summary>
    /// 表示用ホットキーバッジの一覧（1〜9、続けてA〜Z）。ブラウザの表示順に対応する.
    /// </summary>
    public static IReadOnlyList<char> BadgeSequence { get; } = BuildBadgeSequence();

    /// <summary>
    /// キー入力からホットキー文字（'1'-'9', 'A'-'Z'）を解決する。Ctrl/Altが押されている場合は無効（<see langword="null"/>）を返す.
    /// Shiftのみは許容する（"Shift+1"のような入力でも同じキーとして扱う）.
    /// </summary>
    /// <param name="key">押下されたキー.</param>
    /// <param name="modifiers">同時に押されている修飾キー.</param>
    /// <returns>解決されたホットキー文字。無効な入力の場合は <see langword="null"/>.</returns>
    public static char? Resolve(Key key, ModifierKeys modifiers)
    {
        if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.None)
        {
            return null;
        }

        // 数字キー（メイン列 D0-D9、テンキー NumPad0-NumPad9）を正規化。
        if (key is >= Key.D0 and <= Key.D9)
        {
            int digit = key - Key.D0;
            return digit == 0 ? null : (char)('0' + digit);
        }

        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            int digit = key - Key.NumPad0;
            return digit == 0 ? null : (char)('0' + digit);
        }

        if (key is >= Key.A and <= Key.Z)
        {
            return (char)('A' + (key - Key.A));
        }

        return null;
    }

    /// <summary>
    /// 表示順インデックス（0始まり）に対応するホットキーバッジ文字を取得する.
    /// </summary>
    /// <param name="index">ブラウザの表示順インデックス（0始まり）.</param>
    /// <returns>対応するバッジ文字。範囲外（35個を超える）の場合は <see langword="null"/>.</returns>
    public static char? GetBadgeForIndex(int index)
    {
        if (index < 0 || index >= BadgeSequence.Count)
        {
            return null;
        }

        return BadgeSequence[index];
    }

    private static IReadOnlyList<char> BuildBadgeSequence()
    {
        List<char> sequence = [];
        for (char c = '1'; c <= '9'; c++)
        {
            sequence.Add(c);
        }

        for (char c = 'A'; c <= 'Z'; c++)
        {
            sequence.Add(c);
        }

        return sequence;
    }
}
