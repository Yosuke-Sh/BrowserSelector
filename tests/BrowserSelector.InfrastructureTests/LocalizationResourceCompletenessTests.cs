// <copyright file="LocalizationResourceCompletenessTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text.Json;
using BrowserSelector.Core.Models;
using BrowserSelector.Infrastructure.Localization;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// 埋め込みリソースであるja-JP.json/en-US.json/zh-CN.jsonのキー集合が常に一致することを検証する。
/// 過去にen-US.jsonのキー欠落によるアクセシビリティ設定表示不具合（コミット3e077fa）があったため、
/// 新規キー追加時の欠落を機械的に検出する目的で設けている.
/// </summary>
public class LocalizationResourceCompletenessTests
{
    private static readonly string[] CultureCodes = ["ja-JP", "en-US", "zh-CN"];

    [Fact]
    public void AllLocalizationFiles_HaveIdenticalResourceKeySets()
    {
        Dictionary<string, HashSet<string>> keysByCulture = CultureCodes.ToDictionary(
            culture => culture,
            LoadResourceKeys);

        HashSet<string> baseline = keysByCulture["ja-JP"];

        foreach (string culture in CultureCodes.Where(c => c != "ja-JP"))
        {
            HashSet<string> keys = keysByCulture[culture];
            IEnumerable<string> missing = baseline.Except(keys);
            IEnumerable<string> extra = keys.Except(baseline);

            missing.Should().BeEmpty($"{culture}.json はja-JP.jsonに存在するキーを全て含む必要がある");
            extra.Should().BeEmpty($"{culture}.json にja-JP.jsonへ存在しない余分なキーが無い必要がある");
        }
    }

    [Theory]
    [InlineData("ja-JP")]
    [InlineData("en-US")]
    [InlineData("zh-CN")]
    public void LocalizationFile_ContainsNewWindowSizeAndTileElevationKeys(string cultureCode)
    {
        HashSet<string> keys = LoadResourceKeys(cultureCode);

        keys.Should().Contain("Settings.Display.CaptureCurrentWindowSize");
        keys.Should().Contain("Settings.Display.CaptureCurrentWindowSizeDescription");
        keys.Should().Contain("Settings.Display.TileElevationStyle");
        keys.Should().Contain("Settings.App.DefaultBrowserSettings");
        keys.Should().Contain("Settings.App.SetAsDefaultBrowser");
    }

    private static HashSet<string> LoadResourceKeys(string cultureCode)
    {
        Assembly assembly = typeof(LocalizationService).Assembly;
        string resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        stream.Should().NotBeNull($"埋め込みリソース {resourceName} が見つかりません");

        using StreamReader reader = new(stream!);
        string json = reader.ReadToEnd();

        CustomLanguageFile? languageFile = JsonSerializer.Deserialize<CustomLanguageFile>(json);
        languageFile.Should().NotBeNull();

        return [.. languageFile!.Resources.Keys];
    }
}
