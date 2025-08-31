using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// プロトコルハンドラーを管理するサービスのインターフェース
/// </summary>
public interface IProtocolHandler
{
    /// <summary>
    /// プロトコルを登録
    /// </summary>
    /// <param name="applicationPath">アプリケーションのパス</param>
    /// <returns>登録が成功したかどうか</returns>
    bool RegisterProtocol(string applicationPath);

    /// <summary>
    /// プロトコルを登録解除
    /// </summary>
    /// <returns>登録解除が成功したかどうか</returns>
    bool UnregisterProtocol();

    /// <summary>
    /// プロトコルが登録されているかチェック
    /// </summary>
    /// <returns>登録されているかどうか</returns>
    bool IsProtocolRegistered();

    /// <summary>
    /// プロトコルURLからパラメータを抽出
    /// </summary>
    /// <param name="protocolUrl">プロトコルURL</param>
    /// <returns>抽出されたURL</returns>
    string? ExtractUrlFromProtocol(string protocolUrl);

    /// <summary>
    /// プロトコルURLを生成
    /// </summary>
    /// <param name="url">元のURL</param>
    /// <returns>プロトコルURL</returns>
    string CreateProtocolUrl(string url);

    /// <summary>
    /// プロトコル登録情報を取得
    /// </summary>
    /// <returns>登録情報</returns>
    ProtocolRegistrationInfo? GetProtocolRegistrationInfo();
}
