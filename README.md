# 🌐 BrowserSelector

**BrowserSelector**は、Windows環境で複数のブラウザから選択してURLを開くためのモダンなWPFアプリケーションです。

> **v0.2.0 .NET 10移行・ガラスUI刷新** 🎉  
> .NET 10へ移行し、Mica/Acrylicバックドロップによるガラスライクな外観、タイル刷新、キーボード操作、起動高速化を実装しました。

[![CI/CD](https://github.com/Yosuke-Sh/BrowserSelector/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/Yosuke-Sh/BrowserSelector/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Windows](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## ✨ 特徴

### 🎨 視覚的に優れたデザイン
- **ガラスUI**: Mica / Acrylic / MicaAlt のDWMバックドロップに対応した奥行きのあるデザイン（非対応環境向けの半透明単色・完全不透明モードも選択可能）
- **高品質アイコン**: Win32 API ExtractIconExによる高解像度アイコン抽出（PNGベースのリソースアイコンを使用）
- **カスタムロゴ**: ブラウザ選択の概念を表現する専用ロゴ
- **アニメーション効果**: ブラウザタイルのホバーアニメーション、フォーカスリング、奥行き表現
- **タイル立体表現の選択**: なし/影/ベベル/枠線から選択可能（影色はタイル背景色から自動生成）
- **レスポンシブレイアウト**: ウィンドウリサイズに追従するグリッドレイアウト
- **ウィンドウサイズ・起動位置**: 設定した初期サイズを常に適用（現在のサイズをワンクリックで取得可能）、マルチモニター環境ではカーソルのあるモニターに表示

### 🔧 コア機能
- **自動ブラウザ検出**: Windowsレジストリからインストール済みブラウザを自動検出
- **高品質アイコン表示**: Win32 APIを使用した高解像度アイコン抽出と表示
- **起動引数対応**: コマンドライン引数でURLを受け取り、ブラウザ選択画面を表示
- **重複除去**: 同じパスのブラウザの重複登録を防止
- **設定管理**: アプリケーション設定、視覚設定、ログ設定の管理
- **多言語対応**: 日本語・英語の完全対応
- **視覚効果**: カスタム背景色、グラデーション、ブラウザボタンカスタマイズ
- **URLルール管理**: 特定のURLパターンに対して自動的にブラウザを選択
- **キーボード操作**: Esc/Enter/Space/矢印キー/Tab/ホットキーによる操作、Ctrl+,での設定画面呼び出し
- **起動制御**: カウントダウン自動起動、トレイ常駐、CLIオプション（-d/-b/--silent/--auto-launch等）
- **自動アップデート**: GitHub Releases連携、SHA256完全性検証、非モーダル通知UI

### 🚀 使用方法

#### インストール済みアプリケーション
```bash
# 通常起動
BrowserSelector.exe

# URL指定での起動
BrowserSelector.exe "https://www.google.com"
```

#### 開発環境での実行
```bash
# 通常起動
dotnet run --project src/BrowserSelector.App

# URL指定での起動
dotnet run --project src/BrowserSelector.App -- "https://www.google.com"
```

### 📦 インストール

#### システム要件
- **OS**: Windows 10/11 (64-bit)
- **.NET Runtime**: .NET 10.0 Runtime
- **権限**: 管理者権限（インストール時）

#### インストール手順
1. [Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)から最新版をダウンロード
2. `BrowserSelector-Setup-v0.2.0.exe`を管理者権限で実行
3. インストールウィザードに従ってインストール
4. デスクトップまたはスタートメニューから起動

#### アンインストール
- Windows設定 > アプリ > BrowserSelector > アンインストール

#### 既定のブラウザに設定する
Windows 11ではプロトコル（http/https）ごとに既定アプリを選ぶ方式のため、インストーラーがOSへ直接書き込むことはできません。インストール時に「既定のブラウザに設定する」タスクを選択するか、設定画面の「既定のブラウザに設定」ボタンからWindowsの「既定のアプリ」画面を開き、BrowserSelectorを選択してください。

## 🏗️ アーキテクチャ

### プロジェクト構成
```
BrowserSelector.WPF/
├── src/
│   ├── BrowserSelector.Core/           # ドメイン層
│   ├── BrowserSelector.Infrastructure/ # インフラ層
│   ├── BrowserSelector.Presentation/   # プレゼンテーション層
│   └── BrowserSelector.App/           # アプリケーション層
├── tests/                              # テストプロジェクト
└── deployment/                         # 配布パッケージ
```

### 技術スタック
- **フレームワーク**: WPF (.NET 10.0)
- **MVVMライブラリ**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **設定管理**: Microsoft.Extensions.Configuration
- **多言語**: Microsoft.Extensions.Localization

## 🎨 視覚的改善

### 新機能
1. **ガラスUI（Mica/Acrylicバックドロップ）**
   - DWMバックドロップによる奥行きのある表現
   - 非対応環境向けの半透明単色・完全不透明フォールバック

2. **改善されたUI**
   - グローブアイコン付きURL入力フィールド
   - 設定アイコン付きボタン
   - タイルのホバーアニメーション・フォーカスリング
   - 角丸デザイン（外観タブで半径調整可能）

3. **ブランドアイデンティティ**
   - 専用ロゴデザイン（Phase C刷新版）
   - 一貫したカラーパレット
   - プロフェッショナルな外観

### デザイン要素
- **カラーパレット**: Material Design Blue (#2196F3, #1976D2)
- **タイポグラフィ**: Segoe UI
- **アイコン**: PNGベースのカスタムアイコン
- **レイアウト**: グリッドベースのレスポンシブデザイン

## 🧪 テスト

### テストカバレッジ
- **単体テスト（UnitTests）**: 241テスト中240成功（既知のフレーキーテスト1件を除き成功、xUnit + Moq + FluentAssertions）
- **単体テスト（CoreTests）**: 31テスト中31成功
- **単体テスト（InfrastructureTests）**: 28テスト中28成功
- **統合テスト**: 23テスト中23成功（設定ファイル保存・読み込み、レジストリアクセス）
- **UIテスト**: 5テスト中5成功（MSTest + FlaUI）
- **セキュリティテスト**: 238テスト中238成功（入力値検証、ファイルパス、レジストリアクセス）
- **Appテスト**: 88テスト中88成功（WPFアプリケーションリフレクションテスト）
- **パフォーマンステスト**: BenchmarkDotNetによるベンチマーク（xUnitランナーからは除外）
- **E2Eテスト**: 4テスト中4成功（Playwright for .NET）
- **総合**: 658テスト中657成功（既知のフレーキーテスト1件を除き100%成功、警告0件）

### テスト実行
```bash
# 全テスト実行
dotnet test

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"
```

### 品質保証
- **CI/CD**: GitHub Actionsによる自動テスト実行（簡素化済み）
- **テスト安定性**: 並列実行時の競合状態完全解決
- **警告解消**: ビルド警告0件、テスト警告0件の維持

## 📦 ビルド

### 開発ビルド
```bash
dotnet build
```

### リリースビルド
```bash
dotnet build --configuration Release
```

## 🚀 開発

### 前提条件
- .NET 10.0 SDK
- Visual Studio 2022 または VS Code
- Windows 10/11

### インストール方法
1. インストーラーをダウンロード
2. インストーラーを実行してインストール
3. 必要に応じてデフォルトブラウザとして設定

## 📦 ダウンロード・インストール

### リリース版
- [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)から最新版をダウンロード
- インストーラー版をダウンロード

### システム要件
- **OS**: Windows 10/11
- **.NET Runtime**: .NET 10.0 Runtime
- **メモリ**: 100MB以下
- **ディスク**: 50MB以上の空き容量

## 🔄 自動アップデート

v0.3.0より、GitHub Releasesと連携した自動アップデート機能を搭載しています。
- **起動時の自動更新チェック**: 起動5秒後にバックグラウンドで確認（起動速度に影響しない）
- **完全性検証**: SHA256チェックサムによる検証。不一致の場合は適用せずファイルを削除
- **2つの適用経路**: インストーラー版はUAC昇格インストーラー、ポータブル版は別プロセス`BrowserSelector.Updater.exe`による自己置換
- **非モーダル通知**: メインウィンドウ下部のバーで「今すぐ更新」「次回起動時」「スキップ」「リリースノート」を選択可能

詳細は[USER_MANUAL.md](docs/USER_MANUAL.md#-アップデート--updates)を参照してください。

## 📄 ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は[LICENSE](LICENSE)ファイルを参照してください。

## 🤝 貢献

プルリクエストやイシューの報告を歓迎します。

### 貢献方法
1. リポジトリをフォーク
2. フィーチャーブランチを作成
3. 変更をコミット
4. プルリクエストを作成

### 開発ガイドライン
- コード品質: 85%以上のカバレッジ
- 警告: ビルド警告0件の維持
- テスト: 新機能にはテストを追加
- ドキュメント: 変更には適切なドキュメント更新

### ブランチ戦略（簡易）
- ブランチ: `master`（安定版）、`developer`（開発用、通常はここで直接作業）
- フロー: `developer`で開発 → `master`へPRを作成してマージ
- ルール: `master`はPR必須・直接プッシュ禁止

## 📞 サポート

- **Issues**: [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)
- **Wiki**: [プロジェクトWiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)

---

## English

**BrowserSelector** is a modern WPF application for selecting and opening URLs with multiple browsers on Windows.

[![CI/CD](https://github.com/Yosuke-Sh/BrowserSelector/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/Yosuke-Sh/BrowserSelector/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Windows](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

### ✨ Features

#### 🎨 Visually Superior Design
- **Glass UI**: Mica / Acrylic / MicaAlt DWM backdrops for a layered, translucent look (with solid-color and fully opaque fallbacks for non-DWM environments)
- **High-Quality Icons**: High-resolution icon extraction using Win32 API ExtractIconEx (PNG-based resource icons)
- **Custom Logo**: Dedicated logo expressing the concept of browser selection
- **Animation Effects**: Hover animations and focus rings on browser tiles
- **Responsive Layout**: Grid layout that adapts as the window is resized

#### 🔧 Core Features
- **Automatic Browser Detection**: Automatically detects installed browsers from Windows Registry
- **High-Quality Icon Display**: High-resolution icon extraction and display using Win32 API
- **Launch Argument Support**: Accepts URLs via command line arguments and displays browser selection screen
- **Duplicate Prevention**: Prevents duplicate registration of browsers with the same path
- **Settings Management**: Manages application, visual, and log settings
- **Multi-language Support**: Full support for Japanese and English
- **Visual Effects**: Custom background colors, gradients, browser button customization
- **URL Rule Management**: Automatically select browsers for specific URL patterns
- **Keyboard Operation**: Esc/Enter/Space/arrow keys/Tab/hotkeys, Ctrl+, to open settings
- **Startup Control**: Countdown auto-launch, tray residency, CLI options (-d/-b/--silent/--auto-launch, etc.)
- **Automatic Updates**: GitHub Releases integration, SHA256 integrity verification, non-modal notification UI

### 🚀 Usage

#### Normal Launch
```bash
dotnet run --project src/BrowserSelector.App
```

#### Launch with URL
```bash
dotnet run --project src/BrowserSelector.App -- "https://www.google.com"
```

### 🏗️ Architecture

#### Project Structure
```
BrowserSelector.WPF/
├── src/
│   ├── BrowserSelector.Core/           # Domain Layer
│   ├── BrowserSelector.Infrastructure/ # Infrastructure Layer
│   ├── BrowserSelector.Presentation/   # Presentation Layer
│   └── BrowserSelector.App/           # Application Layer
├── tests/                              # Test Projects
└── deployment/                         # Distribution Packages
```

#### Technology Stack
- **Framework**: WPF (.NET 10.0)
- **MVVM Library**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Localization**: Microsoft.Extensions.Localization

### 🧪 Testing

#### Test Coverage
- **Unit Tests (UnitTests)**: 240/241 tests passing (one known flaky test excluded, xUnit + Moq + FluentAssertions)
- **Unit Tests (CoreTests)**: 31/31 tests passing
- **Unit Tests (InfrastructureTests)**: 28/28 tests passing
- **Integration Tests**: 23/23 tests passing (Settings file save/load, registry access)
- **UI Tests**: 5/5 tests passing (MSTest + FlaUI)
- **Security Tests**: 238/238 tests passing (Input validation, file paths, registry access)
- **App Tests**: 88/88 tests passing (WPF application reflection tests)
- **Performance Tests**: Benchmarks with BenchmarkDotNet (excluded from the xUnit test runner)
- **E2E Tests**: 4/4 tests passing (Playwright for .NET)
- **Overall**: 657/658 tests passing (100% excluding one known flaky test, zero warnings)

#### Test Execution
```bash
# Run all tests
dotnet test

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

#### Quality Assurance
- **CI/CD**: Automated test execution with GitHub Actions
- **Quality Gates**: 85% coverage threshold
- **Warning Resolution**: Maintain zero build warnings

### 📦 Download & Installation

#### Release Version
- Download the latest version from [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)
- Download the installer version

#### System Requirements
- **OS**: Windows 10/11
- **.NET Runtime**: .NET 10.0 Runtime
- **Memory**: Less than 100MB
- **Disk**: 50MB+ free space

### 🔄 Automatic Updates

Since v0.3.0, BrowserSelector includes automatic updates integrated with GitHub Releases.
- **Automatic check on startup**: Checked in the background 5 seconds after launch (does not affect startup speed)
- **Integrity verification**: SHA256 checksum verification; mismatched downloads are rejected and deleted, never applied
- **Two apply paths**: Installer installs are updated via a UAC-elevated installer; portable installs are self-replaced by a separate `BrowserSelector.Updater.exe` process
- **Non-modal notification**: A bar at the bottom of the main window offers "Update Now", "Next Launch", "Skip", and "Release Notes"

See [USER_MANUAL.md](docs/USER_MANUAL.md#-アップデート--updates) for details.

### 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

### 🤝 Contributing

Pull requests and issue reports are welcome.

#### How to Contribute
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Create a pull request

#### Development Guidelines
- Code Quality: 85%+ coverage
- Warnings: Maintain zero build warnings
- Testing: Add tests for new features
- Documentation: Update documentation for changes

#### Branch Strategy (Quick)
- Branches: `master` (stable), `developer` (development, usually worked on directly)
- Flow: Develop on `developer` → open a PR to `master` and merge
- Rules: PR required, no direct push to `master`

### 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)
- **Wiki**: [Project Wiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)

---

**BrowserSelector** - Choose Your Browser Wisely! 🌐✨
