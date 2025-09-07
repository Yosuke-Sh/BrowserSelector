// <copyright file="ICustomLanguageService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// カスタム言語ファイル管琁E��ービスのインターフェース
/// </summary>
public interface ICustomLanguageService
{
    /// <summary>
    /// 利用可能な言語一覧を取征E
    /// </summary>
    Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync();

    /// <summary>
    /// カスタム言語ファイルを追加
    /// </summary>
    Task<bool> AddCustomLanguageAsync(string languageFilePath);

    /// <summary>
    /// カスタム言語を削除
    /// </summary>
    Task<bool> RemoveCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// 言語ファイルの検証
    /// </summary>
    Task<bool> ValidateLanguageFileAsync(string languageFilePath);

    /// <summary>
    /// カスタム言語フォルダのパスを取征E
    /// </summary>
    string GetCustomLanguageFolder();

    /// <summary>
    /// カスタム言語ファイルの読み込み
    /// </summary>
    Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// カスタム言語ファイルの保孁E
    /// </summary>
    Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources);

    /// <summary>
    /// 言語ファイルチE��プレートを生�E
    /// </summary>
    Task<bool> GenerateLanguageTemplateAsync(string cultureCode, string displayName);

    /// <summary>
    /// 利用可能なリソースキーの一覧を取征E
    /// </summary>
    Task<IEnumerable<string>> GetAvailableResourceKeysAsync();
}
