using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Updates;
using BrowserSelector.InfrastructureTests.TestDoubles;
using FluentAssertions;
using Moq;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-3: <see cref="UpdateService.CheckForUpdatesAsync"/>のテスト.
/// 実際のGitHub APIは叩かず、本番と同じ経路（AddHttpClient）へスタブハンドラーを差し込む.
/// </summary>
public sealed class UpdateServiceCheckTests : IDisposable
{
    private readonly string _stateDirectory;
    private readonly string _statePath;

    public UpdateServiceCheckTests()
    {
        _stateDirectory = Path.Combine(Path.GetTempPath(), $"BSUpdateCheckTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_stateDirectory);
        _statePath = Path.Combine(_stateDirectory, "etag.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_stateDirectory))
            {
                Directory.Delete(_stateDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // テスト用の一時ディレクトリなので削除失敗は無視してよい。
        }
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnUpdateWhenRemoteVersionIsNewer()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        UpdateInfo? result = await service.CheckForUpdatesAsync();

        result.Should().NotBeNull();
        result!.Version.Should().Be(new Version(0, 9, 0));
        result.TagName.Should().Be("v0.9.0");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullWhenVersionsAreEqual()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.3.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullWhenRemoteVersionIsOlder()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.1.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldRaiseUpdateAvailableEvent()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        UpdateInfo? raised = null;
        service.UpdateAvailable += (_, e) => raised = e.UpdateInfo;

        _ = await service.CheckForUpdatesAsync();

        raised.Should().NotBeNull();
        raised!.TagName.Should().Be("v0.9.0");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldSendGitHubHeaders()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0), configureClient: true);

        _ = await service.CheckForUpdatesAsync();

        HttpRequestMessage request = handler.Requests.Should().ContainSingle().Subject;
        request.Headers.UserAgent.ToString().Should().Contain("BrowserSelector");
        request.Headers.Accept.ToString().Should().Contain("application/vnd.github+json");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldStoreETagAndSendItOnNextCall()
    {
        // 現在バージョンが既に最新版と同じ（＝更新は無い）場合は、キャッシュされたタグ名が
        // 現在バージョンより新しくないため、通常どおり2回目以降はETagを送って304運用へ入る。
        using var handler = new StubHttpMessageHandler(_ =>
        {
            HttpResponseMessage response = JsonResponse(ReleaseJson("v0.3.0"));
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"abc123\"");
            return response;
        });
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        _ = await service.CheckForUpdatesAsync();
        _ = await service.CheckForUpdatesAsync();

        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].Headers.Contains("If-None-Match").Should().BeFalse();
        handler.Requests[1].Headers.GetValues("If-None-Match").Should().Contain("\"abc123\"");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullOnNotModified()
    {
        // 304はレート制限を消費しないので、更新なしとして即座に戻る。
        using var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotModified));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldKeepReturningUpdateOnRepeatedChecksWhenNotYetApplied()
    {
        // 1回目: 新版を検出してETagをキャッシュする（ユーザーがまだ適用していない状況を模す）。
        // 2回目以降: GitHub側は同じリリースのため304を返すはずだが、キャッシュされたタグ名が
        // 現在バージョンより新しいままなので、ETagを送らずフルリクエストへフォールバックし、
        // 「最新の状態です」という誤った判定にならないことを確認する回帰テスト。
        int requestCount = 0;
        using var handler = new StubHttpMessageHandler(request =>
        {
            requestCount++;
            if (requestCount == 1)
            {
                HttpResponseMessage first = JsonResponse(ReleaseJson("v0.9.0"));
                first.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"abc123\"");
                return first;
            }

            // 2回目以降、If-None-Matchが付いていれば304を返す通常のGitHub API挙動を模す。
            if (request.Headers.Contains("If-None-Match"))
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            HttpResponseMessage subsequent = JsonResponse(ReleaseJson("v0.9.0"));
            subsequent.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"abc123\"");
            return subsequent;
        });
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        UpdateInfo? first = await service.CheckForUpdatesAsync();
        UpdateInfo? second = await service.CheckForUpdatesAsync();

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        second!.TagName.Should().Be("v0.9.0");

        // アプリ未更新のままの間はETagを送らないため、2リクエストとも If-None-Match なしで届く。
        handler.Requests.Should().HaveCount(2);
        handler.Requests[1].Headers.Contains("If-None-Match").Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullOnNotModifiedAfterUpdateIsApplied()
    {
        // キャッシュされたタグ名と同じバージョンで起動した（＝適用済み）場合は、
        // 通常どおりETagを送って304を受け入れ、レート制限を節約する。
        using var handler = new StubHttpMessageHandler(request =>
        {
            if (request.Headers.Contains("If-None-Match"))
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            HttpResponseMessage response = JsonResponse(ReleaseJson("v0.9.0"));
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"abc123\"");
            return response;
        });

        // 1回目はv0.3.0として実行し、v0.9.0を検出・ETagをキャッシュさせる。
        using (UpdateService firstRun = CreateService(handler, currentVersion: new Version(0, 3, 0)))
        {
            (await firstRun.CheckForUpdatesAsync()).Should().NotBeNull();
        }

        // 2回目はv0.9.0（適用済み）として実行する。
        using UpdateService secondRun = CreateService(handler, currentVersion: new Version(0, 9, 0));
        (await secondRun.CheckForUpdatesAsync()).Should().BeNull();

        handler.Requests.Should().HaveCount(2);
        handler.Requests[1].Headers.Contains("If-None-Match").Should().BeTrue();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldSuppressSubsequentCallsWhenRateLimited()
    {
        long resetAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        using var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            response.Headers.Add("X-RateLimit-Remaining", "0");
            response.Headers.Add("X-RateLimit-Reset", resetAt.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return response;
        });
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();

        // 2回目はリセット時刻まで通信自体を行わない。
        (await service.CheckForUpdatesAsync()).Should().BeNull();
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullOnNetworkFailureWithoutThrowing()
    {
        // ネットワーク断は無害な状況なので、例外を投げず静かにnullを返す契約。
        using var handler = new StubHttpMessageHandler(_ => throw new HttpRequestException("no network"));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        Func<Task> act = async () => (await service.CheckForUpdatesAsync()).Should().BeNull();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullOnServerError()
    {
        using var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullOnMalformedJson()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse("{ not json"));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldExcludePrereleaseByDefault()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0", prerelease: true)));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldIncludePrereleaseWhenSettingEnabled()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0", prerelease: true)));
        using UpdateService service = CreateService(
            handler,
            currentVersion: new Version(0, 3, 0),
            settings: new AppSettings { IncludePrereleases = true });

        UpdateInfo? result = await service.CheckForUpdatesAsync();

        result.Should().NotBeNull();
        result!.IsPrerelease.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullForDraftRelease()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0", draft: true)));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));

        (await service.CheckForUpdatesAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnNullWhenCanceled()
    {
        using var handler = new StubHttpMessageHandler(_ => JsonResponse(ReleaseJson("v0.9.0")));
        using UpdateService service = CreateService(handler, currentVersion: new Version(0, 3, 0));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        (await service.CheckForUpdatesAsync(cts.Token)).Should().BeNull();
    }

    private static string ReleaseJson(string tag, bool prerelease = false, bool draft = false) => $$"""
    {
      "tag_name": "{{tag}}",
      "name": "BrowserSelector {{tag}}",
      "body": "release notes",
      "draft": {{(draft ? "true" : "false")}},
      "prerelease": {{(prerelease ? "true" : "false")}},
      "published_at": "2026-08-16T12:00:00Z",
      "html_url": "https://github.com/Yosuke-Sh/BrowserSelector/releases/tag/{{tag}}",
      "assets": [
        {
          "name": "BrowserSelector-Setup-{{tag}}.exe",
          "browser_download_url": "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/{{tag}}/BrowserSelector-Setup-{{tag}}.exe",
          "size": 4321
        }
      ]
    }
    """;

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private UpdateService CreateService(
        HttpMessageHandler handler,
        Version currentVersion,
        AppSettings? settings = null,
        bool configureClient = false)
    {
        var settingsService = new Mock<ISettingsService>();
        _ = settingsService.Setup(s => s.LoadAppSettingsAsync())
            .ReturnsAsync(settings ?? new AppSettings());

        IHttpClientFactory factory = configureClient
            ? TestHttpClientFactory.CreateWithGitHubHeaders(UpdateService.HttpClientName, handler)
            : TestHttpClientFactory.Create(UpdateService.HttpClientName, handler);

        return new UpdateService(
            factory,
            settingsService.Object,
            Mock.Of<ILogService>(),
            _statePath,
            currentVersion,
            _stateDirectory,
            new RecordingProcessLauncher());
    }
}
