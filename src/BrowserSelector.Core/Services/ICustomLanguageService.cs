// <copyright file="ICustomLanguageService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// カスタム言語ファイル管理サービスのインターフェース.
/// </summary>
public interface ICustomLanguageService
{
    /// <summary>
    /// 利用可能な言語一覧を取得.
    /// </summary>
    /// <returns>利用可能な言語の一覧.</returns>
    Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync();

    /// <summary>
    /// カスタム言語ファイルを追加.
    /// </summary>
    /// <returns>追加が成功したかどうか.</returns>
    Task<bool> AddCustomLanguageAsync(string languageFilePath);

    /// <summary>
    /// カスタム言語を削除.
    /// </summary>
    /// <returns>削除が成功したかどうか.</returns>
    Task<bool> RemoveCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// 言語ファイルの検証.
    /// </summary>
    /// <returns>検証が成功したかどうか.</returns>
    Task<bool> ValidateLanguageFileAsync(string languageFilePath);

    /// <summary>
    /// カスタム言語フォルダのパスを取得.
    /// </summary>
    /// <returns></returns>
    string GetCustomLanguageFolder();

    /// <summary>
    /// カスタム言語ファイルの読み込み.
    /// </summary>
    /// <returns>読み込まれた言語リソース（失敗時はnull）.</returns>
    Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode);

    /// <summary>
    /// カスタム言語ファイルの保存.
    /// </summary>
    /// <returns>保存が成功したかどうか.</returns>
    Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources);

    /// <summary>
    /// 言語ファイルテンプレートを生成.
    /// </summary>
    /// <returns>生成が成功したかどうか.</returns>
    Task<bool> GenerateLanguageTemplateAsync(string cultureCode, string displayName);

    /// <summary>
    /// 利用可能なリソースキーの一覧を取得.
    /// </summary>
    /// <returns>リソースキーの一覧.</returns>
    Task<IEnumerable<string>> GetAvailableResourceKeysAsync();
}
