# コード品質分析スクリプト
param(
    [string]$Configuration = "Release",
    [switch]$SkipTests = $false,
    [switch]$GenerateReport = $true
)

Write-Host "コード品質分析を開始します..." -ForegroundColor Green

# プロジェクトのビルド
Write-Host "プロジェクトをビルドしています..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "ビルドに失敗しました"
    exit 1
}

# テストの実行（スキップしない場合）
if (-not $SkipTests) {
    Write-Host "テストを実行しています..." -ForegroundColor Yellow
    $testResult = dotnet test --configuration $Configuration --verbosity minimal --collect:"XPlat Code Coverage"
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "テストの実行に問題がありました"
    }
}

# コード品質レポートの生成
if ($GenerateReport) {
    Write-Host "コード品質レポートを生成しています..." -ForegroundColor Yellow
    
    # カバレッジレポートの生成
    if (Test-Path "TestResults") {
        Write-Host "カバレッジレポートを生成中..." -ForegroundColor Cyan
        reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"quality-report" -reporttypes:"Html;TextSummary"
    }
    
    # 依存関係の分析
    Write-Host "依存関係を分析中..." -ForegroundColor Cyan
    dotnet list src/BrowserSelector.App package --include-transitive > quality-report/dependencies.txt
    
    Write-Host "コード品質レポートが生成されました: quality-report/" -ForegroundColor Green
}

# 警告の確認
Write-Host "ビルド警告を確認しています..." -ForegroundColor Yellow
$warnings = $buildResult | Select-String "warning"
if ($warnings) {
    Write-Host "以下の警告が検出されました:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host "  - $($_.Line)" -ForegroundColor Yellow }
} else {
    Write-Host "ビルド警告は検出されませんでした" -ForegroundColor Green
}

# コード品質のサマリー
Write-Host "`nコード品質分析サマリー:" -ForegroundColor Green
Write-Host "  - ビルド: 成功" -ForegroundColor Green
if (-not $SkipTests) {
    Write-Host "  - テスト: 実行完了" -ForegroundColor Green
}
if ($GenerateReport) {
    Write-Host "  - レポート: 生成完了" -ForegroundColor Green
}
if ($warnings) {
    Write-Host "  - 警告: 検出" -ForegroundColor Yellow
} else {
    Write-Host "  - 警告: なし" -ForegroundColor Green
}

Write-Host "`nコード品質分析が完了しました!" -ForegroundColor Green