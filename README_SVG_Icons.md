# PNGアイコン使用ガイド

## 📁 作成されたPNGアイコンファイル

### 🎯 メイン画面用アイコン
- **`Icon_Settings_Gray.png`** - メイン画面の設定ボタン用（灰色の歯車）
- **`Icon_Globe.png`** - URL入力フィールド用（グローブアイコン）

### ⚙️ 設定画面タブ用アイコン
- **`Icon_General.png`** - 一般設定タブ（青い歯車）
- **`Icon_Display.png`** - 表示設定タブ（目のアイコン）
- **`Icon_Browser.png`** - ブラウザ設定タブ（ブラウザアイコン）
- **`Icon_Accessibility.png`** - アクセシビリティ設定タブ（アクセシビリティアイコン）

### 🔧 設定画面ボタン用アイコン
- **`Icon_Refresh.png`** - ブラウザ再検出ボタン（更新アイコン）
- **`Icon_Add.png`** - ブラウザ追加ボタン（追加アイコン）
- **`Icon_Reset.png`** - 設定リセットボタン（リセットアイコン）
- **`Icon_Import.png`** - 設定インポートボタン（インポートアイコン）
- **`Icon_Export.png`** - 設定エクスポートボタン（エクスポートアイコン）
- **`Icon_Check.png`** - OKボタン（チェックアイコン）
- **`Icon_Close.png`** - キャンセルボタン（クローズアイコン）

## 🎨 アイコンの特徴

### カラーパレット
- **プライマリカラー**: `#2196F3` (青) - タブヘッダー、主要ボタン
- **セカンダリカラー**: `#666666` (グレー) - メイン画面の設定ボタン、グローブアイコン
- **アクセントカラー**: 
  - `#4CAF50` (緑) - インポート、OKボタン
  - `#FF9800` (オレンジ) - エクスポートボタン
  - `#FF5722` (赤) - リセットボタン
  - `#F44336` (赤) - キャンセルボタン

### デザイン仕様
- **サイズ**: 16x16ピクセル
- **形式**: PNG
- **スタイル**: Material Design風のフラットデザイン

## 🔧 使用方法

### 1. XAMLでの基本的な使用方法
```xml
<!-- シンプルな画像表示 -->
<Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_Settings_Gray.png" 
       Width="16" Height="16" />

<!-- ボタンにアイコンを追加 -->
<Button>
    <StackPanel Orientation="Horizontal">
        <Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_Check.png" 
               Width="16" Height="16" 
               Margin="0,0,8,0"/>
        <TextBlock Text="OK" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

### 2. タブヘッダーでの使用方法
```xml
<TabItem>
    <TabItem.Header>
        <StackPanel Orientation="Horizontal">
                    <Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_General.png" 
               Width="16" Height="16" 
               Margin="0,0,8,0"/>
            <TextBlock Text="一般" VerticalAlignment="Center"/>
        </StackPanel>
    </TabItem.Header>
    <!-- タブコンテンツ -->
</TabItem>
```

### 3. コードビハインドでの使用方法
```csharp
// 動的にアイコンを設定
public void SetIcon(string iconName)
{
    var image = new Image();
    image.Source = new BitmapImage(
        new Uri($"pack://application:,,,/Resources/Images/{iconName}.png"));
    image.Width = 16;
    image.Height = 16;
    
    // コントロールに追加
    container.Children.Add(image);
}
```

## 📱 レスポンシブ対応

### スケーラビリティ
- PNG形式のため、高品質な表示
- 高解像度ディスプレイでも鮮明に表示
- 16x16ピクセルに最適化

### 推奨サイズ
- **小**: 12x12px - ツールバー、メニュー
- **標準**: 16x16px - ボタン、タブ（現在の設定）
- **大**: 24x24px - ヘッダー、強調表示
- **特大**: 32x32px - メインアイコン

## 🎨 カスタマイズ

### 色の変更
```xml
<!-- フィルターを使用して色を変更 -->
<Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_Settings_Gray.png">
    <Image.Effect>
        <DropShadowEffect Color="Red" BlurRadius="0" ShadowDepth="0"/>
    </Image.Effect>
</Image>
```

### サイズの変更
```xml
<!-- 幅と高さを個別に設定 -->
<Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_General.png" 
       Width="24" Height="20" />
```

## 🔍 トラブルシューティング

### よくある問題
1. **アイコンが表示されない**
   - ファイルパスが正しいか確認
   - ビルドアクションが「リソース」になっているか確認

2. **アイコンが小さすぎる/大きすぎる**
   - Width/Heightプロパティを調整
   - 親コントロールのサイズ制約を確認

3. **色が期待と異なる**
   - SVGファイルのfill属性を確認
   - アプリケーションのテーマカラーとの競合を確認

### デバッグ方法
```xml
<!-- デバッグ用：ボーダーを表示 -->
<Image Source="/BrowserSelector.Presentation;component/Resources/Images/Icon_General.png" 
       Width="16" Height="16" 
       BorderBrush="Red" 
       BorderThickness="1" />
```

## 📚 参考資料

### PNG仕様
- [PNG Specification](https://www.w3.org/TR/PNG/)
- [WPF Image Support](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/graphics-overview)

### デザインガイドライン
- [Material Design Icons](https://material.io/design/iconography/system-icons.html)
- [Windows Design Guidelines](https://docs.microsoft.com/en-us/windows/uwp/design/)

## 🚀 今後の拡張

### 追加予定アイコン
- ブラウザ固有アイコン（Chrome、Firefox、Edge等）
- システムトレイアイコン
- 通知アイコン
- ヘルプ・情報アイコン

### アニメーション対応
- ホバー効果
- クリックアニメーション
- ローディングアニメーション

---

**作成日**: 2024年12月
**バージョン**: 1.0
**作成者**: BrowserSelector開発チーム
