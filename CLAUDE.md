# CLAUDE.md

BrowserSelector: Windows用の複数ブラウザ選択・起動WPFアプリケーション。旧WinFormsアプリを0ベースでWPF + MVVMに刷新したもの。

## 現在の状況（2025-09-13時点、最新コミット基準）

- **バージョン**: v0.1.1（品質改善完了）。v0.1.0は初期リリース済み、インストーラーサイズ最適化済み（50.79MB → 4.12MB）。
- **進行中**: v0.2.0 起動パフォーマンス最適化（起動時間3秒→2秒、メモリ100MB→50MB目標）。
- **予定**: v0.3.0 自動アップデート機能（GitHub Releases連携、差分更新、ロールバック）。
- テスト: 702/702成功、警告0件を維持中（このプロジェクトの最重要方針）。
- 詳細は [PROJECT_STATUS_AND_PLAN.md](PROJECT_STATUS_AND_PLAN.md) を参照（実装済み/未実装機能の一覧、テスト内訳、警告是正計画あり）。このファイルは頻繁に更新されるため、作業前に必ず読むこと。

## アーキテクチャ

レイヤードアーキテクチャ + MVVM。`.cursor/rules/global.mdc` にある「理想形」の詳細なフォルダ構成は初期計画であり、実際の構成とは異なる（例: `ValueObjects/`, `Behaviors/` 等は未実装、`BrowserSelector.Library.*` という別レイヤー群が追加されている）。実装の正とするのは常に `src/` の実物。

```
src/
├── BrowserSelector.Core/            # ドメイン層（Models, Services interfaces, Enums, Converters）
├── BrowserSelector.Infrastructure/  # インフラ層（Localization, Logging, SystemIntegration, Updates, Services実装）
├── BrowserSelector.Presentation/    # プレゼンテーション層（ViewModels, Views, Controls, Behaviors, Converters, Extensions, Helpers, Resources）
├── BrowserSelector.App/             # アプリケーション層（起動、DI設定, App.xaml）
└── BrowserSelector.Library/         # 別系統のライブラリレイヤー（Core/Infrastructure/Presentation）

tests/
├── BrowserSelector.UnitTests / CoreTests / InfrastructureTests
├── BrowserSelector.IntegrationTests
├── BrowserSelector.UITests          # FlaUI
├── BrowserSelector.E2ETests         # Playwright for .NET
├── BrowserSelector.SecurityTests
├── BrowserSelector.AppTests         # Appプロジェクトのリフレクションベーステスト
├── BrowserSelector.LibraryTests
└── BrowserSelector.PerformanceTests # BenchmarkDotNet（xUnitランナーからは除外）
```

## 技術スタック

- WPF (.NET 8.0) / CommunityToolkit.Mvvm / Microsoft.Extensions.DependencyInjection
- 設定: Microsoft.Extensions.Configuration（JSON） / ログ: Microsoft.Extensions.Logging + Serilog
- 多言語: Microsoft.Extensions.Localization（日本語・英語、動的切替対応）
- テスト: xUnit + Moq + FluentAssertions（単体/統合）、MSTest + FlaUI（UI）、NUnit + Playwright（E2E）、BenchmarkDotNet（性能）
- 配布: Inno Setup（インストーラー）

## ビルド・テストの絶対ルール

このプロジェクトの最優先方針は**警告ゼロの維持**。機能実装より警告解消を優先する。

```bash
# 警告確認は必ずクリーンビルドから（キャッシュ由来の誤カウントを防ぐ）
dotnet clean
dotnet build

# テスト実行
dotnet test

# テスト修正時の必須順序: clean → build → 警告ゼロ確認 → test
```

- ビルド警告・エラーは全て根本対応する。安易な `#pragma warning disable` や解析ルール無効化は行わない（正当な理由がある場合のみ最小スコープで抑制し、理由をコメントに明記）。
- 警告種別ごとにコミットをバッチ化する（例: CA1707違反を一括修正して単一コミットにする）。
- コミットはビルド・テストが全て通ってから。コミットメッセージは日本語で詳細に記載。
- ビルドログの一時ファイルは `tmp/`（`.gitignore`済み）に `build_*.log` 形式で置く。
- PowerShellでビルド出力を一貫させるため `$env:DOTNET_CLI_UI_LANGUAGE="en"` を使う。

## ブランチ戦略

個人開発のため、`feature/*`・`release/*`・`hotfix/*`のような細かいブランチ分割は行わない簡素な2本体制。

- `master`: 本番リリース用（GitHub上のデフォルトブランチ、保護、PR必須）
- `developer`: 開発用ブランチ（**通常はここで直接作業する**）

フロー: `developer`で開発 → `master`へPRを作成してマージ → 必要に応じて`master`にリリースタグを打つ

## 開発の進め方

1. 着手前に既存の類似機能・同名/類似名の関数やコンポーネント・重複APIの有無を確認する（重複実装防止）。
2. 指示された範囲外の追加実装はしない。不明点や重要な判断が必要な場合は確認を取る。
3. 新機能には対応するテストを追加する。ログ出力は既存のLoggerクラス経由で行う。
4. 実装完了後はユーザーテストを実施してからコミットする。

## ドキュメント構成

- [README.md](README.md) — プロダクト概要、機能一覧、インストール手順（日英併記）
- [PROJECT_STATUS_AND_PLAN.md](PROJECT_STATUS_AND_PLAN.md) — 実装状況・テスト状況・v0.2/v0.3計画。**最も頻繁に更新される、状況把握の一次情報源**
- [CHANGELOG.md](CHANGELOG.md) — Keep a Changelog形式のリリース履歴とロードマップ
- [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) — 貢献ガイド、PRテンプレート、コーディング規約
- [docs/API.md](docs/API.md), [docs/USER_MANUAL.md](docs/USER_MANUAL.md), [docs/RELEASE_NOTES_v0.1.0.md](docs/RELEASE_NOTES_v0.1.0.md)
- [GitHub Wiki](https://github.com/Yosuke-Sh/BrowserSelector/wiki) — `docs/USER_MANUAL.md` の内容をエンドユーザー向けに公開したもの。実体は別リポジトリ（`https://github.com/Yosuke-Sh/BrowserSelector.wiki.git`）で、`git clone` して編集・pushする。**バージョンを追従させる責任はリポジトリ側にあり、放置すると自動では同期されない**

ドキュメントを更新する際は、機能追加・仕様変更の内容に応じて README.md / PROJECT_STATUS_AND_PLAN.md / CHANGELOG.md の該当箇所を揃えて更新すること（状況の食い違いを防ぐため）。

## リリース手順

`developer` → `master` へのマージが完了し、リリースタグ（`vX.Y.Z`）をpushする際は、以下を漏れなく実施する。

1. `docs/USER_MANUAL.md` を最新の機能・設定項目に更新する（README.md / PROJECT_STATUS_AND_PLAN.md / CHANGELOG.md と同様、リリースのたびに内容が古びやすい）
2. **GitHub Wikiを更新する**（`docs/USER_MANUAL.md` の内容を反映）:
   ```bash
   git clone https://github.com/Yosuke-Sh/BrowserSelector.wiki.git
   # Home.md を docs/USER_MANUAL.md ベースで更新（Wiki内リンクはリポジトリへの絶対URLに変換すること）
   git add Home.md && git commit -m "..." && git push
   ```
3. `vX.Y.Z` タグをpushし、`.github/workflows/release.yml` の完走を確認する（インストーラー・ポータブルZIP・SHA256SUMS.txtがGitHub Releaseに添付されること）

過去にWiki更新が抜け落ち、v0.1.0時代の内容（.NET 8.0前提、未実装の自動アップデート機能を実装済みと誤記）のまま放置された実績があるため、**タグpush前のチェックリストとして必ず参照すること**。

## サブエージェント

- **dotnet-quality-guardian**: ビルド警告ゼロ化・テスト全件成功を維持するための専用エージェント。`dotnet clean/build/test` サイクルを回し、警告を種別ごとに一括修正してコミット単位を提案する。「警告直して」「テスト直して」「クリーンビルドして確認して」等で使う。
