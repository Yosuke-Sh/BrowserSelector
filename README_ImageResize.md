# BrowserSelector 画像リサイズスクリプト (PowerShell版)

## 概要
このスクリプトは、BrowserSelectorのアイコンとロゴ画像を指定されたサイズにリサイズするPowerShellスクリプトです。

## 必要なファイル
スクリプトを実行する前に、以下のファイルが指定されたフォルダに配置されていることを確認してください：

- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon.png` - アイコン用の元画像
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo.png` - ロゴ用の元画像

## 使用方法

### 方法1: PowerShellで直接実行（推奨）
```powershell
# 基本的な実行
.\resize_images.ps1

# 既存ファイルを上書き
.\resize_images.ps1 -Force

# ヘルプを表示
Get-Help .\resize_images.ps1
```

### 方法2: コマンドプロンプトから実行
```cmd
# PowerShellスクリプトを直接実行
powershell -ExecutionPolicy Bypass -File "resize_images.ps1"

# パラメータ付きで実行
powershell -ExecutionPolicy Bypass -File "resize_images.ps1" -Force
```

## パラメータ

| パラメータ | 説明 | デフォルト値 |
|------------|------|--------------|
| `-Force` | 既存ファイルを上書きするかどうか | `$false` |

## 出力サイズ

### アイコン (BrowserSelector_Icon.png)
- 16x16 ピクセル
- 32x32 ピクセル
- 48x48 ピクセル
- 256x256 ピクセル

### ロゴ (BrowserSelector_Logo.png)
- 120x120 ピクセル
- 180x180 ピクセル
- 240x240 ピクセル

## 出力ファイル名と場所

### アイコン
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon_16.png`
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon_32.png`
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon_48.png`
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon_256.png`

### ロゴ
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo_120.png`
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo_180.png`
- `D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo_240.png`

## 技術仕様

### リサイズ品質
- **補間方式**: HighQualityBicubic
- **スムージング**: HighQuality
- **ピクセルオフセット**: HighQuality

### 出力形式
- 形式: PNG
- 透明度: 保持
- 品質: 高品質

## 実行例

### 例1: 基本的な実行
```powershell
PS D:\Project\BrowserSelector> .\resize_images.ps1
BrowserSelector Image Resize Script
===================================

Image folder: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images

Processing icon file: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon.png
  Creating: BrowserSelector_Icon_16.png (16 x 16)
  Resizing: 16x16... OK
  OK: BrowserSelector_Icon_16.png created
  Creating: BrowserSelector_Icon_32.png (32 x 32)
  Resizing: 32x32... OK
  OK: BrowserSelector_Icon_32.png created
  Creating: BrowserSelector_Icon_48.png (48 x 48)
  Resizing: 48x48... OK
  OK: BrowserSelector_Icon_48.png created
  Creating: BrowserSelector_Icon_256.png (256 x 256)
  Resizing: 256x256... OK
  OK: BrowserSelector_Icon_256.png created

Processing logo file: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo.png
  Creating: BrowserSelector_Logo_120.png (120 x 120)
  Resizing: 120x120... OK
  OK: BrowserSelector_Logo_120.png created
  Creating: BrowserSelector_Logo_180.png (180 x 180)
  Resizing: 180x180... OK
  OK: BrowserSelector_Logo_180.png created
  Creating: BrowserSelector_Logo_240.png (240 x 240)
  Resizing: 240x240... OK
  OK: BrowserSelector_Logo_240.png created

Processing complete!
Output folder: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images

Created files:
  BrowserSelector_Icon_16.png
  BrowserSelector_Icon_32.png
  BrowserSelector_Icon_48.png
  BrowserSelector_Icon_256.png
  BrowserSelector_Logo_120.png
  BrowserSelector_Logo_180.png
  BrowserSelector_Logo_240.png

Script complete. Press any key to exit...
```

### 例2: 既存ファイルの上書き
```powershell
PS D:\Project\BrowserSelector> .\resize_images.ps1 -Force
```

## トラブルシューティング

### よくある問題

#### 1. 実行ポリシーのエラー
```powershell
# 実行ポリシーを変更
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

#### 2. 画像フォルダが見つからない
```
エラー: 画像フォルダが見つかりません: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images
```
**対処法**: 
- フォルダパスが正しいか確認
- フォルダが存在するか確認
- アクセス権限があるか確認

#### 3. 画像ファイルが見つからない
```
警告: アイコンファイルが見つかりません: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Icon.png
警告: ロゴファイルが見つかりません: D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images\BrowserSelector_Logo.png
```
**対処法**: 
- ファイル名が正確か確認（大文字小文字も含む）
- ファイルが指定されたフォルダに配置されているか確認
- ファイルが存在するか確認

#### 4. 権限エラー
**対処法**: 
- 出力先フォルダに書き込み権限があるか確認
- 管理者権限で実行を試行
- アンチウイルスソフトの干渉を確認

### エラーメッセージの例
```
FAILED: BrowserSelector_Icon_16.png の作成に失敗しました
FAILED: BrowserSelector_Logo_120.png の作成に失敗しました
```

## 注意事項

1. **元画像の品質**: 高解像度の元画像を使用することで、より良い結果が得られます
2. **ファイルサイズ**: 大きなサイズ（256x256など）は、ファイルサイズが大きくなる可能性があります
3. **透明度**: PNG形式の透明度は保持されます
4. **リソース管理**: スクリプトは適切にリソースを解放します
5. **既存ファイル**: 同じ名前のファイルが既に存在する場合はスキップされます（-Forceオプションで上書き可能）
6. **フォルダパス**: ハードコードされたパスを使用しているため、プロジェクトの場所が変更された場合は修正が必要です

## 依存関係

### 必須
- PowerShell 5.1以上
- .NET Framework（System.Drawingアセンブリ用）

## カスタマイズ

### サイズの変更
スクリプト内の以下の行を編集して、サイズを変更できます：

```powershell
$iconSizes = @(16, 32, 48, 256)
$logoSizes = @(120, 180, 240)
```

### フォルダパスの変更
スクリプト内の以下の行を編集して、フォルダパスを変更できます：

```powershell
$ImageFolder = "D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images"
```

## ライセンス
このスクリプトは、BrowserSelectorプロジェクトの一部として提供されています。
