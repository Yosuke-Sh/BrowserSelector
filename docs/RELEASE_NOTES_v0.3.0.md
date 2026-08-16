# BrowserSelector v0.3.0 リリースノート

**リリース日**: 2026年8月16日
**バージョン**: v0.3.0
**コードネーム**: "Auto Update"

## 🎉 概要

BrowserSelector v0.3.0は、GitHub Releasesと連携した自動アップデート機能を実装するリリースです。v0.2.0時点の`IUpdateService`は「原理的に動作しない骨格」の状態でした（GitHub APIレスポンスのマッピング欠落・拡張子なしダウンロード・`AppContext.BaseDirectory`をファイルパス扱いするバグ等）。本リリースではこれらを土台から作り直し、GitHub API正しいマッピング・SHA256による完全性検証・別プロセス（`BrowserSelector.Updater.exe`）による適用・非モーダルな通知UI・起動を阻害しないバックグラウンドチェックを実装しました。

## ✨ 主な変更点

### 🔄 自動アップデート機能

- **更新確認**: 起動5秒後にバックグラウンドで実行（起動→ブラウザ選択の動線を阻害しない）。ETagによる条件付きリクエストとレート制限抑止付き
- **完全性検証**: `SHA256SUMS.txt`によるSHA256照合。コード署名を持たないため、これが「掴まされた成果物が本物か」を保証する唯一の手段。不一致・取得不可の場合はファイルを削除して適用しない
- **2つの適用経路**:
  - **インストーラー版**（既定のProgram Filesインストール）: インストーラーを`/SILENT /CLOSEAPPLICATIONS`でUAC昇格起動
  - **ポータブル版**: 別プロセス`BrowserSelector.Updater.exe`が本体プロセスの終了を待ち、バックアップ・ファイル置換・再起動までを行う
- **非モーダル通知UI**: メインウィンドウ最下部に「今すぐ更新」「次回起動時」「このバージョンをスキップ」「リリースノート」を表示。ダイアログでの操作ブロックは一切行わない
- **設定画面**: 「プレリリースを含める」「最終チェック日時」「今すぐ確認」「スキップを解除」を一般タブへ追加
- **ホスト検証**: `api.github.com`/`github.com`は完全一致、アセット配信は`.githubusercontent.com`サフィックス厳密一致。`evil-githubusercontent.com`のようなサフィックス偽装は拒否。リダイレクト後の最終URLも再検証
- **Zip Slip対策**: ポータブルZIP展開時に相対パストラバーサル・絶対パス・代替データストリームを拒否し、エントリ数・展開後サイズの上限（ZIP爆弾対策）を設定

### 🔧 破壊的変更

- **`IUpdateService`**: `CheckForUpdatesAsync`/`DownloadUpdateAsync`/`ApplyUpdateAsync`/`ResolveChannel`の新シグネチャへ刷新。`RollbackUpdateAsync`/`CreateBackup`は削除し`BrowserSelector.Updater.exe`側へ移設（アプリが動いていない状態でしか意味をなさないため）
- **`UpdateInfo`**: 18プロパティから11プロパティへ削減。`Version`は`string`から`System.Version`へ変更。アセット詳細は新設の`UpdateAsset`（`InstallerAsset`/`PortableAsset`/`ChecksumsAsset`）へ移動
- **HTTP通信**: `IHttpClientFactory`を採用（従来のSingleton `HttpClient`から変更）。本番と同じ経路にスタブを差し込めるようになりテスト容易性が向上

### 🐛 修正されたバグ（v0.2.0以前から存在）

- GitHub APIのレスポンスを`UpdateInfo`へ直接デシリアライズしており、`tag_name`/`assets[]`に対応するプロパティが存在しなかったため**更新が原理的に検出されなかった**問題 → `GitHubReleaseMapper`による正しいマッピングで修正
- ダウンロード先が`Path.GetRandomFileName()`で拡張子なしに保存されており、インストーラーを起動できなかった問題 → 元のアセット名（拡張子込み）で保存するよう修正
- `AppContext.BaseDirectory`（ディレクトリ）をファイルパスとしてバックアップ・ロールバックに渡しており、常に無効化されていた問題 → パスベースのチャネル判定・Updater.exe側の適切なファイル操作で修正

### 📦 新規プロジェクト

- **`BrowserSelector.Updater`**: ポータブル配置の更新適用を担う独立プロセス。Core/Infrastructureへの依存を持たない（置換対象のDLLをロードするとファイルロックで置換が失敗するため）
- **`BrowserSelector.UpdaterTests`**: 上記のテストプロジェクト（36テスト）

## 🧪 テスト

- **総テスト数**: 858テスト全成功、警告0件（v0.2.0の658テストから+200）
  - UnitTests: 255テスト
  - CoreTests: 49テスト
  - InfrastructureTests: 160テスト（GitHub APIマッピング・チェックサム解析・ダウンロード検証・セキュリティテスト等を大幅追加）
  - IntegrationTests: 23テスト
  - SecurityTests: 238テスト
  - AppTests: 88テスト
  - UITests: 5テスト
  - E2ETests: 4テスト
  - UpdaterTests: 36テスト（新設）
  - PerformanceTests: BenchmarkDotNetベースのためxUnitランナーの集計対象外

## 🐛 既知の問題

- **コード署名**: 実行ファイル・インストーラーは現時点で未署名（ホスト検証・SHA256検証で代替）
- **Updater自己更新**: `BrowserSelector.Updater.exe`自体はポータブル更新時に1世代遅れが許容される仕様（`.new`として置き、次回のインストーラー更新で正規化）

## 🔭 スコープ外（意図的な決定事項）

- **コード署名**: リポジトリに証明書が存在しないため
- **真の差分更新（バイナリdelta）**: 配布物が数MBのため費用対効果が無いと判断
- **プレリリース配信運用**: 設定項目とフィルタは実装済みだが、`release.yml`側でbetaタグを打つ運用は現時点で予定なし

## 🔜 今後の予定

### v0.4.0以降（候補）
- コード署名の導入
- 真の差分更新
- 監視・分析機能、多言語拡張、アクセシビリティ完全実装

## 📚 ドキュメント

- [ユーザーマニュアル](USER_MANUAL.md) — 「アップデート」章を全面刷新、FAQに「アップデートが検出されない場合」を追加
- [API仕様書](API.md) — `IUpdateService`節を全面書き換え
- [PROJECT_STATUS_AND_PLAN.md](../PROJECT_STATUS_AND_PLAN.md) — 実装状況・テスト状況の最新版

## 📄 ライセンス

本ソフトウェアはMITライセンスの下で提供されています。詳細は`LICENSE`ファイルを参照してください。

---

**BrowserSelector v0.3.0** - 常に最新版へ、安全に。

*このリリースノートは2026年8月16日に作成されました。*
