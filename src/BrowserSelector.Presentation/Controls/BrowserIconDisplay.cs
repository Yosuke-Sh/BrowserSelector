using BrowserSelector.Core.Models;
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
    /// デストラクタ.
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
            IconSource = GetDefaultIcon();
            return;
        }

        try
        {
            // アイコンファイルから読み込み
            if (!string.IsNullOrEmpty(Browser.IconPath) && System.IO.File.Exists(Browser.IconPath))
            {
                ImageSource? iconFromFile = LoadIconFromFile(Browser.IconPath);
                IconSource = iconFromFile ?? GetDefaultIcon();
            }

            // 実行ファイルからアイコンを抽出
            else if (!string.IsNullOrEmpty(Browser.ExecutablePath) && System.IO.File.Exists(Browser.ExecutablePath))
            {
                ImageSource? iconFromExe = await LoadIconFromExecutableAsync(Browser.ExecutablePath, Browser.IconIndex).ConfigureAwait(false);
                IconSource = iconFromExe ?? GetDefaultIcon();
            }

            // デフォルトアイコンを使用
            else
            {
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
            bitmap.EndInit();
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
                // Windows APIを使用してアイコンを抽出（iconIndexは現在のAPIでは使用できないため、デフォルトアイコンを取得）
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                if (icon != null)
                {
                    using System.IO.MemoryStream stream = new();
                    icon.Save(stream);
                    stream.Position = 0;

                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // UIスレッドでの使用を可能にする

                    return bitmap;
                }
            }
            catch
            {
                // エラーが発生した場合はデフォルトアイコンを使用
            }

            return GetDefaultIcon();
        }).ConfigureAwait(false);
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
            bitmap.EndInit();
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
