# 🌐 BrowserSelector

**BrowserSelector**は、Windows環境で複数のブラウザから選択してURLを開くためのモダンなWPFアプリケーションです。

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
- **設定管理**: アプリケーション設定と視覚設定の管理
- **多言語対応**: 日本語・英語の完全対応

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
- **単体テスト**: xUnit + Moq + FluentAssertions
- **統合テスト**: xUnit + Microsoft.Extensions.Hosting.Testing
- **UIテスト**: FlaUI
- **E2Eテスト**: Playwright for .NET

### テスト実行
```bash
dotnet test
```

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

### 開発環境セットアップ
1. リポジトリをクローン
2. 依存関係を復元: `dotnet restore`
3. ビルド: `dotnet build`
4. テスト実行: `dotnet test`

## 📄 ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 🤝 貢献

プルリクエストやイシューの報告を歓迎します。

---

**BrowserSelector** - Choose Your Browser Wisely! 🌐✨
