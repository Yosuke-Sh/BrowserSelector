// <copyright file="IShellCloseService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Services;

/// <summary>
/// ブラウザ起動後のアプリ終了要求を、トレイ常駐設定に応じて
/// 「完全終了」か「トレイ格納」へ振り分けるための抽象化.
/// Presentation層からApp層のトレイ管理コンポーネントへ直接依存しないために設ける.
/// </summary>
public interface IShellCloseService
{
    /// <summary>
    /// Gets a value indicating whether トレイ常駐が有効かつ、現在トレイへ格納可能な状態かどうか.
    /// </summary>
    bool CanMinimizeToTray { get; }

    /// <summary>
    /// アプリを閉じる。トレイ常駐が有効な場合はトレイへ格納し、それ以外は完全終了する.
    /// </summary>
    void RequestClose();
}
