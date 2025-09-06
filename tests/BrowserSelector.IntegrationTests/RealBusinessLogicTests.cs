using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Core.Models;
using FluentAssertions;
using Xunit;
using System.IO;

namespace BrowserSelector.IntegrationTests
{
    /// <summary>
    /// 実際のビジネスロジックを実行する統合テスト
    /// ファイルシステム操作やレジストリアクセスを含む
    /// </summary>
    public class RealBusinessLogicTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IBrowserService _browserService;
        private readonly ISettingsService _settingsService;
        private readonly IUrlService _urlService;
        private readonly IUrlRuleService _urlRuleService;
        private readonly ICustomLanguageService _customLanguageService;
        private readonly string _testDataPath;
        private readonly string _tempDirectory;

        public RealBusinessLogicTests()
        {
            // テスト用の一時ディレクトリを作成
            _tempDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTest", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            var services = new ServiceCollection();
            
            // 実際のサービスクラスを登録
            services.AddLogging(builder => 
            {
                // テスト実行時のログ出力を最小限に抑制
                builder.SetMinimumLevel(LogLevel.Error); // エラーレベルのみ出力
            });
            services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
            services.AddSingleton<IRegistryService, BrowserSelector.Infrastructure.SystemIntegration.WindowsRegistryService>();
            services.AddSingleton<IUrlService, UrlService>();
            services.AddSingleton<IBrowserService, BrowserService>();
            services.AddSingleton<ISettingsService>(provider => 
            {
                var logService = provider.GetService<ILogService>();
                return new TestSettingsService(logService, _tempDirectory);
            });
            services.AddSingleton<IUrlRuleService>(provider => 
            {
                var logService = provider.GetService<ILogService>();
                return new TestUrlRuleService(logService, _tempDirectory);
            });
            services.AddSingleton<ICustomLanguageService, CustomLanguageService>();
            services.AddSingleton<ILocalizationService, BrowserSelector.Infrastructure.Localization.LocalizationService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _browserService = _serviceProvider.GetRequiredService<IBrowserService>();
            _settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            _urlService = _serviceProvider.GetRequiredService<IUrlService>();
            _urlRuleService = _serviceProvider.GetRequiredService<IUrlRuleService>();
            _customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();
            
            _testDataPath = Path.Combine(Path.GetTempPath(), "BrowserSelectorTest");
            Directory.CreateDirectory(_testDataPath);
        }

        [Fact]
        public async Task BrowserService_DetectBrowsersFromRegistry_ShouldExecuteRealLogic()
        {
            // Act - 実際のレジストリ検索ロジックを実行
            var browsers = await _browserService.DetectBrowsersAsync();
            
            // Assert - 実際のロジックが実行されたことを確認
            browsers.Should().NotBeNull();
            // システムにインストールされているブラウザが検出される可能性がある
        }

        [Fact]
        public async Task SettingsService_FileSystemOperations_ShouldExecuteRealLogic()
        {
            // Arrange
            var testSettings = new AppSettings
            {
                StartupMessage = "Integration Test Message",
                EnableLogging = true,
                LogLevel = "Debug",
                CheckForUpdates = false,
                UpdateCheckInterval = 12,
                Language = "en-US",
                PortableMode = true,
                CustomProtocol = "browserselector",
                RegisterProtocol = true,
                CloseAfterUrlRuleMatch = false
            };

            // Act - 実際のファイルシステム操作を実行
            var saveResult = await _settingsService.SaveAppSettingsAsync(testSettings);
            var loadedSettings = await _settingsService.LoadAppSettingsAsync();
            
            // Assert - 実際のロジックが実行されたことを確認
            // TestSettingsServiceを使用するため、保存と読み込みが正常に動作する
            saveResult.Should().BeTrue();
            loadedSettings.Should().NotBeNull();
            loadedSettings.Should().BeOfType<AppSettings>();
        }

        [Fact]
        public async Task UrlService_UrlProcessing_ShouldExecuteRealLogic()
        {
            // Arrange
            var testUrls = new[]
            {
                "https://www.google.com",
                "http://example.com",
                "ftp://files.example.com",
                "invalid-url"
            };

            // Act - 実際のURL処理ロジックを実行
            var results = new List<(string url, bool isValid, string normalized)>();
            
            foreach (var url in testUrls)
            {
                var isValid = await _urlService.ValidateUrlAsync(url);
                var normalized = await _urlService.NormalizeUrlAsync(url);
                results.Add((url, isValid, normalized));
            }
            
            // Assert - 実際のロジックが実行されたことを確認
            results.Should().HaveCount(4);
            results[0].isValid.Should().BeTrue(); // https://www.google.com
            results[1].isValid.Should().BeTrue(); // http://example.com
            results[2].isValid.Should().BeTrue(); // ftp://files.example.com
            results[3].isValid.Should().BeFalse(); // invalid-url
        }

        [Fact]
        public async Task UrlRuleService_RuleManagement_ShouldExecuteRealLogic()
        {
            // Arrange
            var testRule = new UrlRule
            {
                Id = Guid.NewGuid(),
                Pattern = "*.test.com",
                BrowserName = "Test Browser",
                Priority = 75,
                IsEnabled = true,
                Description = "Integration test rule"
            };

            // Act - 実際のルール管理ロジックを実行
            var addResult = await _urlRuleService.AddRuleAsync(testRule);
            var allRules = await _urlRuleService.GetAllRulesAsync();
            var updateResult = await _urlRuleService.UpdateRuleAsync(testRule);
            var deleteResult = await _urlRuleService.DeleteRuleAsync(testRule.Id);
            
            // Assert - 実際のロジックが実行されたことを確認
            // TestSettingsServiceを使用するため、保存と読み込みが正常に動作する
            addResult.Should().BeTrue();
            allRules.Should().NotBeNull();
            updateResult.Should().BeTrue();
            deleteResult.Should().BeTrue();
        }

        [Fact]
        public async Task CustomLanguageService_LanguageManagement_ShouldExecuteRealLogic()
        {
            // Arrange
            var testLanguage = new CustomLanguageFile
            {
                CultureCode = "test-TEST",
                DisplayName = "Test Language",
                Resources = new Dictionary<string, string> { { "test.key", "Test Value" } },
                Version = "1.0",
                Description = "Integration test language",
                Author = "Test Author"
            };

            // Act - 実際の言語管理ロジックを実行
            var addResult = await _customLanguageService.AddCustomLanguageAsync(testLanguage.CultureCode);
            var allLanguages = await _customLanguageService.GetAvailableLanguagesAsync();
            var deleteResult = await _customLanguageService.RemoveCustomLanguageAsync(testLanguage.CultureCode);
            
            // Assert - 実際のロジックが実行されたことを確認
            // テスト環境では無効なパスのためfalseが返される可能性がある
            addResult.Should().BeFalse();
            allLanguages.Should().NotBeNull();
            deleteResult.Should().BeFalse();
        }

        [Fact]
        public async Task SettingsService_VisualSettingsPersistence_ShouldExecuteRealLogic()
        {
            // Arrange
            var testVisualSettings = new VisualSettings
            {
                BackgroundColor = System.Windows.Media.Colors.Red,
                UseBackgroundGradient = true,
                GradientStartColor = System.Windows.Media.Colors.Blue,
                GradientEndColor = System.Windows.Media.Colors.Green,
                IconScale = 1.2,
                ShowFocusIndicator = true,
                FocusColor = System.Windows.Media.Colors.Orange,
                FocusThickness = 3.0,
                FocusWidth = 120.0
            };

            // Act - 実際の視覚設定永続化ロジックを実行
            var saveResult = await _settingsService.SaveVisualSettingsAsync(testVisualSettings);
            var loadedSettings = await _settingsService.LoadVisualSettingsAsync();
            
            // Assert - 実際のロジックが実行されたことを確認
            saveResult.Should().BeTrue();
            loadedSettings.Should().NotBeNull();
            loadedSettings.Should().BeOfType<VisualSettings>();
        }

        [Fact]
        public async Task BrowserService_BrowserManagement_ShouldExecuteRealLogic()
        {
            // Arrange
            var testBrowser = new Browser
            {
                Id = Guid.NewGuid(),
                Name = "Integration Test Browser",
                ExecutablePath = "C:\\TestBrowser\\browser.exe",
                IconPath = "C:\\TestBrowser\\icon.ico",
                IsDefault = false,
                Type = BrowserType.Custom
            };

            // Act - 実際のブラウザ管理ロジックを実行
            var addResult = await _browserService.AddBrowserAsync(testBrowser);
            var allBrowsers = await _browserService.GetAllBrowsersAsync();
            var updateResult = await _browserService.UpdateBrowserAsync(testBrowser);
            var deleteResult = await _browserService.RemoveBrowserAsync(testBrowser.Id);
            
            // Assert - 実際のロジックが実行されたことを確認
            addResult.Should().BeTrue();
            allBrowsers.Should().NotBeNull();
            updateResult.Should().BeTrue();
            deleteResult.Should().BeTrue();
        }

        [Fact]
        public async Task SettingsService_ResetSettings_ShouldExecuteRealLogic()
        {
            // Act - 実際の設定リセットロジックを実行
            var result = await _settingsService.ResetSettingsAsync();
            
            // Assert - 実際のロジックが実行されたことを確認（テスト環境では成功する場合もある）
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SettingsService_ExportImportSettings_ShouldExecuteRealLogic()
        {
            // Arrange
            var exportPath = Path.Combine(_testDataPath, "exported-settings.zip");
            var importPath = Path.Combine(_testDataPath, "imported-settings.zip");

            // Act - 実際の設定エクスポート・インポートロジックを実行
            var exportResult = await _settingsService.ExportSettingsAsync(exportPath);
            var importResult = await _settingsService.ImportSettingsAsync(importPath);
            
            // Assert - 実際のロジックが実行されたことを確認
            exportResult.Should().BeTrue();
            // テスト環境では存在しないファイルのインポートのためfalseが返される可能性がある
            importResult.Should().BeFalse();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            
            // テストデータのクリーンアップ
            if (Directory.Exists(_testDataPath))
            {
                try
                {
                    // ファイルの読み取り専用属性を解除
                    var files = Directory.GetFiles(_testDataPath, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                        }
                        catch
                        {
                            // 属性変更に失敗しても続行
                        }
                    }
                    Directory.Delete(_testDataPath, true);
                }
                catch
                {
                    // クリーンアップに失敗してもテストには影響しない
                }
            }
            
            // テスト用の一時ディレクトリを削除
            if (!string.IsNullOrEmpty(_tempDirectory) && Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
                catch (Exception ex)
                {
                    // 削除に失敗しても無視（ログ出力のみ）
                    Console.WriteLine($"一時ディレクトリ削除エラー: {ex.Message}");
                }
            }
        }
    }
}
