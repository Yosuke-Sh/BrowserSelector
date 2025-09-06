using BrowserSelector.Core.Models;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Controls;

/// <summary>
/// ブラウザアイコン表示用のカスタムコントロール
/// </summary>
public class BrowserIconDisplay : Control
{
    public static readonly DependencyProperty BrowserProperty =
        DependencyProperty.Register(nameof(Browser), typeof(Browser), typeof(BrowserIconDisplay),
            new PropertyMetadata(null, OnBrowserChanged));

    public static readonly DependencyProperty IconScaleProperty =
        DependencyProperty.Register(nameof(IconScale), typeof(double), typeof(BrowserIconDisplay),
            new PropertyMetadata(1.0, OnIconScaleChanged));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(BrowserIconDisplay),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(BrowserIconDisplay),
            new PropertyMetadata(null));

    /// <summary>
    /// ブラウザ情報
    /// </summary>
    public Browser Browser
    {
        get => (Browser)GetValue(BrowserProperty);
        set => SetValue(BrowserProperty, value);
    }

    /// <summary>
    /// アイコンのスケール
    /// </summary>
    public double IconScale
    {
        get => (double)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }

    /// <summary>
    /// アイコンを表示するかどうか
    /// </summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>
    /// アイコンのソース
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    static BrowserIconDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserIconDisplay),
            new FrameworkPropertyMetadata(typeof(BrowserIconDisplay)));
    }

    public BrowserIconDisplay()
    {
        // アクセシビリティプロパティを設定
        SetValue(AutomationProperties.NameProperty, "BrowserIconDisplay");
        SetValue(AutomationProperties.HelpTextProperty, "ブラウザアイコン表示");
    }

    private static void OnBrowserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrowserIconDisplay display)
        {
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
    /// アイコンを読み込み
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
                var iconFromFile = LoadIconFromFile(Browser.IconPath);
                IconSource = iconFromFile ?? GetDefaultIcon();
            }
            // 実行ファイルからアイコンを抽出
            else if (!string.IsNullOrEmpty(Browser.ExecutablePath) && System.IO.File.Exists(Browser.ExecutablePath))
            {
                var iconFromExe = await LoadIconFromExecutableAsync(Browser.ExecutablePath);
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
    /// ファイルからアイコンを読み込み
    /// </summary>
    private ImageSource? LoadIconFromFile(string filePath)
    {
        try
        {
            var bitmap = new BitmapImage();
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
    /// 実行ファイルからアイコンを抽出
    /// </summary>
    private async Task<ImageSource?> LoadIconFromExecutableAsync(string executablePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Windows APIを使用してアイコンを抽出
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                if (icon != null)
                {
                    using var stream = new System.IO.MemoryStream();
                    icon.Save(stream);
                    stream.Position = 0;

                    var bitmap = new BitmapImage();
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
        });
    }

    /// <summary>
    /// デフォルトアイコンを取得
    /// </summary>
    private ImageSource? GetDefaultIcon()
    {
        try
        {
            // デフォルトのブラウザアイコンを返す
            // 実際の実装では、アプリケーションリソースからアイコンを読み込み
            var bitmap = new BitmapImage();
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
    /// アイコンのスケールを更新
    /// </summary>
    private void UpdateIconScale()
    {
        if (IconSource != null)
        {
            // スケールを適用
            // 実際の実装では、RenderTransformを使用してスケールを適用
        }
    }

    /// <summary>
    /// アイコンをリフレッシュ
    /// </summary>
    public void RefreshIcon()
    {
        LoadIcon();
    }

    /// <summary>
    /// カスタムアイコンファイルを設定
    /// </summary>
    public void SetCustomIcon(string iconPath)
    {
        if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath))
        {
            var icon = LoadIconFromFile(iconPath);
            IconSource = icon ?? GetDefaultIcon();
        }
    }
}
