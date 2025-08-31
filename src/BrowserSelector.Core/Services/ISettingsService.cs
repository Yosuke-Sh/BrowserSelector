using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 設定管理サービスのインターフェース
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// アプリケーション設定を読み込み
    /// </summary>
    Task<AppSettings> LoadAppSettingsAsync();

    /// <summary>
    /// アプリケーション設定を保存
    /// </summary>
    Task<bool> SaveAppSettingsAsync(AppSettings settings);

    /// <summary>
    /// 視覚設定を読み込み
    /// </summary>
    Task<VisualSettings> LoadVisualSettingsAsync();

    /// <summary>
    /// 視覚設定を保存
    /// </summary>
    Task<bool> SaveVisualSettingsAsync(VisualSettings settings);

    /// <summary>
    /// 設定をリセット
    /// </summary>
    Task<bool> ResetSettingsAsync();

    /// <summary>
    /// 設定ファイルのパスを取得
    /// </summary>
    string GetSettingsFilePath();

    /// <summary>
    /// 設定が存在するかどうかを確認
    /// </summary>
    Task<bool> SettingsExistAsync();
}
