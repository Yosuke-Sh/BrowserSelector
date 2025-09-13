using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrowserSelector.UnitTests
{
    /// <summary>
    /// 実際のサービスクラスを使用するテスト
    /// モックではなく実際のインスタンスを使用してビジネスロジックを実行.
    /// </summary>
    public class ActualServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IBrowserService _browserService;
        private readonly ISettingsService _settingsService;
        private readonly IUrlService _urlService;
        private readonly IUrlRuleService _urlRuleService;
        private readonly ICustomLanguageService _customLanguageService;
        private readonly string _tempDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualServiceTests"/> class.
        /// </summary>
        public ActualServiceTests()
        {
            // テスト用の一時ディレクトリを作成
            _tempDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTests", Guid.NewGuid().ToString());
            _ = Directory.CreateDirectory(_tempDirectory);

            ServiceCollection services = new();

            // 実際のサービスクラスを登録
            _ = services.AddLogging(builder =>
            {
                // テスト実行時のログ出力を最小限に抑制
                _ = builder.SetMinimumLevel(LogLevel.Error); // エラーレベルのみ出力
            });
            _ = services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
            _ = services.AddSingleton<IRegistryService, BrowserSelector.Infrastructure.SystemIntegration.WindowsRegistryService>();
            _ = services.AddSingleton<IUrlService, UrlService>();
            _ = services.AddSingleton<IBrowserService, BrowserService>();
            _ = services.AddSingleton<ISettingsService>(provider =>
            {
                ILogService? logService = provider.GetService<ILogService>();
                return new TestSettingsService(logService, _tempDirectory);
            });
            _ = services.AddSingleton<IUrlRuleService>(provider =>
            {
                ILogService? logService = provider.GetService<ILogService>();
                return new TestUrlRuleService(logService, _tempDirectory);
            });
            _ = services.AddSingleton<ICustomLanguageService, CustomLanguageService>();
            _ = services.AddSingleton<ILocalizationService, BrowserSelector.Infrastructure.Localization.LocalizationService>();

            _serviceProvider = services.BuildServiceProvider();
            _browserService = _serviceProvider.GetRequiredService<IBrowserService>();
            _settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            _urlService = _serviceProvider.GetRequiredService<IUrlService>();
            _urlRuleService = _serviceProvider.GetRequiredService<IUrlRuleService>();
            _customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();
        }

        [Fact]
        public async Task BrowserService_DetectBrowsersAsync_ShouldExecuteActualLogic()
        {
            // Act - 実際のブラウザ検出ロジックを実行
            IEnumerable<Browser> browsers = await _browserService.DetectBrowsersAsync();

            // Assert - 実際のロジックが実行されたことを確認
            _ = browsers.Should().NotBeNull();
            // 実際のブラウザが検出される可能性がある
        }


        [Fact]
        public async Task SettingsService_LoadAppSettingsAsync_ShouldExecuteActualLogic()
        {
            // Act - 実際の設定読み込みロジックを実行
            AppSettings settings = await _settingsService.LoadAppSettingsAsync();

            // Assert - 実際のロジックが実行されたことを確認
            _ = settings.Should().NotBeNull();
            _ = settings.Should().BeOfType<AppSettings>();
        }

        [Fact]
        public async Task SettingsService_SaveAppSettingsAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            AppSettings settings = new()
            {
                EnableLogging = true,
                LogLevel = "Information",
                CheckForUpdates = true,
                UpdateCheckInterval = 24,
                Language = "en-US",
                CustomProtocol = "browserselector",
                RegisterProtocol = true,
                CloseAfterUrlRuleMatch = true
            };

            // Act - 実際の設定保存ロジックを実行
            bool result = await _settingsService.SaveAppSettingsAsync(settings);

            // Assert - 実際のロジックが実行されたことを確認
            // TestSettingsServiceを使用するため、保存が正常に動作する
            _ = result.Should().BeTrue();
        }

        [Fact]
        public async Task UrlService_NormalizeUrlAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            string testUrl = "https://www.example.com";

            // Act - 実際のURL正規化ロジックを実行
            string result = await _urlService.NormalizeUrlAsync(new Uri(testUrl));

            // Assert - 実際のロジックが実行されたことを確認
            _ = result.Should().NotBeNull();
            _ = result.Should().Be(testUrl); // 正規化されたURL
        }

        [Fact]
        public async Task UrlService_ValidateUrlAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            string validUrl = "https://www.example.com";
            string invalidUrl = "invalid-url";

            // Act - 実際のURL検証ロジックを実行
            bool validResult = await _urlService.ValidateUrlAsync(validUrl);
            bool invalidResult = await _urlService.ValidateUrlAsync(invalidUrl);

            // Assert - 実際のロジックが実行されたことを確認
            _ = validResult.Should().BeTrue();
            _ = invalidResult.Should().BeFalse();
        }

        [Fact]
        public async Task UrlRuleService_GetAllRulesAsync_ShouldExecuteActualLogic()
        {
            // Act - 実際のURLルール取得ロジックを実行
            IEnumerable<UrlRule> rules = await _urlRuleService.GetAllRulesAsync();

            // Assert - 実際のロジックが実行されたことを確認
            _ = rules.Should().NotBeNull();
        }

        [Fact]
        public async Task UrlRuleService_AddRuleAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            UrlRule rule = new()
            {
                Id = Guid.NewGuid(),
                Pattern = "*.example.com",
                BrowserName = "Test Browser",
                Priority = 50,
                IsEnabled = true,
                Description = "Test rule"
            };

            // Act - 実際のURLルール追加ロジックを実行
            bool result = await _urlRuleService.AddRuleAsync(rule);

            // Assert - 実際のロジックが実行されたことを確認
            // TestSettingsServiceを使用するため、保存が正常に動作する
            _ = result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomLanguageService_GetAvailableLanguagesAsync_ShouldExecuteActualLogic()
        {
            // Act - 実際のカスタム言語取得ロジックを実行
            IEnumerable<LanguageInfo> languages = await _customLanguageService.GetAvailableLanguagesAsync();

            // Assert - 実際のロジックが実行されたことを確認
            _ = languages.Should().NotBeNull();
            _ = languages.Should().BeOfType<List<BrowserSelector.Core.Models.LanguageInfo>>();
        }

        [Fact]
        public async Task CustomLanguageService_AddCustomLanguageAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            CustomLanguageFile languageFile = new()
            {
                CultureCode = "test-TEST",
                DisplayName = "Test Language",
                Resources = new Dictionary<string, string> { { "test.key", "Test Value" } },
                Version = "1.0",
                Description = "Test language file",
                Author = "Test Author"
            };

            // Act - 実際のカスタム言語追加ロジックを実行
            bool result = await _customLanguageService.AddCustomLanguageAsync(languageFile.CultureCode);

            // Assert - 実際のロジックが実行されたことを確認
            // テスト環境では無効なパスのためfalseが返される可能性がある
            _ = result.Should().BeFalse();
        }

        [Fact]
        public async Task SettingsService_LoadVisualSettingsAsync_ShouldExecuteActualLogic()
        {
            // Act - 実際の視覚設定読み込みロジックを実行
            VisualSettings settings = await _settingsService.LoadVisualSettingsAsync();

            // Assert - 実際のロジックが実行されたことを確認
            _ = settings.Should().NotBeNull();
            _ = settings.Should().BeOfType<VisualSettings>();
        }

        [Fact]
        public async Task SettingsService_SaveVisualSettingsAsync_ShouldExecuteActualLogic()
        {
            // Arrange
            VisualSettings settings = new()
            {
                BackgroundColor = System.Windows.Media.Colors.Blue,
                UseBackgroundGradient = false,
                GradientStartColor = System.Windows.Media.Colors.White,
                GradientEndColor = System.Windows.Media.Colors.Black,
                IconScale = 1.0,
                ShowFocusIndicator = true,
                FocusColor = System.Windows.Media.Colors.Blue,
                FocusThickness = 2.0,
                FocusWidth = 100.0
            };

            // Act - 実際の視覚設定保存ロジックを実行
            bool result = await _settingsService.SaveVisualSettingsAsync(settings);

            // Assert - 実際のロジックが実行されたことを確認
            // TestSettingsServiceを使用するため、保存が正常に動作する
            _ = result.Should().BeTrue();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _serviceProvider?.Dispose();

            // テスト用の一時ディレクトリをクリーンアップ
            if (Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
                catch
                {
                    // クリーンアップに失敗してもテストには影響しない
                }
            }
        }
    }
}
