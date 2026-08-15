# BrowserSelector ユーザーマニュアル / User Manual

## 📋 目次 / Table of Contents

### 日本語
- [概要](#概要)
- [インストール](#インストール)
- [基本的な使用方法](#基本的な使用方法)
- [キーボードショートカット](#-キーボードショートカット--keyboard-shortcuts)
- [設定](#設定)
- [外観設定](#-外観設定--appearance-settings)
- [起動制御（トレイ常駐・自動起動・CLIオプション）](#-起動制御トレイ常駐自動起動cliオプション--startup-control-tray-residency-auto-launch-cli-options)
- [機能詳細](#機能詳細)
- [トラブルシューティング](#トラブルシューティング)
- [FAQ](#faq)

### English
- [Overview](#overview)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Keyboard Shortcuts](#-キーボードショートカット--keyboard-shortcuts)
- [Configuration](#configuration)
- [Appearance Settings](#-外観設定--appearance-settings)
- [Startup Control (Tray, Auto-launch, CLI Options)](#-起動制御トレイ常駐自動起動cliオプション--startup-control-tray-residency-auto-launch-cli-options)
- [Feature Details](#feature-details)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)

## 🌐 概要 / Overview

### 日本語

BrowserSelectorは、Windows環境で複数のブラウザから選択してURLを開くためのアプリケーションです。URLを入力すると、インストールされているブラウザの一覧が表示され、希望するブラウザを選択してURLを開くことができます。

#### 主な特徴

- **自動ブラウザ検出**: インストールされているブラウザを自動的に検出
- **高品質アイコン**: 各ブラウザのアイコンを高解像度で表示
- **カスタマイズ可能**: 外観や動作を自由にカスタマイズ
- **多言語対応**: 日本語・英語に対応

### English

BrowserSelector is an application for selecting and opening URLs with multiple browsers on Windows. When you enter a URL, a list of installed browsers is displayed, allowing you to select your preferred browser to open the URL.

#### Key Features

- **Automatic Browser Detection**: Automatically detects installed browsers
- **High-Quality Icons**: Displays high-resolution icons for each browser
- **Customizable**: Freely customize appearance and behavior
- **Multi-language Support**: Supports Japanese and English

## 📦 インストール / Installation

### システム要件 / System Requirements

#### 日本語
- **OS**: Windows 10/11
- **.NET Runtime**: .NET 10.0 Runtime
- **メモリ**: 100MB以下
- **ディスク**: 50MB以上の空き容量

#### English
- **OS**: Windows 10/11
- **.NET Runtime**: .NET 10.0 Runtime
- **Memory**: 100MB or less
- **Disk**: 50MB or more free space

### インストール方法 / Installation Instructions

#### 日本語
1. [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)から最新版をダウンロード
2. インストーラー版をダウンロード
3. setup.exeを実行してインストール
4. 必要に応じてデフォルトブラウザとして設定

#### English
1. Download the latest version from [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)
2. Download the installer version
3. Run setup.exe to install
4. Set as default browser if needed

### .NET 10.0 Runtimeのインストール / .NET 10.0 Runtime Installation

#### 日本語
.NET 10.0 Runtimeがインストールされていない場合：
1. [Microsoft .NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)からダウンロード
2. インストーラーを実行してインストール

#### English
If .NET 10.0 Runtime is not installed:
1. Download from [Microsoft .NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Run the installer to install

## 🚀 基本的な使用方法 / Basic Usage

### 起動方法 / Launch Methods

#### 日本語

##### 通常起動
1. BrowserSelector.exeをダブルクリック
2. アプリケーションが起動し、ブラウザ選択画面が表示されます

##### コマンドライン起動
```cmd
BrowserSelector.exe "https://www.google.com"
```

#### English

##### Normal Launch
1. Double-click BrowserSelector.exe
2. The application launches and displays the browser selection screen

##### Command Line Launch
```cmd
BrowserSelector.exe "https://www.google.com"
```

### 基本的な操作 / Basic Operations

#### 日本語

1. **URL入力**
   - 上部のテキストボックスにURLを入力
   - 例：`https://www.google.com`

2. **ブラウザ選択**
   - 表示されたブラウザボタンをクリック
   - 選択したブラウザでURLが開きます

3. **設定画面**
   - 下部の「設定」ボタンをクリック
   - 各種設定を変更できます

#### English

1. **URL Input**
   - Enter URL in the text box at the top
   - Example: `https://www.google.com`

2. **Browser Selection**
   - Click on the displayed browser button
   - URL opens in the selected browser

3. **Settings Screen**
   - Click the "Settings" button at the bottom
   - Various settings can be changed

## ⌨️ キーボードショートカット / Keyboard Shortcuts

### 日本語

BrowserSelectorはマウス操作なしでも完結できるよう、キーボード操作に対応しています。

| キー | 動作 |
|---|---|
| `Esc` | ウィンドウを閉じる |
| `Enter` / `Space` | フォーカス中のブラウザを起動 |
| 矢印キー | グリッド内のブラウザタイル間を移動 |
| `Tab` | 次の操作可能な要素へフォーカス移動 |
| `1`〜`9`、`A`〜`Z` | 対応するホットキーが割り当てられたブラウザを直接起動 |
| `Ctrl` + `,` | 設定画面を開く |
| `Ctrl` + クリック / `Ctrl` + `Enter` | ブラウザを起動してもアプリケーションを閉じない |

### English

BrowserSelector supports full keyboard operation so the app can be used without a mouse.

| Key | Action |
|---|---|
| `Esc` | Close the window |
| `Enter` / `Space` | Launch the focused browser |
| Arrow keys | Move focus between browser tiles in the grid |
| `Tab` | Move focus to the next operable element |
| `1`-`9`, `A`-`Z` | Directly launch the browser assigned to that hotkey |
| `Ctrl` + `,` | Open the settings screen |
| `Ctrl` + Click / `Ctrl` + `Enter` | Launch the browser without closing the application |

## ⚙️ 設定 / Configuration

### 設定画面の開き方 / Opening Settings Screen

#### 日本語
1. メイン画面で「設定」ボタンをクリック
2. 設定ウィンドウが開きます

#### English
1. Click the "Settings" button on the main screen
2. The settings window opens

### 設定項目 / Configuration Options

#### 一般設定 / General Settings

##### 日本語
- **URLルールマッチ後に閉じる**: URLルールにマッチしたブラウザを起動後にアプリケーションを閉じる
- **言語設定**: アプリケーションの表示言語を選択（日本語・英語対応）
- **自動更新チェック / 更新チェック間隔**: 設定項目自体は存在するが、更新チェック機能自体が未実装（v0.3.0予定）のため現時点では効果を持たない

##### English
- **Close After URL Rule Match**: Close application after launching browser that matches URL rule
- **Language Settings**: Select application display language (Japanese/English support)
- **Automatic Update Check / Update Check Interval**: These settings exist in the configuration model, but have no effect yet since the update-check feature itself is not implemented (planned for v0.3.0)

#### 表示設定 / Display Settings

##### 日本語
- **背景色**: ウィンドウの背景色
- **背景グラデーション**: 背景グラデーションの有効/無効
- **グラデーション開始色**: グラデーションの開始色
- **グラデーション終了色**: グラデーションの終了色
- **グラデーション方向**: グラデーションの方向（縦・横・斜め）
- **初期ウィンドウサイズ**: 起動時のウィンドウサイズ（幅×高さ）
- **ロゴ表示**: アプリケーションロゴの表示/非表示
- **URL入力表示**: URL入力フィールドの表示/非表示

##### English
- **Background Color**: Window background color
- **Background Gradient**: Enable/disable background gradient
- **Gradient Start Color**: Gradient start color
- **Gradient End Color**: Gradient end color
- **Gradient Direction**: Gradient direction (vertical/horizontal/diagonal)
- **Initial Window Size**: Window size at startup (width × height)
- **Logo Display**: Show/hide application logo
- **URL Input Display**: Show/hide URL input field

#### ブラウザボタン設定 / Browser Button Settings

##### 日本語
- **ボタンサイズ**: ブラウザボタンの幅と高さ
- **ボタン背景色**: ボタンの背景色
- **ボタンテキスト色**: ボタンのテキスト色
- **ボタン透明度**: ボタンの透明度（0.1-1.0）
- **ボタン角丸半径**: ボタンの角の丸み（0-20px）
- **ブラウザ名表示**: ブラウザ名の表示/非表示
- **アイコンサイズ**: ブラウザアイコンのサイズ（16-64px）

##### English
- **Button Size**: Browser button width and height
- **Button Background Color**: Button background color
- **Button Text Color**: Button text color
- **Button Transparency**: Button transparency (0.1-1.0)
- **Button Corner Radius**: Button corner roundness (0-20px)
- **Browser Name Display**: Show/hide browser name
- **Icon Size**: Browser icon size (16-64px)

### 設定の保存 / Saving Settings

#### 日本語
設定を変更した後：
1. 「保存」ボタンをクリック
2. 設定が保存され、即座に反映されます

#### English
After changing settings:
1. Click the "Save" button
2. Settings are saved and immediately applied

### 設定のリセット / Resetting Settings

#### 日本語
設定を初期値に戻す場合：
1. 「リセット」ボタンをクリック
2. 確認ダイアログで「はい」を選択
3. 設定が初期値に戻ります

#### English
To reset settings to default values:
1. Click the "Reset" button
2. Select "Yes" in the confirmation dialog
3. Settings are reset to default values

## 🎨 外観設定 / Appearance Settings

### 日本語

v0.2.0で設定画面に「外観」タブが新設され、ガラスUIに関する各種設定をまとめて調整できるようになりました。

- **バックドロップ方式**: ウィンドウ背景の描画方式を選択
  - `Mica`: 既定の不透明多層ブラー
  - `Acrylic`: 半透明・強めのブラー
  - `MicaAlt`: タブ付きウィンドウ向けの濃いMica
  - 半透明単色: DWM非対応環境向けのフォールバック
  - 不透明: ハイコントラスト・低スペック環境向けの完全不透明表示
- **不透明度**: バックドロップの不透明度を調整
- **角丸半径**: ウィンドウ・タイルの角の丸みを調整（実際のDWM描画に反映）
- **タイトルバー表示切替**: タイトルバーの表示/非表示
- **常に最前面**: ウィンドウを常に最前面に表示するかどうか
- **テーマ**: ライト/ダーク/システム追従（OSのテーマ設定に自動追従）から選択

### English

v0.2.0 introduces a new "Appearance" tab in the settings screen, consolidating all glass-UI-related options in one place.

- **Backdrop Mode**: Choose how the window background is rendered
  - `Mica`: The default opaque multi-layer blur
  - `Acrylic`: Semi-transparent, stronger blur
  - `MicaAlt`: A denser Mica variant for tabbed windows
  - Solid Translucent: Fallback for environments without DWM support
  - Opaque: Fully opaque rendering for high-contrast or low-spec environments
- **Opacity**: Adjust the backdrop's opacity
- **Corner Radius**: Adjust the roundness of window/tile corners (reflected in actual DWM rendering)
- **Title Bar Toggle**: Show/hide the title bar
- **Always on Top**: Whether the window always stays on top of other windows
- **Theme**: Choose Light, Dark, or System (automatically follows the OS theme setting)

## 🚀 起動制御（トレイ常駐・自動起動・CLIオプション） / Startup Control (Tray Residency, Auto-launch, CLI Options)

### 日本語

v0.2.0で起動制御系の機能が追加されました。

- **トレイ常駐**: アプリケーションをシステムトレイに常駐させ、バックグラウンドで待機できます
- **カウントダウン自動起動**: 起動後、指定した秒数が経過すると自動的に選択中（またはデフォルト）のブラウザを起動します
- **CLIオプション**: コマンドラインから起動時の挙動を制御できます

| オプション | 説明 |
|---|---|
| `-d`, `--delay <秒>` | カウントダウン自動起動までの秒数を指定 |
| `-b`, `--browser <名前>` | 起動時に選択状態にする（または自動起動する）ブラウザを指定 |
| `--silent` | ブラウザ選択画面のUIを表示せず、既定ブラウザへ直接遷移 |
| `--auto-launch` | 起動後、カウントダウンを経て自動的にブラウザを起動 |
| `-h`, `--help` | ヘルプを表示 |
| `-v`, `--version` | バージョン情報を表示 |

### English

v0.2.0 adds startup-control features.

- **Tray Residency**: Keep the application resident in the system tray, running in the background
- **Countdown Auto-launch**: After a configured delay, automatically launches the selected (or default) browser
- **CLI Options**: Control startup behavior from the command line

| Option | Description |
|---|---|
| `-d`, `--delay <seconds>` | Sets the countdown delay before auto-launch |
| `-b`, `--browser <name>` | Specifies which browser to pre-select (or auto-launch) at startup |
| `--silent` | Skips the browser selection UI and goes directly to the default browser |
| `--auto-launch` | Automatically launches the browser after the countdown |
| `-h`, `--help` | Shows help text |
| `-v`, `--version` | Shows version information |

## 🔧 機能詳細 / Feature Details

### ブラウザ検出 / Browser Detection

#### 日本語
BrowserSelectorは以下の方法でブラウザを検出します：

1. **Windowsレジストリ**: インストールされているブラウザを自動検出
2. **一般的なブラウザ**: Chrome、Firefox、Edge、Safari、Opera等
3. **カスタムブラウザ**: 手動でブラウザを追加可能

#### English
BrowserSelector detects browsers using the following methods:

1. **Windows Registry**: Automatically detects installed browsers
2. **Common Browsers**: Chrome, Firefox, Edge, Safari, Opera, etc.
3. **Custom Browsers**: Manually add browsers

### カスタムブラウザの追加 / Adding Custom Browsers

#### 日本語
1. 設定画面を開く
2. 「ブラウザ設定」タブを選択
3. 「追加」ボタンをクリック
4. ブラウザ情報を入力：
   - 名前：表示名
   - 実行ファイル：ブラウザの実行ファイルパス
   - アイコン：アイコンファイルのパス（オプション）
   - 引数：起動時の引数（オプション）

#### English
1. Open settings screen
2. Select "Browser Settings" tab
3. Click "Add" button
4. Enter browser information:
   - Name: Display name
   - Executable: Browser executable file path
   - Icon: Icon file path (optional)
   - Arguments: Launch arguments (optional)

### URLルール管理 / URL Rule Management

#### 日本語
URLルールを使用して特定のURLパターンに対して自動的にブラウザを選択：

1. 設定画面を開く
2. 「URLルール設定」タブを選択
3. 「ルール追加」ボタンをクリック
4. ルール情報を入力：
   - パターン：URLパターン（正規表現）
   - ブラウザ：使用するブラウザ
   - 優先度：ルールの優先度
   - 有効/無効：ルールの状態

#### English
Use URL rules to automatically select browsers for specific URL patterns:

1. Open settings screen
2. Select "URL Rule Settings" tab
3. Click "Add Rule" button
4. Enter rule information:
   - Pattern: URL pattern (regular expression)
   - Browser: Browser to use
   - Priority: Rule priority
   - Enable/Disable: Rule status

### ログ管理 / Log Management

#### 日本語
アプリケーションの動作状況をログで確認：

1. 設定画面を開く
2. 「ログ設定」タブを選択
3. ログ設定を調整：
   - ログ有効/無効
   - ログレベル（Debug、Information、Warning、Error）
   - ファイルログの設定
   - ログファイルの場所

#### English
Check application operation status through logs:

1. Open settings screen
2. Select "Log Settings" tab
3. Adjust log settings:
   - Enable/disable logs
   - Log level (Debug, Information, Warning, Error)
   - File log settings
   - Log file location

## 🔄 自動アップデート / Auto-update

### 日本語

**自動アップデート機能は現時点では未実装です。** `IUpdateService`のインターフェース定義とサービスの骨格のみが実装されており、GitHub Releases連携・ダウンロード・インストール・ロールバックといった実際の更新処理は動作しません。この機能は**v0.3.0で実装予定**です。

現在、最新バージョンを入手するには以下の手動手順を使用してください：
1. [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)にアクセス
2. 最新のインストーラーをダウンロード
3. インストーラーを管理者権限で実行し、上書きインストール

### English

**Automatic update functionality is not yet implemented.** Only the `IUpdateService` interface definition and a minimal service skeleton exist; GitHub Releases integration, downloading, installation, and rollback do not actually function yet. This feature is **planned for v0.3.0**.

For now, obtain the latest version manually:
1. Visit [GitHub Releases](https://github.com/Yosuke-Sh/BrowserSelector/releases)
2. Download the latest installer
3. Run the installer with administrator privileges to overwrite-install

## 🛠️ トラブルシューティング / Troubleshooting

### よくある問題 / Common Issues

#### アプリケーションが起動しない / Application Won't Start

##### 日本語
**原因と対処法：**
- .NET 10.0 Runtimeがインストールされていない
  → [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)をインストール
- ウイルス対策ソフトがブロックしている
  → ウイルス対策ソフトの除外設定に追加
- 管理者権限が必要
  → 管理者として実行

##### English
**Causes and Solutions:**
- .NET 10.0 Runtime is not installed
  → Install [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- Antivirus software is blocking
  → Add to antivirus software exclusion settings
- Administrator privileges required
  → Run as administrator

#### ブラウザが検出されない / Browsers Not Detected

##### 日本語
**原因と対処法：**
- ブラウザが正しくインストールされていない
  → ブラウザを再インストール
- レジストリに問題がある
  → 手動でブラウザを追加
- カスタムブラウザの場合
  → 設定画面で手動追加

##### English
**Causes and Solutions:**
- Browser is not properly installed
  → Reinstall browser
- Registry issues
  → Manually add browser
- For custom browsers
  → Manually add in settings screen

#### 設定が保存されない / Settings Not Saving

##### 日本語
**原因と対処法：**
- ファイルの書き込み権限がない
  → 管理者として実行
- 設定ファイルが破損している
  → 設定をリセット
- ディスク容量不足
  → ディスク容量を確保

##### English
**Causes and Solutions:**
- No file write permissions
  → Run as administrator
- Settings file is corrupted
  → Reset settings
- Insufficient disk space
  → Free up disk space

#### アップデートが失敗する / Update Fails

##### 日本語
**原因と対処法：**
- ネットワーク接続の問題
  → インターネット接続を確認
- ウイルス対策ソフトがブロック
  → ウイルス対策ソフトの除外設定に追加
- 管理者権限が必要
  → 管理者として実行

##### English
**Causes and Solutions:**
- Network connection issues
  → Check internet connection
- Antivirus software blocking
  → Add to antivirus software exclusion settings
- Administrator privileges required
  → Run as administrator

### ログファイル / Log Files

#### 日本語
問題の詳細を確認する場合：
1. 設定で「ログ有効」を有効
2. ログファイルの場所：`%AppData%\BrowserSelector\logs\`
3. ログファイルを確認して問題を特定

#### English
To check problem details:
1. Enable "Log Enabled" in settings
2. Log file location: `%AppData%\BrowserSelector\logs\`
3. Check log files to identify problems

### 設定ファイルの場所 / Configuration File Locations

#### 日本語
- **設定ファイル**: `%AppData%\BrowserSelector\settings.json`
- **ログファイル**: `%AppData%\BrowserSelector\logs\`
- **バックアップ**: `%AppData%\BrowserSelector\backup\`

#### English
- **Settings File**: `%AppData%\BrowserSelector\settings.json`
- **Log Files**: `%AppData%\BrowserSelector\logs\`
- **Backup**: `%AppData%\BrowserSelector\backup\`

## ❓ FAQ

### Q: どのブラウザがサポートされていますか？ / Q: Which browsers are supported?

#### 日本語
A: 一般的なブラウザ（Chrome、Firefox、Edge、Safari、Opera等）を自動検出します。その他のブラウザは手動で追加できます。

#### English
A: Automatically detects common browsers (Chrome, Firefox, Edge, Safari, Opera, etc.). Other browsers can be added manually.

### Q: デフォルトブラウザとして設定できますか？ / Q: Can it be set as the default browser?

#### 日本語
A: はい、インストール時にデフォルトブラウザとして設定するオプションがあります。また、後からWindowsの設定からも変更可能です。

#### English
A: Yes, there is an option to set it as the default browser during installation. It can also be changed later from Windows settings.

### Q: 設定はどこに保存されますか？ / Q: Where are settings saved?

#### 日本語
A: 設定は`%AppData%\BrowserSelector\`フォルダに保存されます。

#### English
A: Settings are saved in the `%AppData%\BrowserSelector\` folder.

### Q: 複数のユーザーで使用できますか？ / Q: Can it be used by multiple users?

#### 日本語
A: はい、各ユーザーが個別の設定を持つことができます。

#### English
A: Yes, each user can have individual settings.

### Q: アンインストール方法は？ / Q: How to uninstall?

#### 日本語
A: コントロールパネルの「プログラムの追加と削除」からアンインストールできます。

#### English
A: Can be uninstalled from Control Panel "Add or Remove Programs".

### Q: 自動アップデート機能はありますか？ / Q: Is there an automatic update feature?

#### 日本語
A: 現時点では未実装です（v0.3.0で実装予定）。最新版はGitHub Releasesから手動でダウンロードしてください。

#### English
A: Not yet implemented (planned for v0.3.0). Please download the latest version manually from GitHub Releases for now.

## 📞 サポート / Support

### 問題の報告 / Issue Reporting

#### 日本語
問題が発生した場合：
1. [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)で報告
2. 問題の詳細とログファイルを添付
3. 再現手順を明記

#### English
When problems occur:
1. Report on [GitHub Issues](https://github.com/Yosuke-Sh/BrowserSelector/issues)
2. Attach problem details and log files
3. Specify reproduction steps

### 機能要望 / Feature Requests

#### 日本語
新機能の要望がある場合：
1. [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)で提案
2. 使用例と期待する動作を説明

#### English
For new feature requests:
1. Propose on [GitHub Discussions](https://github.com/Yosuke-Sh/BrowserSelector/discussions)
2. Explain usage examples and expected behavior

### ドキュメント / Documentation

#### 日本語
詳細な情報は以下を参照：
- [プロジェクトWiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)
- [API仕様書](docs/API.md)
- [開発者ガイド](CONTRIBUTING.md)

#### English
For detailed information, refer to:
- [Project Wiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki)
- [API Specification](docs/API.md)
- [Developer Guide](CONTRIBUTING.md)

---

### 日本語
**BrowserSelector** - 賢いブラウザ選択を！ 🌐✨

### English
**BrowserSelector** - Choose Your Browser Wisely! 🌐✨
