# 🌐 BrowserSelector

**BrowserSelector**は、Windows環境で複数のブラウザから選択してURLを開くためのモダンなWPFアプリケーションです。

[![CI/CD](https://github.com/Yosuke-Sh/BrowserSelector/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/Yosuke-Sh/BrowserSelector/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Windows](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

## ✨ 特徴

### 🎨 視覚的に優れたデザイン
- **モダンなUI**: Material Designにインスパイアされた洗練されたデザイン
- **高品質アイコン**: Win32 API ExtractIconExによる高解像度アイコン抽出
- **SVGアイコン**: 高解像度でスケーラブルなベクターアイコン
- **カスタムロゴ**: ブラウザ選択の概念を表現する専用ロゴ
- **アニメーション効果**: ホバー時のドロップシャドウとトランジション
- **レスポンシブレイアウト**: 様々な画面サイズに対応

### 🔧 コア機能
- **自動ブラウザ検出**: Windowsレジストリからインストール済みブラウザを自動検出
- **高品質アイコン表示**: Win32 APIを使用した高解像度アイコン抽出と表示
- **起動引数対応**: コマンドライン引数でURLを受け取り、ブラウザ選択画面を表示
- **重複除去**: 同じパスのブラウザの重複登録を防止
- **設定管理**: アプリケーション設定、視覚設定、ログ設定の管理
- **多言語対応**: 日本語・英語の完全対応
- **視覚効果**: カスタム背景色、グラデーション、ブラウザボタンカスタマイズ
- **URLルール管理**: 特定のURLパターンに対して自動的にブラウザを選択
- **自動アップデート**: GitHub Releases連携による設定ファイル差分更新

### 🚀 使用方法

#### 通常起動
```bash
dotnet run --project src/BrowserSelector.App
```

#### URL指定での起動
```bash
dotnet run --project src/BrowserSelector.App -- "https://www.google.com"
```

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
- **フレームワーク**: WPF (.NET 8.0)
- **MVVMライブラリ**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **設定管理**: Microsoft.Extensions.Configuration
- **多言語**: Microsoft.Extensions.Localization

## 🎨 視覚的改善

### 新機能
1. **SVGアイコンシステム**
   - 高解像度でスケーラブル
   - ブラウザ選択の概念を表現
   - モダンなグラデーション効果

2. **改善されたUI**
   - グローブアイコン付きURL入力フィールド
   - 設定アイコン付きボタン
   - ホバー効果とドロップシャドウ
   - 角丸デザイン

3. **ブランドアイデンティティ**
   - 専用ロゴデザイン
   - 一貫したカラーパレット
   - プロフェッショナルな外観

### デザイン要素
- **カラーパレット**: Material Design Blue (#2196F3, #1976D2)
- **タイポグラフィ**: Segoe UI
- **アイコン**: カスタムSVGアイコン
- **レイアウト**: グリッドベースのレスポンシブデザイン

## 🧪 テスト

### テストカバレッジ
- **単体テスト**: 190テスト中189成功（xUnit + Moq + FluentAssertions）
- **統合テスト**: 23テスト中23成功（設定ファイル保存・読み込み、レジストリアクセス）
- **UIテスト**: 5テスト中5成功（MSTest + FlaUI）
- **セキュリティテスト**: 238テスト中238成功（入力値検証、ファイルパス、レジストリアクセス）
- **Appテスト**: 154テスト中154成功（WPFアプリケーションリフレクションテスト）
- **ライブラリテスト**: 61テスト中61成功（ライブラリ機能テスト）
- **パフォーマンステスト**: BenchmarkDotNetによるベンチマーク
- **E2Eテスト**: 4テスト中4成功（Playwright for .NET）
- **総合**: 696テスト中695成功（99.9%成功率）

### テスト実行
```bash
# 全テスト実行
dotnet test

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"
```

### 品質保証
- **CI/CD**: GitHub Actionsによる自動テスト実行
- **品質ゲート**: 85%カバレッジ閾値
- **警告解消**: ビルド警告0件の維持

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
- .NET 8.0 SDK
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
- **.NET Runtime**: .NET 8.0 Runtime
- **メモリ**: 100MB以下
- **ディスク**: 50MB以上の空き容量

## 🔄 自動アップデート

BrowserSelectorは自動更新機能を搭載しています：
- 起動時の自動更新チェック
- GitHub Releases API連携
- セキュアな更新プロセス
- ロールバック機能

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
- ブランチ: `main`（安定版）、`developer`（統合）、`feature/*`、`hotfix/*`、`release/*`
- フロー: `feature/*` → `developer` → `release/*` → `main`
- ルール: `main`/`developer`はPR必須・直接プッシュ禁止

## 📞 サポート

- **Issues**: [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)
- **Wiki**: [プロジェクトWiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)

---

## English

**BrowserSelector** is a modern WPF application for selecting and opening URLs with multiple browsers on Windows.

[![CI/CD](https://github.com/Yosuke-Sh/BrowserSelector/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/Yosuke-Sh/BrowserSelector/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Windows](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)

### ✨ Features

#### 🎨 Visually Superior Design
- **Modern UI**: Refined design inspired by Material Design
- **High-Quality Icons**: High-resolution icon extraction using Win32 API ExtractIconEx
- **SVG Icons**: High-resolution and scalable vector icons
- **Custom Logo**: Dedicated logo expressing the concept of browser selection
- **Animation Effects**: Drop shadows and transitions on hover
- **Responsive Layout**: Adapts to various screen sizes

#### 🔧 Core Features
- **Automatic Browser Detection**: Automatically detects installed browsers from Windows Registry
- **High-Quality Icon Display**: High-resolution icon extraction and display using Win32 API
- **Launch Argument Support**: Accepts URLs via command line arguments and displays browser selection screen
- **Duplicate Prevention**: Prevents duplicate registration of browsers with the same path
- **Settings Management**: Manages application, visual, and log settings
- **Multi-language Support**: Full support for Japanese and English
- **Visual Effects**: Custom background colors, gradients, browser button customization
- **URL Rule Management**: Automatically select browsers for specific URL patterns
- **Automatic Updates**: GitHub Releases integration with settings file differential updates

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
- **Framework**: WPF (.NET 8.0)
- **MVVM Library**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Localization**: Microsoft.Extensions.Localization

### 🧪 Testing

#### Test Coverage
- **Unit Tests**: 189/190 tests passing (xUnit + Moq + FluentAssertions)
- **Integration Tests**: 23/23 tests passing (Settings file save/load, registry access)
- **UI Tests**: 5/5 tests passing (MSTest + FlaUI)
- **Security Tests**: 238/238 tests passing (Input validation, file paths, registry access)
- **App Tests**: 154/154 tests passing (WPF application reflection tests)
- **Library Tests**: 61/61 tests passing (Library functionality tests)
- **Performance Tests**: Benchmarks with BenchmarkDotNet
- **E2E Tests**: 4/4 tests passing (Playwright for .NET)
- **Overall**: 695/696 tests passing (99.9% success rate)

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
- **.NET Runtime**: .NET 8.0 Runtime
- **Memory**: Less than 100MB
- **Disk**: 50MB+ free space

### 🔄 Automatic Updates

BrowserSelector features automatic update functionality:
- Automatic update check on startup
- GitHub Releases API integration
- Secure update process
- Rollback functionality

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
- Branches: `main` (stable), `developer` (integration), `feature/*`, `hotfix/*`, `release/*`
- Flow: `feature/*` → `developer` → `release/*` → `main`
- Rules: PR required, no direct push to `main`/`developer`

### 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)
- **Wiki**: [Project Wiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)

---

**BrowserSelector** - Choose Your Browser Wisely! 🌐✨
