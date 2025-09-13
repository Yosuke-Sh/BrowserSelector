# ファイルヘッダーとusing文の修正スクリプト

param(
    [string]$ProjectPath = "src/BrowserSelector.Core"
)

$headerTemplate = @"
// <copyright file="{0}" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

"@

# .csファイルを取得
$csFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $fileName = $file.Name
    
    # ファイルヘッダーが既に存在するかチェック
    if ($content -notmatch "// <copyright") {
        # ファイルヘッダーを追加
        $header = $headerTemplate -f $fileName
        $content = $header + $content
    }
    
    # using文をnamespace内に移動
    if ($content -match "using\s+[^;]+;" -and $content -match "namespace\s+([^;{]+)") {
        $namespace = $matches[1].Trim()
        
        # using文を抽出
        $usingStatements = [regex]::Matches($content, "using\s+[^;]+;")
        $usingBlock = ""
        foreach ($match in $usingStatements) {
            $usingBlock += "    " + $match.Value + "`n"
        }
        
        # using文を削除
        $content = [regex]::Replace($content, "using\s+[^;]+;`n?", "")
        
        # namespaceを修正
        $content = $content -replace "namespace\s+$namespace;", "namespace $namespace`n{`n$usingBlock"
        
        # ファイルの最後に閉じ括弧を追加
        if ($content -notmatch "`n}$") {
            $content += "`n}"
        }
    }
    
    # ファイルに書き戻し
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8
    Write-Host "修正完了: $($file.FullName)"
}

Write-Host "ファイルヘッダーとusing文の修正が完了しました。"
