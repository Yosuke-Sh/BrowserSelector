using BrowserSelector.Core.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Controls;

/// <summary>
/// ブラウザアイコン表示用のカスタムコントロール.
/// </summary>
public class BrowserIconDisplay : Control
{
    /// <summary>
    /// BrowserProperty.
    /// </summary>
    public static readonly DependencyProperty BrowserProperty =
        DependencyProperty.Register(nameof(Browser), typeof(Browser), typeof(BrowserIconDisplay),
            new PropertyMetadata(null, OnBrowserChanged));

    /// <summary>
    /// IconScaleProperty.
    /// </summary>
    public static readonly DependencyProperty IconScaleProperty =
        DependencyProperty.Register(
            nameof(IconScale),
            typeof(double),
            typeof(BrowserIconDisplay),
            new PropertyMetadata(1.0, OnIconScaleChanged));

    /// <summary>
    /// ShowIconProperty.
    /// </summary>
    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(BrowserIconDisplay),
            new PropertyMetadata(true));

    /// <summary>
    /// IconSourceProperty.
    /// </summary>
    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(BrowserIconDisplay),
            new PropertyMetadata(null));

    static BrowserIconDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserIconDisplay),
            new FrameworkPropertyMetadata(typeof(BrowserIconDisplay)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserIconDisplay"/> class.
    /// </summary>
    public BrowserIconDisplay()
    {
        // InitializeComponent();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BrowserIconDisplay"/> class.
    /// </summary>
    ~BrowserIconDisplay()
    {
        // イベントハンドラーを解除
        if (Browser != null)
        {
            Browser.PropertyChanged -= OnBrowserPropertyChanged;
        }
    }

    /// <summary>
    /// Gets or sets ブラウザ情報.
    /// </summary>
    public Browser Browser
    {
        get => (Browser)GetValue(BrowserProperty);
        set => SetValue(BrowserProperty, value);
    }

    /// <summary>
    /// Gets or sets アイコンのスケール.
    /// </summary>
    public double IconScale
    {
        get => (double)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether アイコンを表示するかどうか.
    /// </summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>
    /// Gets or sets アイコンのソース.
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// アイコンをリフレッシュ.
    /// </summary>
    public void RefreshIcon()
    {
        LoadIcon();
    }

    /// <summary>
    /// カスタムアイコンファイルを設定.
    /// </summary>
    /// <param name="iconPath">iconPath.</param>
    public void SetCustomIcon(string iconPath)
    {
        if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath))
        {
            ImageSource? icon = LoadIconFromFile(iconPath);
            IconSource = icon ?? GetDefaultIcon();
        }
    }

    /// <summary>
    /// 指定されたインデックスのアイコンを抽出.
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス.</param>
    /// <param name="iconIndex">アイコンのインデックス.</param>
    /// <returns>抽出されたアイコン.</returns>
    private static System.Drawing.Icon? ExtractIconByIndex(string executablePath, int iconIndex)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ExtractIconByIndex開始: {executablePath}, IconIndex: {iconIndex}");
            // ExtractIconEx APIを使用してアイコンを抽出
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int extractedCount = ExtractIconEx(executablePath, iconIndex, largeIcons, smallIcons, 1);
            System.Diagnostics.Debug.WriteLine($"ExtractIconEx結果: extractedCount={extractedCount}, largeIcons[0]={largeIcons[0]}");

            if (extractedCount > 0 && largeIcons[0] != IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"アイコン抽出成功: {executablePath}, IconIndex: {iconIndex}");
                return System.Drawing.Icon.FromHandle(largeIcons[0]);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"アイコン抽出失敗: {executablePath}, IconIndex: {iconIndex}");
            }
        }
        catch (Exception ex)
        {
            // エラーが発生した場合はnullを返す
            System.Diagnostics.Debug.WriteLine($"ExtractIconByIndexエラー: {ex.Message}");
        }

        return null;
    }

    // Windows API宣言
    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

    private static void OnBrowserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrowserIconDisplay display)
        {
            // 前のBrowserオブジェクトのイベントハンドラーを解除
            if (e.OldValue is Browser oldBrowser)
            {
                oldBrowser.PropertyChanged -= display.OnBrowserPropertyChanged;
            }

            // 新しいBrowserオブジェクトのイベントハンドラーを設定
            if (e.NewValue is Browser newBrowser)
            {
                newBrowser.PropertyChanged += display.OnBrowserPropertyChanged;
            }

            display.LoadIcon();
        }
    }

    private static void OnIconScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrowserIconDisplay display)
        {
            display.UpdateIconScale();
        }
    }

    /// <summary>
    /// Browserオブジェクトのプロパティ変更を監視.
    /// </summary>
    private void OnBrowserPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // IconPath、IconIndex、またはExecutablePathが変更された場合はアイコンを再読み込み
        if (e.PropertyName == nameof(Browser.IconPath) || e.PropertyName == nameof(Browser.IconIndex) || e.PropertyName == nameof(Browser.ExecutablePath))
        {
            LoadIcon();
        }
    }

    /// <summary>
    /// アイコンを読み込み.
    /// </summary>
    private async void LoadIcon()
    {
        if (Browser == null)
        {
            System.Diagnostics.Debug.WriteLine("LoadIcon: Browserがnull");
            IconSource = GetDefaultIcon();
            return;
        }

        System.Diagnostics.Debug.WriteLine($"LoadIcon開始: {Browser.Name}, IconPath: {Browser.IconPath}, ExecutablePath: {Browser.ExecutablePath}, IconIndex: {Browser.IconIndex}");

        try
        {
            // アイコンファイルから読み込み
            if (!string.IsNullOrEmpty(Browser.IconPath) && System.IO.File.Exists(Browser.IconPath))
            {
                System.Diagnostics.Debug.WriteLine($"アイコンファイルから読み込み: {Browser.IconPath}, IconIndex: {Browser.IconIndex}");
                // IconIndexが0以外の場合は、指定されたインデックスのアイコンを抽出
                if (Browser.IconIndex != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IconIndex={Browser.IconIndex}でアイコン抽出を試行");
                    ImageSource? iconFromIndex = await LoadIconFromExecutableAsync(Browser.IconPath, Browser.IconIndex).ConfigureAwait(false);
                    if (iconFromIndex != null)
                    {
                        IconSource = iconFromIndex;
                    }
                    else
                    {
                        // IconIndexでの抽出に失敗した場合は、デフォルトのアイコンファイル読み込みを試行
                        System.Diagnostics.Debug.WriteLine($"IconIndexでの抽出に失敗、デフォルト読み込みを試行");
                        ImageSource? iconFromFile = LoadIconFromFile(Browser.IconPath);
                        IconSource = iconFromFile ?? GetDefaultIcon();
                    }
                }
                else
                {
                    // IconIndexが0の場合は、通常のアイコンファイル読み込み
                    ImageSource? iconFromFile = LoadIconFromFile(Browser.IconPath);
                    IconSource = iconFromFile ?? GetDefaultIcon();
                }
            }

            // 実行ファイルからアイコンを抽出
            else if (!string.IsNullOrEmpty(Browser.ExecutablePath) && System.IO.File.Exists(Browser.ExecutablePath))
            {
                System.Diagnostics.Debug.WriteLine($"実行ファイルからアイコン抽出: {Browser.ExecutablePath}, IconIndex: {Browser.IconIndex}");
                ImageSource? iconFromExe = await LoadIconFromExecutableAsync(Browser.ExecutablePath, Browser.IconIndex).ConfigureAwait(false);
                IconSource = iconFromExe ?? GetDefaultIcon();
            }

            // デフォルトアイコンを使用
            else
            {
                System.Diagnostics.Debug.WriteLine("デフォルトアイコンを使用");
                IconSource = GetDefaultIcon();
            }

            UpdateIconScale();
        }
        catch (Exception)
        {
            // アイコン読み込みエラーは通常の操作なので、ログレベルを下げる
            // System.Diagnostics.Debug.WriteLine($"アイコン読み込みエラー: {ex.Message}");
            IconSource = GetDefaultIcon();
        }
    }

    /// <summary>
    /// ファイルからアイコンを読み込み.
    /// </summary>
    private ImageSource? LoadIconFromFile(string filePath)
    {
        try
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return GetDefaultIcon();
        }
    }

    /// <summary>
    /// 実行ファイルからアイコンを抽出.
    /// </summary>
    private async Task<ImageSource?> LoadIconFromExecutableAsync(string executablePath, int iconIndex = 0)
    {
        return await Task.Run(() =>
        {
            try
            {
                // デバッグログ: IconIndexの値を確認
                System.Diagnostics.Debug.WriteLine($"LoadIconFromExecutableAsync: {executablePath}, IconIndex: {iconIndex}");

                // Windows APIを使用してアイコンを抽出
                System.Drawing.Icon? icon = null;

                if (iconIndex == 0)
                {
                    // デフォルトアイコン（インデックス0）の場合
                    System.Diagnostics.Debug.WriteLine($"ExtractAssociatedIconを使用: {executablePath}");
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                }
                else
                {
                    // 特定のインデックスのアイコンを取得
                    System.Diagnostics.Debug.WriteLine($"ExtractIconByIndexを使用: {executablePath}, IconIndex: {iconIndex}");
                    icon = ExtractIconByIndex(executablePath, iconIndex);
                    System.Diagnostics.Debug.WriteLine($"ExtractIconByIndex結果: {icon != null}");
                }

                if (icon != null)
                {
                    return ConvertIconToHighQualityBitmapImage(icon);
                }
            }
            catch (Exception)
            {
                // エラーが発生した場合はデフォルトアイコンを使用
            }

            return GetDefaultIcon();
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// アイコンを高品質BitmapImageに変換.
    /// </summary>
    /// <param name="icon">変換するアイコン.</param>
    /// <returns>高品質BitmapImage.</returns>
    private BitmapImage ConvertIconToHighQualityBitmapImage(System.Drawing.Icon icon)
    {
        try
        {
            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Position = 0;

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;

            // DPI設定を明示的に指定
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            // 高品質スケーリングを有効化
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(bitmap, EdgeMode.Aliased);

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception)
        {
            return null!;
        }
    }

    /// <summary>
    /// デフォルトアイコンを取得.
    /// </summary>
    private ImageSource? GetDefaultIcon()
    {
        try
        {
            // デフォルトのブラウザアイコンを返す
            // 実際の実装では、アプリケーションリソースからアイコンを読み込み
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri("pack://application:,,,/BrowserSelector.Presentation;component/Resources/Images/default-browser.png");
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            // リソースが見つからない場合はnullを返す
            return null;
        }
    }

    /// <summary>
    /// アイコンのスケールを更新.
    /// </summary>
    private void UpdateIconScale()
    {
        if (IconSource != null)
        {
            // スケールを適用
            // 実際の実装では、RenderTransformを使用してスケールを適用
        }
    }

}
