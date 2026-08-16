using System.Net.Http;

namespace BrowserSelector.InfrastructureTests.TestDoubles;

/// <summary>
/// 任意のレスポンスを返し、送信されたリクエストを記録する<see cref="HttpMessageHandler"/>のスタブ（Phase H-3）.
/// 実際のGitHub APIを叩くテストは書かない（レート制限と外部依存でCIが不安定になるため）.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    /// <summary>
    /// Gets このハンドラーへ送信されたリクエストの一覧（送信順）.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Requests.Add(request);

        HttpResponseMessage response = _responder(request);

        // 実際のHttpClientはリダイレクト後の最終URLをRequestMessageへ設定する。
        // UpdateServiceはこれを再検証するため、スタブでも同じ形にしておく。
        response.RequestMessage ??= request;

        return Task.FromResult(response);
    }
}
