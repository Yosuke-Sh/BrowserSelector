# BrowserSelector WPF + MVVM 開発計画書

## 📋 開発計画概要

このドキュメントは、BrowserSelector WPF + MVVMアプリケーションの詳細な開発計画を記載しています。プロジェクトルール（`.cursor/rules/global.mdc`）と併せて参照してください。

## 🏃‍♂️ 開発フェーズ詳細

### Phase 1: 基盤構築（2-3週間）

#### 🎯 目標
- プロジェクトの基盤となるアーキテクチャの構築
- 開発環境の整備
- 基本的なMVVMパターンの実装

#### 📋 実装内容

**Week 1: プロジェクト構成とDI設定**
- [x] ソリューション構成の作成
- [x] プロジェクト参照の設定
- [x] NuGetパッケージの追加
- [x] 基本的なディレクトリ構造の作成
- [x] DIコンテナの設定

**Week 2: 基本的なMVVMアーキテクチャ**
- [x] Coreプロジェクトの基本モデル作成
- [x] サービスインターフェースの定義
- [x] PresentationプロジェクトのViewModel作成
- [x] 基本的なViewの実装
- [x] コンバーターの実装

**Week 3: テストフレームワークと多言語対応基盤**
- [ ] 単体テストフレームワークの設定
- [ ] 多言語対応基盤の実装
- [ ] CI/CDパイプラインの構築
- [ ] 基本的なテストケースの作成

#### ✅ 成果物
- プロジェクトテンプレート
- 基本的なViewModel/View
- テスト自動化環境
- 多言語リソースファイル

#### 🔧 技術的詳細

**プロジェクト構成**
```
BrowserSelector.WPF/
├── src/
│   ├── BrowserSelector.Core/           # ドメイン層
│   │   ├── Models/
│   │   │   ├── Browser.cs
│   │   │   ├── AppSettings.cs
│   │   │   └── VisualSettings.cs
│   │   ├── Services/
│   │   │   ├── IBrowserService.cs
│   │   │   ├── ISettingsService.cs
│   │   │   └── ILocalizationService.cs
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Extensions/
│   ├── BrowserSelector.Infrastructure/ # インフラ層
│   ├── BrowserSelector.Presentation/   # プレゼンテーション層
│   └── BrowserSelector.App/           # アプリケーション層
└── tests/
    ├── BrowserSelector.UnitTests/
    ├── BrowserSelector.IntegrationTests/
    ├── BrowserSelector.UITests/
    └── BrowserSelector.E2ETests/
```

**DI設定例**
```csharp
// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IBrowserService, BrowserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        
        // Infrastructure Services
        services.AddScoped<IRegistryService, WindowsRegistryService>();
        services.AddScoped<ISystemTrayService, SystemTrayService>();
        
        // Presentation Services
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        return services;
    }
}
```

### Phase 2: コア機能（3-4週間）

#### 🎯 目標
- ブラウザ検出・管理機能の実装
- 基本的なUI（ブラウザ選択画面）の完成
- 設定管理システムの構築

#### 📋 実装内容

**Week 1: ブラウザ検出機能**
- [ ] Windowsレジストリからのブラウザ検出
- [ ] ファイルシステムからのブラウザ検出
- [ ] ブラウザ情報の正規化
- [ ] アイコン読み込み機能

**Week 2: ブラウザ管理機能**
- [ ] ブラウザの追加・編集・削除
- [ ] デフォルトブラウザ設定
- [ ] ブラウザ起動機能
- [ ] 使用統計の記録

**Week 3: 設定管理システム**
- [ ] XML設定ファイルの実装
- [ ] 設定の保存・読み込み
- [ ] 設定画面の基本UI
- [ ] 設定の検証機能

**Week 4: URL処理ロジック**
- [ ] URL正規化機能
- [ ] 短縮URL展開機能
- [ ] プロトコルハンドリング
- [ ] コマンドライン引数処理

#### ✅ 成果物
- 動作するブラウザ選択機能
- 設定保存・読み込み
- コア機能の包括的テスト

#### 🔧 技術的詳細

**ブラウザ検出ロジック**
```csharp
public class BrowserDetectionService : IBrowserDetectionService
{
    public async Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        var browsers = new List<Browser>();
        
        // Chrome検出
        browsers.AddRange(await DetectChromeAsync());
        
        // Firefox検出
        browsers.AddRange(await DetectFirefoxAsync());
        
        // Edge検出
        browsers.AddRange(await DetectEdgeAsync());
        
        // カスタムブラウザ検出
        browsers.AddRange(await DetectCustomBrowsersAsync());
        
        return browsers.OrderBy(b => b.DisplayOrder);
    }
}
```

**設定管理システム**
```csharp
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly ILogger<SettingsService> _logger;
    
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();
                
            var json = await File.ReadAllTextAsync(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定の読み込みに失敗しました");
            return new AppSettings();
        }
    }
}
```

### Phase 3: 高度なUI機能（3-4週間）

#### 🎯 目標
- 透明化・視覚効果の実装
- アクセシビリティ機能の実装
- カスタマイズ可能な設定画面の完成

#### 📋 実装内容

**Week 1: 透明化・視覚効果**
- [ ] ウィンドウ透明化機能
- [ ] カスタム透明化色設定
- [ ] 角の丸み設定
- [ ] 背景グラデーション

**Week 2: アクセシビリティ機能**
- [ ] キーボードナビゲーション
- [ ] フォーカス管理
- [ ] スクリーンリーダー対応
- [ ] 高コントラスト対応

**Week 3: 設定画面の高度化**
- [ ] タブ構成の設定画面
- [ ] リアルタイムプレビュー
- [ ] 設定のインポート・エクスポート
- [ ] 設定のリセット機能

**Week 4: UIテスト実装**
- [ ] FlaUIを使用したUIテスト
- [ ] アクセシビリティテスト
- [ ] 視覚効果のテスト
- [ ] パフォーマンステスト

#### ✅ 成果物
- 完全な透明化機能
- アクセシブルなUI
- 包括的なUIテスト

#### 🔧 技術的詳細

**透明化ウィンドウ実装**
```csharp
public class TransparentWindow : Window
{
    public static readonly DependencyProperty TransparencyColorProperty =
        DependencyProperty.Register(nameof(TransparencyColor), typeof(Color), typeof(TransparentWindow),
            new PropertyMetadata(Colors.Black, OnTransparencyColorChanged));
    
    public Color TransparencyColor
    {
        get => (Color)GetValue(TransparencyColorProperty);
        set => SetValue(TransparencyColorProperty, value);
    }
    
    private static void OnTransparencyColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransparentWindow window)
        {
            window.UpdateTransparency();
        }
    }
    
    private void UpdateTransparency()
    {
        // 透明化処理の実装
    }
}
```

### Phase 4: システム統合（2-3週間）

#### 🎯 目標
- システムトレイ機能の実装
- プロトコルハンドラーの実装
- 自動アップデート機能の実装

#### 📋 実装内容

**Week 1: システムトレイ機能**
- [ ] システムトレイアイコンの実装
- [ ] コンテキストメニューの実装
- [ ] バルーンティップの実装
- [ ] 最小化時の動作

**Week 2: プロトコルハンドラー**
- [ ] カスタムプロトコル登録
- [ ] プロトコルハンドラーの実装
- [ ] レジストリ操作
- [ ] セキュリティ考慮事項

**Week 3: 自動アップデート機能**
- [ ] 更新チェック機能
- [ ] ダウンロード機能
- [ ] インストール機能
- [ ] ロールバック機能

#### ✅ 成果物
- システム統合機能
- 自動アップデート機能
- 完全なE2Eテスト

#### 🔧 技術的詳細

**システムトレイ実装**
```csharp
public class SystemTrayService : ISystemTrayService
{
    private NotifyIcon? _notifyIcon;
    private readonly ILogger<SystemTrayService> _logger;
    
    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = GetApplicationIcon(),
            Text = "BrowserSelector",
            Visible = true
        };
        
        _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        CreateContextMenu();
    }
    
    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("設定", null, OnSettingsClick);
        contextMenu.Items.Add("終了", null, OnExitClick);
        _notifyIcon.ContextMenuStrip = contextMenu;
    }
}
```

### Phase 5: 最適化・テスト（2-3週間）

#### 🎯 目標
- パフォーマンス最適化
- エラーハンドリング強化
- セキュリティ監査
- リリース準備

#### 📋 実装内容

**Week 1: パフォーマンス最適化**
- [ ] 起動時間の最適化
- [ ] メモリ使用量の最適化
- [ ] UI応答性の改善
- [ ] 非同期処理の最適化

**Week 2: エラーハンドリング強化**
- [ ] 包括的な例外処理
- [ ] ログ機能の強化
- [ ] エラー報告機能
- [ ] 自動復旧機能

**Week 3: セキュリティ・リリース準備**
- [ ] セキュリティ監査
- [ ] コード署名
- [ ] インストーラー作成
- [ ] ドキュメント完成

#### ✅ 成果物
- 本番レディなアプリケーション
- 完全なドキュメント
- インストーラー・配布パッケージ

## 🧪 テスト戦略詳細

### 単体テスト（Unit Tests）
**フレームワーク**: xUnit + Moq + FluentAssertions

```csharp
[Fact]
public async Task BrowserService_DetectBrowsers_ShouldReturnValidBrowsers()
{
    // Arrange
    var mockRegistry = new Mock<IRegistryService>();
    var service = new BrowserService(mockRegistry.Object);
    
    // Act
    var browsers = await service.DetectBrowsersAsync();
    
    // Assert
    browsers.Should().NotBeEmpty();
    browsers.Should().AllSatisfy(b => b.IsValid.Should().BeTrue());
}
```

**テスト対象:**
- ビジネスロジック（BrowserService, SettingsService等）
- ViewModels（プロパティ変更、コマンド実行）
- ユーティリティクラス
- 変換ロジック（Converters）

### 統合テスト（Integration Tests）
**フレームワーク**: xUnit + Microsoft.Extensions.Testing

```csharp
[Fact]
public async Task SettingsService_SaveAndLoad_ShouldPersistCorrectly()
{
    // 設定の保存・読み込みの統合テスト
    // ファイルI/O、XMLシリアライゼーションの確認
}
```

**テスト対象:**
- 設定ファイルの保存・読み込み
- レジストリアクセス
- システム統合機能
- サービス間の連携

### UIテスト（UI Tests）
**フレームワーク**: Microsoft.VisualStudio.TestTools.UnitTesting + FlaUI

```csharp
[TestMethod]
public void MainWindow_ClickBrowser_ShouldLaunchCorrectBrowser()
{
    // UI操作の自動テスト
    using var app = Application.Launch("BrowserSelector.exe");
    var window = app.GetMainWindow();
    
    var browserButton = window.FindFirstDescendant(cf => cf.ByName("Chrome"));
    browserButton.Click();
    
    // ブラウザ起動の確認
}
```

**テスト対象:**
- ウィンドウ操作（透明化、リサイズ等）
- ブラウザボタンのクリック動作
- 設定画面の操作
- アクセシビリティ機能
- キーボードナビゲーション

### E2Eテスト（End-to-End Tests）
**フレームワーク**: Playwright for .NET

```csharp
[Test]
public async Task CompleteWorkflow_OpenURL_ShouldWorkEndToEnd()
{
    // コマンドライン起動からブラウザ起動まで
    // 実際のユーザーシナリオの自動テスト
}
```

**テスト対象:**
- コマンドライン引数処理
- URL処理フロー全体
- 設定変更の反映
- アップデート機能
- システムトレイ機能

### パフォーマンステスト
**フレームワーク**: BenchmarkDotNet

```csharp
[Benchmark]
public async Task BrowserDetection_Performance()
{
    await browserService.DetectBrowsersAsync();
}
```

**テスト対象:**
- 起動時間
- ブラウザ検出速度
- メモリ使用量
- UI応答性

## 🌍 多言語対応（国際化）詳細

### 実装アーキテクチャ
```csharp
// 多言語対応サービス
public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string key, params object[] args);
    void SetLanguage(CultureInfo culture);
    event EventHandler<LanguageChangedEventArgs> LanguageChanged;
    IEnumerable<CultureInfo> SupportedLanguages { get; }
}
```

### リソース管理
```xml
<!-- Resources.ja-JP.resx -->
<data name="BrowserChooser.Title" xml:space="preserve">
    <value>ブラウザを選択してください</value>
</data>
<data name="Settings.Display.Transparency" xml:space="preserve">
    <value>透明度</value>
</data>
```

### XAML実装
```xml
<!-- 動的言語切り替え対応 -->
<TextBlock Text="{Binding Source={x:Static loc:Resources.BrowserChooser_Title}}" 
           x:Name="TitleText"/>

<!-- MultiBinding使用例 -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource LocalizedStringConverter}">
            <Binding Source="Settings.Transparency.Value" />
            <Binding Path="TransparencyValue" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

### サポート言語
- **日本語（ja-JP）**: プライマリ言語
- **英語（en-US）**: セカンダリ言語
- **将来対応**: 中国語（zh-CN）、韓国語（ko-KR）

### 実装要件
- 実行時言語切り替え
- 文字列外部化（ハードコード文字列禁止）
- 数値・日付・時刻のローカライゼーション
- RTL言語への対応準備
- フォントファミリーの言語別対応

## 🔄 自動アップデート機能詳細

### アーキテクチャ
```csharp
public interface IUpdateService
{
    Task<UpdateInfo> CheckForUpdatesAsync();
    Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int> progress);
    Task<bool> InstallUpdateAsync(string packagePath);
    Task<bool> RollbackUpdateAsync();
    event EventHandler<UpdateAvailableEventArgs> UpdateAvailable;
}
```

### 更新フロー
1. **バックグラウンドチェック**
   - 起動時・定期実行での更新確認
   - GitHub Releases API またはカスタムエンドポイント
   - バージョン比較ロジック

2. **差分アップデート**
   - バイナリ差分パッチの適用
   - 設定ファイルのマイグレーション
   - 依存関係の更新

3. **段階的ロールアウト**
   - アルファ・ベータ・安定版チャンネル
   - カナリアリリース対応
   - ロールバック機能

### セキュリティ
- デジタル署名検証
- HTTPS通信の強制
- ハッシュ値検証
- サンドボックス内での更新処理

### UI実装
```xml
<!-- アップデート通知 -->
<Border x:Name="UpdateNotification" 
        Background="#E3F2FD" 
        Visibility="{Binding HasUpdate, Converter={StaticResource BoolToVisibilityConverter}}">
    <StackPanel Orientation="Horizontal" Margin="10">
        <TextBlock Text="{loc:Localize UpdateAvailable}" 
                   VerticalAlignment="Center"/>
        <Button Content="{loc:Localize InstallUpdate}" 
                Command="{Binding InstallUpdateCommand}"
                Margin="10,0,0,0"/>
    </StackPanel>
</Border>
```

## 🧪 包括的テスト設計詳細

### テスト自動化パイプライン
```yaml
# GitHub Actions設定例
name: Comprehensive Tests
on: [push, pull_request]

jobs:
  unit-tests:
    steps:
      - name: Run Unit Tests
        run: dotnet test --collect:"XPlat Code Coverage"
      - name: Generate Coverage Report
        run: reportgenerator

  integration-tests:
    steps:
      - name: Setup Test Environment
      - name: Run Integration Tests
      - name: Cleanup Test Data

  ui-tests:
    runs-on: windows-latest
    steps:
      - name: Start Application
      - name: Run FlaUI Tests
      - name: Capture Screenshots

  e2e-tests:
    steps:
      - name: Install Browsers
      - name: Run E2E Scenarios
      - name: Performance Monitoring

  accessibility-tests:
    steps:
      - name: Axe Accessibility Tests
      - name: Screen Reader Simulation
      - name: High Contrast Testing
```

### テストカバレッジ要件
- **コードカバレッジ**: 最低85%
- **分岐カバレッジ**: 最低80%
- **UI要素カバレッジ**: 100%
- **アクセシビリティ**: WCAG 2.1 AA準拠

### テストケース分類

#### 🔧 機能テスト
1. **ブラウザ管理**
   - ブラウザ検出・追加・編集・削除
   - アイコン読み込み・表示
   - デフォルトブラウザ設定

2. **URL処理**
   - URL正規化・短縮URL展開
   - プロトコルハンドリング
   - コマンドライン引数処理

3. **UI機能**
   - 透明化・視覚効果
   - グリッドレイアウト
   - 設定画面操作

#### ♿ アクセシビリティテスト
1. **キーボード操作**
   - Tab順序の確認
   - ショートカットキー動作
   - Enter/Spaceキー対応

2. **フォーカス管理**
   - フォーカス表示の確認
   - フォーカストラップ
   - 初期フォーカス設定

3. **スクリーンリーダー**
   - AutomationProperties設定
   - 音声読み上げ対応
   - ラベル・説明の適切性

#### 🌍 多言語テスト
1. **言語切り替え**
   - 実行時言語変更
   - UI要素の完全翻訳
   - レイアウト調整

2. **地域設定**
   - 日付・時刻フォーマット
   - 数値フォーマット
   - 通貨表示

#### 🔄 アップデートテスト
1. **更新チェック**
   - バックグラウンド確認
   - ネットワーク接続エラー処理
   - バージョン比較ロジック

2. **更新プロセス**
   - ダウンロード進捗表示
   - インストール成功・失敗
   - ロールバック機能

#### ⚡ パフォーマンステスト
1. **起動時間**
   - コールドスタート時間
   - ウォームスタート時間
   - ブラウザ検出時間

2. **メモリ使用量**
   - 初期メモリ使用量
   - 長時間動作時のメモリリーク
   - ガベージコレクション効率

3. **UI応答性**
   - ボタンクリック応答時間
   - 設定変更反映時間
   - 透明化処理のパフォーマンス

## 📅 スケジュール管理

### マイルストーン
- **Week 3**: Phase 1完了（基盤構築）
- **Week 7**: Phase 2完了（コア機能）
- **Week 11**: Phase 3完了（高度なUI機能）
- **Week 14**: Phase 4完了（システム統合）
- **Week 17**: Phase 5完了（最適化・テスト）

### リスク管理
- **技術的リスク**: 新技術の学習時間を考慮
- **スケジュールリスク**: 各フェーズに1週間のバッファを設定
- **品質リスク**: 継続的なテストとレビューを実施

### 進捗管理
- 週次進捗レビュー
- 日次スタンドアップ（必要に応じて）
- マイルストーン完了時の品質ゲート

## 📚 参考資料

- [WPF MVVM パターン](https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/data-binding-overview)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- [xUnit テストフレームワーク](https://xunit.net/)
- [FlaUI UIテスト](https://github.com/FlaUI/FlaUI)
- [WCAG 2.1 アクセシビリティガイドライン](https://www.w3.org/WAI/WCAG21/quickref/)

---

**この開発計画書は、プロジェクトルール（`.cursor/rules/global.mdc`）と併せて参照してください。**
