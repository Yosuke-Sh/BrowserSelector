using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BrowserSelector.InfrastructureTests.TestDoubles;

/// <summary>
/// 本番と同じ経路（AddHttpClient + ConfigurePrimaryHttpMessageHandler）で
/// <see cref="IHttpClientFactory"/>を組み立てるテストヘルパー（Phase H-3）.
/// </summary>
internal static class TestHttpClientFactory
{
    /// <summary>
    /// 指定ハンドラーを使う名前付きクライアントを登録した<see cref="IHttpClientFactory"/>を生成する.
    /// </summary>
    /// <param name="name">クライアント名.</param>
    /// <param name="handler">プライマリハンドラー.</param>
    /// <returns>ファクトリ.</returns>
    public static IHttpClientFactory Create(string name, HttpMessageHandler handler)
    {
        ServiceCollection services = new();

        _ = services.AddHttpClient(name)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
    }

    /// <summary>
    /// 本番のDI登録（ServiceCollectionExtensions）と同じヘッダー設定を施したファクトリを生成する.
    /// UpdateServiceが正しいUser-Agent/Acceptで送信することを検証するために使う.
    /// </summary>
    /// <param name="name">クライアント名.</param>
    /// <param name="handler">プライマリハンドラー.</param>
    /// <returns>ファクトリ.</returns>
    public static IHttpClientFactory CreateWithGitHubHeaders(string name, HttpMessageHandler handler)
    {
        ServiceCollection services = new();

        _ = services.AddHttpClient(name, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"BrowserSelector/{BrowserSelector.Core.AppInfo.CurrentVersion}");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
    }
}
