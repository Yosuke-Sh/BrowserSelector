namespace BrowserSelector.Core.Services;

/// <summary>
/// URL処理サービスのインターフェース
/// </summary>
public interface IUrlService
{
    /// <summary>
    /// URLを正規化する
    /// </summary>
    /// <param name="url">正規化するURL</param>
    /// <returns>正規化されたURL</returns>
    Task<string> NormalizeUrlAsync(string url);



    /// <summary>
    /// URLが有効かどうかを検証する
    /// </summary>
    /// <param name="url">検証するURL</param>
    /// <returns>有効な場合はtrue</returns>
    Task<bool> ValidateUrlAsync(string url);

    /// <summary>
    /// URLからドメインを抽出する
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>ドメイン名</returns>
    string ExtractDomain(string url);

    /// <summary>
    /// プロトコルを追加する（必要に応じて）
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>プロトコルが追加されたURL</returns>
    string AddProtocolIfNeeded(string url);
}
