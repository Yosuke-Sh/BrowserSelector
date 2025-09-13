# BrowserSelector v0.1.0 リリースノート / Release Notes

**リリース日 / Release Date**: 2025年1月10日 / January 10, 2025  
**バージョン / Version**: v0.1.0  
**コードネーム / Codename**: "Initial Release"

## 🎉 概要 / Overview

### 日本語
BrowserSelector v0.1.0は、Windows環境で複数のブラウザから選択してURLを開くためのWPFアプリケーションの初期リリースです。現代的なMVVMアーキテクチャと包括的なテストスイートを採用し、高品質なユーザー体験を提供します。

### English
BrowserSelector v0.1.0 is the initial release of a WPF application for selecting and opening URLs with multiple browsers on Windows. It adopts modern MVVM architecture and comprehensive test suites to provide a high-quality user experience.

## ✨ 新機能 / New Features

### 🌐 コア機能 / Core Features

#### 日本語
- **ブラウザ選択機能**: 複数ブラウザの一覧表示とグリッドレイアウト
- **ブラウザクリックでURL起動**: ブラウザクリックでURL起動機能
- **自動ブラウザ検出**: レジストリ/ファイルシステムからの自動検出
- **カスタムブラウザの追加・編集・削除**: ブラウザ管理機能
- **ブラウザごとのカスタムアイコン設定**: .exe、.ico、画像ファイル対応
- **アイコンスケール調整機能**: アイコンサイズの調整
- **デフォルトブラウザ設定**: デフォルトブラウザの設定

#### English
- **Browser Selection**: Multiple browser list display with grid layout
- **URL Launch on Browser Click**: Launch URLs by clicking browser buttons
- **Automatic Browser Detection**: Auto-detection from registry/file system
- **Custom Browser Management**: Add, edit, and delete custom browsers
- **Custom Icon Settings**: Support for .exe, .ico, and image files
- **Icon Scale Adjustment**: Adjustable icon sizes
- **Default Browser Setting**: Set default browser preference

### 🎨 UI/UX機能 / UI/UX Features

#### 日本語
- **透明化・視覚効果**: ウィンドウ透明化（透明度0.01-1.0）
- **カスタム透明化色設定**: 透明化色のカスタマイズ
- **角の丸み設定**: 半径0-50の角丸設定
- **タイトルバー表示/非表示**: タイトルバーの制御
- **カスタム背景色**: 背景色の設定
- **縦方向背景グラデーション**: グラデーション背景

#### English
- **Transparency & Visual Effects**: Window transparency (0.01-1.0 opacity)
- **Custom Transparency Color**: Customizable transparency colors
- **Corner Radius Setting**: Rounded corners (0-50 radius)
- **Title Bar Toggle**: Show/hide title bar
- **Custom Background Color**: Background color customization
- **Vertical Background Gradient**: Gradient background support

### ⚙️ システム機能 / System Features

#### 日本語
- **システムトレイ機能**: 基本的なシステムトレイ機能
- **カスタムプロトコル登録**: `browser://`プロトコルの登録
- **URL正規化機能**: URL処理機能
- **コマンドライン引数処理**: コマンドライン引数の処理

#### English
- **System Tray Functionality**: Basic system tray features
- **Custom Protocol Registration**: `browser://` protocol registration
- **URL Normalization**: URL processing functionality
- **Command Line Argument Processing**: Command line argument handling

### 🌍 多言語対応 / Multi-language Support

#### 日本語
- **日本語・英語の基本対応**: 日英の基本的な多言語対応
- **動的言語切り替え**: 実行時言語切り替え

#### English
- **Japanese & English Support**: Basic multi-language support for Japanese and English
- **Dynamic Language Switching**: Runtime language switching

### ♿ アクセシビリティ / Accessibility

#### 日本語
- **AccessibleButtonクラス**: 基本的なアクセシブルボタン実装

#### English
- **AccessibleButton Class**: Basic accessible button implementation

## 🏗️ 技術仕様 / Technical Specifications

### アーキテクチャ / Architecture

#### 日本語
- **フレームワーク**: WPF (.NET 8.0)
- **アーキテクチャパターン**: MVVM (Model-View-ViewModel)
- **MVVMライブラリ**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **設定管理**: Microsoft.Extensions.Configuration
- **ログ**: Microsoft.Extensions.Logging + Serilog
- **多言語**: Microsoft.Extensions.Localization

#### English
- **Framework**: WPF (.NET 8.0)
- **Architecture Pattern**: MVVM (Model-View-ViewModel)
- **MVVM Library**: CommunityToolkit.Mvvm
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Configuration Management**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging + Serilog
- **Localization**: Microsoft.Extensions.Localization

### テスト / Testing

#### 日本語
- **単体テスト**: xUnit + Moq + FluentAssertions (190テスト)
- **統合テスト**: xUnit + Microsoft.Extensions.Testing (23テスト)
- **E2Eテスト**: NUnit + Playwright (4テスト)
- **UIテスト**: MSTest + FlaUI (5テスト)
- **セキュリティテスト**: xUnit + FluentAssertions (238テスト)
- **Appテスト**: xUnit + FluentAssertions (154テスト)
- **ライブラリテスト**: xUnit + FluentAssertions (61テスト)
- **総テスト数**: 702テスト（100%成功）

#### English
- **Unit Tests**: xUnit + Moq + FluentAssertions (190 tests)
- **Integration Tests**: xUnit + Microsoft.Extensions.Testing (23 tests)
- **E2E Tests**: NUnit + Playwright (4 tests)
- **UI Tests**: MSTest + FlaUI (5 tests)
- **Security Tests**: xUnit + FluentAssertions (238 tests)
- **App Tests**: xUnit + FluentAssertions (154 tests)
- **Library Tests**: xUnit + FluentAssertions (61 tests)
- **Total Tests**: 702 tests (100% success)

### 品質保証 / Quality Assurance

#### 日本語
- **静的解析**: SonarQube Community
- **コード解析**: Microsoft.CodeAnalysis.Analyzers
- **セキュリティ**: Microsoft.CodeAnalysis.BannedApiAnalyzers
- **警告**: 0件（完全解決済み）

#### English
- **Static Analysis**: SonarQube Community
- **Code Analysis**: Microsoft.CodeAnalysis.Analyzers
- **Security**: Microsoft.CodeAnalysis.BannedApiAnalyzers
- **Warnings**: 0 (completely resolved)

## 📦 配布パッケージ / Distribution Packages

### インストーラー / Installer

#### 日本語
- **ファイル名**: `BrowserSelector-Setup-v0.1.0.exe`
- **サイズ**: 約53.3MB
- **対応OS**: Windows 10/11 (x64)
- **インストール先**: `C:\Program Files\BrowserSelector\`
- **プロトコル登録**: `browser://`プロトコルの自動登録

#### English
- **File Name**: `BrowserSelector-Setup-v0.1.0.exe`
- **Size**: Approximately 53.3MB
- **Supported OS**: Windows 10/11 (x64)
- **Installation Directory**: `C:\Program Files\BrowserSelector\`
- **Protocol Registration**: Automatic `browser://` protocol registration

### 機能 / Features

#### 日本語
- **管理者権限**: インストール時に管理者権限が必要
- **多言語対応**: 日本語・英語のインストーラー
- **アンインストール**: 完全なアンインストール機能
- **スタートメニュー**: スタートメニューへの登録
- **デスクトップショートカット**: デスクトップショートカットの作成

#### English
- **Administrator Privileges**: Administrator privileges required for installation
- **Multi-language Support**: Japanese and English installer
- **Uninstall**: Complete uninstall functionality
- **Start Menu**: Start menu registration
- **Desktop Shortcut**: Desktop shortcut creation

## 🔧 システム要件 / System Requirements

### 最小要件 / Minimum Requirements

#### 日本語
- **OS**: Windows 10 (バージョン 1903) 以降
- **アーキテクチャ**: x64
- **.NET Runtime**: .NET 8.0 Runtime
- **メモリ**: 100MB以上の空きメモリ
- **ディスク**: 100MB以上の空き容量

#### English
- **OS**: Windows 10 (version 1903) or later
- **Architecture**: x64
- **.NET Runtime**: .NET 8.0 Runtime
- **Memory**: 100MB or more free memory
- **Disk**: 100MB or more free space

### 推奨要件 / Recommended Requirements

#### 日本語
- **OS**: Windows 11
- **メモリ**: 4GB以上
- **ディスク**: 1GB以上の空き容量

#### English
- **OS**: Windows 11
- **Memory**: 4GB or more
- **Disk**: 1GB or more free space

## 🚀 インストール方法 / Installation Instructions

### 日本語

1. **インストーラーのダウンロード**
   - GitHub Releasesから`BrowserSelector-Setup-v0.1.0.exe`をダウンロード

2. **インストールの実行**
   - ダウンロードしたファイルを右クリック → 「管理者として実行」
   - インストールウィザードに従ってインストール

3. **初回起動**
   - スタートメニューまたはデスクトップショートカットから起動
   - 初回起動時にブラウザの自動検出が実行されます

### English

1. **Download the Installer**
   - Download `BrowserSelector-Setup-v0.1.0.exe` from GitHub Releases

2. **Run the Installation**
   - Right-click the downloaded file → "Run as administrator"
   - Follow the installation wizard to install

3. **First Launch**
   - Launch from Start Menu or desktop shortcut
   - Automatic browser detection will run on first launch

## 📖 使用方法 / Usage Guide

### 基本的な使用方法 / Basic Usage

#### 日本語

1. **アプリケーションの起動**
   - スタートメニューから「BrowserSelector」を起動
   - または、デスクトップショートカットをダブルクリック

2. **ブラウザの選択**
   - 表示されたブラウザ一覧から目的のブラウザをクリック
   - URLが選択したブラウザで開かれます

3. **設定の変更**
   - アプリケーション内の設定ボタンから各種設定を変更可能

#### English

1. **Launch the Application**
   - Launch "BrowserSelector" from Start Menu
   - Or double-click the desktop shortcut

2. **Select Browser**
   - Click on the desired browser from the displayed browser list
   - URL will open in the selected browser

3. **Change Settings**
   - Change various settings from the settings button within the application

### コマンドライン使用 / Command Line Usage

#### 日本語
```bash
# デフォルトブラウザでURLを開く
BrowserSelector.exe "https://example.com"

# 特定のブラウザでURLを開く
BrowserSelector.exe --browser "Chrome" "https://example.com"
```

#### English
```bash
# Open URL with default browser
BrowserSelector.exe "https://example.com"

# Open URL with specific browser
BrowserSelector.exe --browser "Chrome" "https://example.com"
```

### プロトコル使用 / Protocol Usage

#### 日本語
```
# ブラウザ選択画面を表示
browser://

# 特定のURLを開く
browser://https://example.com
```

#### English
```
# Display browser selection screen
browser://

# Open specific URL
browser://https://example.com
```

## ⚙️ 設定項目 / Configuration Options

### 表示設定 / Display Settings

#### 日本語
- **透明度**: 0.01-1.0の範囲で設定可能
- **背景色**: カスタム背景色の設定
- **グラデーション**: 縦方向グラデーションの有効/無効
- **角の丸み**: 0-50の範囲で設定可能
- **タイトルバー**: 表示/非表示の切り替え

#### English
- **Transparency**: Configurable in range 0.01-1.0
- **Background Color**: Custom background color setting
- **Gradient**: Enable/disable vertical gradient
- **Corner Radius**: Configurable in range 0-50
- **Title Bar**: Show/hide toggle

### ブラウザ設定 / Browser Settings

#### 日本語
- **カスタムブラウザの追加**: 新しいブラウザの追加
- **ブラウザの編集**: 既存ブラウザの設定変更
- **ブラウザの削除**: 不要なブラウザの削除
- **アイコン設定**: カスタムアイコンの設定
- **デフォルトブラウザ**: デフォルトブラウザの設定

#### English
- **Add Custom Browser**: Add new browser
- **Edit Browser**: Modify existing browser settings
- **Delete Browser**: Remove unnecessary browsers
- **Icon Settings**: Custom icon configuration
- **Default Browser**: Set default browser preference

## 🐛 既知の問題 / Known Issues

### 制限事項 / Limitations

#### 日本語
- **WPFトリミング**: .NET 8.0のトリミング機能はWPFでサポートされていないため、無効化されています
- **単一ファイル配布**: 現在は単一ファイル配布を使用していますが、一部の機能で制限があります

#### English
- **WPF Trimming**: .NET 8.0 trimming feature is not supported for WPF, so it is disabled
- **Single File Distribution**: Currently using single file distribution, but some features have limitations

### 注意事項 / Important Notes

#### 日本語
- **管理者権限**: インストール時とプロトコル登録時に管理者権限が必要です
- **ウイルス対策ソフト**: 一部のウイルス対策ソフトで誤検知される可能性があります

#### English
- **Administrator Privileges**: Administrator privileges are required for installation and protocol registration
- **Antivirus Software**: May be falsely detected by some antivirus software

## 🔄 今後の予定 / Future Plans

### v0.2.0（予定 / Planned）

#### 日本語
- **パフォーマンス最適化**: 起動時間とメモリ使用量の改善
- **実行ファイルサイズの最適化**: 現在の53.3MBから30MB以下への削減
- **アクセシビリティの強化**: スクリーンリーダー対応の完全実装
- **多言語対応の拡張**: 中国語、韓国語対応

#### English
- **Performance Optimization**: Improve startup time and memory usage
- **Executable Size Optimization**: Reduce from current 53.3MB to under 30MB
- **Accessibility Enhancement**: Complete screen reader support implementation
- **Multi-language Support Expansion**: Chinese and Korean support

### v0.3.0（予定 / Planned）

#### 日本語
- **自動アップデート機能**: セキュアな自動更新システム
- **高度な設定**: より詳細なカスタマイズオプション
- **プラグインシステム**: 拡張機能のサポート

#### English
- **Auto-update Feature**: Secure automatic update system
- **Advanced Settings**: More detailed customization options
- **Plugin System**: Extension support

## 📞 サポート / Support

### 問題報告 / Issue Reporting

#### 日本語
- **GitHub Issues**: [https://github.com/your-username/BrowserSelector/issues](https://github.com/your-username/BrowserSelector/issues)
- **バグ報告**: 問題が発生した場合は、詳細な情報とログファイルを添付してください

#### English
- **GitHub Issues**: [https://github.com/your-username/BrowserSelector/issues](https://github.com/your-username/BrowserSelector/issues)
- **Bug Reports**: When reporting issues, please attach detailed information and log files

### ドキュメント / Documentation

#### 日本語
- **ユーザーマニュアル**: `docs/USER_MANUAL.md`
- **API仕様書**: `docs/API.md`
- **開発者ガイド**: `docs/CONTRIBUTING.md`

#### English
- **User Manual**: `docs/USER_MANUAL.md`
- **API Specification**: `docs/API.md`
- **Developer Guide**: `docs/CONTRIBUTING.md`

## 📄 ライセンス / License

### 日本語
本ソフトウェアはMITライセンスの下で提供されています。詳細は`LICENSE`ファイルを参照してください。

### English
This software is provided under the MIT License. See the `LICENSE` file for details.

## 🙏 謝辞 / Acknowledgments

### 日本語
- **.NET Community**: .NET 8.0とWPFフレームワークの提供
- **CommunityToolkit.Mvvm**: MVVMライブラリの提供
- **テストフレームワーク**: xUnit、NUnit、FlaUI、Playwrightの開発者
- **オープンソースコミュニティ**: 様々なライブラリとツールの提供

### English
- **.NET Community**: Providing .NET 8.0 and WPF framework
- **CommunityToolkit.Mvvm**: Providing MVVM library
- **Test Frameworks**: Developers of xUnit, NUnit, FlaUI, Playwright
- **Open Source Community**: Providing various libraries and tools

---

### 日本語
**BrowserSelector v0.1.0** - 高品質なブラウザ選択体験をお届けします。

*このリリースノートは2025年1月10日に作成されました。*

### English
**BrowserSelector v0.1.0** - Delivering high-quality browser selection experience.

*This release notes was created on January 10, 2025.*
