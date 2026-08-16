using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// IconSelectionDialog.xaml の相互作用ロジック.
/// </summary>
public partial class IconSelectionDialog : Window
{
    private readonly ILogService? _logService;
    private string? _currentSelectedPath;
    private int _currentSelectedIconIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconSelectionDialog"/> class.
    /// </summary>
    /// <param name="logService">logService.</param>
    public IconSelectionDialog(ILogService? logService = null)
    {
        _logService = logService;
        InitializeComponent();
        LoadSystemIcons();
        LoadRecentIcons();
    }

    /// <summary>
    /// Gets selectedIconPath.
    /// </summary>
    public string? SelectedIconPath { get; private set; }

    /// <summary>
    /// Gets selectedIconIndex.
    /// </summary>
    public int SelectedIconIndex { get; private set; } = -1;

    /// <summary>
    /// 特定の実行ファイルからアイコンを抽出して表示.
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス.</param>
    public void LoadExecutableIcon(string executablePath)
    {
        try
        {

            if (File.Exists(executablePath))
            {
                // 既存のアイコンをクリア
                ExecutableIconsPanel.Children.Clear();

                // 実行ファイルから複数のアイコンを抽出
                List<IconInfo> icons = ExtractIconsFromExecutable(executablePath);

                foreach (IconInfo iconInfo in icons)
                {
                    AddIconButton(iconInfo, ExecutableIconsPanel);
                }

            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception)
        {
            // 実行ファイルアイコン読み込みエラーは無視
        }
        #pragma warning restore CA1031
    }

    #region Win32 API
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);
    #endregion

    /// <summary>
    /// 実行ファイルから複数のアイコンを抽出.
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス.</param>
    /// <returns>抽出されたアイコンのリスト.</returns>
    private List<IconInfo> ExtractIconsFromExecutable(string executablePath)
    {
        List<IconInfo> icons = new();

        try
        {
            // アイコン数を取得
            int iconCount = ExtractIconEx(executablePath, -1, null!, null!, 0);

            if (iconCount > 0)
            {
                nint[] largeIcons = new IntPtr[iconCount];
                nint[] smallIcons = new IntPtr[iconCount];

                // すべてのアイコンを抽出
                _ = ExtractIconEx(executablePath, 0, largeIcons, smallIcons, iconCount);

                for (int i = 0; i < iconCount; i++)
                {
                    if (largeIcons[i] != IntPtr.Zero)
                    {
                        System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(largeIcons[i]);
                        icons.Add(new IconInfo
                        {
                            Icon = icon,
                            Index = i,
                            Path = executablePath,
                            Name = $"Icon {i + 1}"
                        });
                    }
                }
            }
            else
            {
                // フォールバック: 関連付けられたアイコンを取得
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                if (icon != null)
                {
                    icons.Add(new IconInfo
                    {
                        Icon = icon,
                        Index = 0,
                        Path = executablePath,
                        Name = "Associated Icon"
                    });
                }
            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン抽出エラー: {executablePath}, {ex.Message}", "IconSelectionDialog", ex);

            // フォールバック: 関連付けられたアイコンを取得
            try
            {
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                if (icon != null)
                {
                    icons.Add(new IconInfo
                    {
                        Icon = icon,
                        Index = 0,
                        Path = executablePath,
                        Name = "Fallback Icon"
                    });
                }
            }
            // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
            #pragma warning disable CA1031
            catch (Exception fallbackEx)
            {
                _logService?.LogError($"フォールバックアイコン抽出エラー: {fallbackEx.Message}", "IconSelectionDialog", fallbackEx);
            }
            #pragma warning restore CA1031
        }
        #pragma warning restore CA1031

        return icons;
    }

    private void LoadSystemIcons()
    {
        try
        {
            // システムフォルダから一般的なアイコンファイルを検索
            string[] systemFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            string[] iconExtensions = new[] { ".ico", ".exe", ".dll" };

            foreach (string? folder in systemFolders)
            {
                if (Directory.Exists(folder))
                {
                    try
                    {
                        IEnumerable<string> files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(f => iconExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                            .Take(30); // 最大30個まで

                        foreach (string? file in files)
                        {
                            if (IsValidIconFile(file))
                            {
                                AddIconButton(new IconInfo
                                {
                                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(file),
                                    Index = 0,
                                    Path = file,
                                    Name = Path.GetFileName(file)
                                }, SystemIconsPanel);
                            }
                        }
                    }
                    // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
                    #pragma warning disable CA1031
                    catch (Exception ex)
                    {
                        // フォルダアクセスエラーは無視
                        _logService?.LogWarning($"フォルダアクセスエラー: {folder}, {ex.Message}", "IconSelectionDialog");
                    }
                    #pragma warning restore CA1031
                }
            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"システムアイコン読み込みエラー: {ex.Message}", "IconSelectionDialog", ex);
        }
        #pragma warning restore CA1031
    }

    private List<string> GetRecentIcons()
    {
        // 実際の実装では設定ファイルから読み込む
        // ここではサンプルとして空のリストを返す
        return new List<string>();
    }

    private void LoadRecentIcons()
    {
        try
        {
            // 最近使用したアイコンのパスを設定ファイルから読み込み
            var recentIcons = GetRecentIcons();

            // 最近使用したアイコンがあれば表示
            if (recentIcons.Count > 0)
            {
                foreach (string iconPath in recentIcons)
                {
                    if (File.Exists(iconPath))
                    {
                        AddIconButton(new IconInfo
                        {
                            Icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath),
                            Index = 0,
                            Path = iconPath,
                            Name = Path.GetFileName(iconPath)
                        }, RecentIconsPanel);
                    }
                }
            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"最近使用アイコン読み込みエラー: {ex.Message}", "IconSelectionDialog", ex);
        }
        #pragma warning restore CA1031
    }

    private void AddIconButton(IconInfo iconInfo, WrapPanel panel)
    {
        try
        {
            Button button = new()
            {
                Width = 48,
                Height = 48,
                Margin = new Thickness(2),
                Tag = iconInfo,
                ToolTip = iconInfo.Name
            };

            // 高解像度アイコンをボタンに設定
            Image image = new()
            {
                Source = iconInfo.Icon != null ? ConvertIconToBitmapImage(iconInfo.Icon) : null,
                Stretch = Stretch.Uniform,
                Width = 32,
                Height = 32
            };

            button.Content = image;
            button.Click += IconButton_Click;
            _ = panel.Children.Add(button);
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アイコンボタン作成エラー: {iconInfo.Path}, {ex.Message}", "IconSelectionDialog", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// アイコンを高解像度BitmapImageに変換.
    /// </summary>
    /// <param name="icon">変換するアイコン.</param>
    /// <returns>高解像度BitmapImage.</returns>
    private BitmapImage ConvertIconToBitmapImage(System.Drawing.Icon icon)
    {
        try
        {
            // リサイズせずに元のアイコンをそのまま使用
            System.Drawing.Size originalSize = icon.Size;
            _logService?.LogTrace($"アイコン元サイズ: {originalSize.Width}x{originalSize.Height}", "IconSelectionDialog");

            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Position = 0;

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.DecodePixelWidth = 32; // プレビュー用のサイズを指定
            bitmap.DecodePixelHeight = 32;

            // DPI設定を明示的に指定
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            // 高品質スケーリングを有効化
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(bitmap, EdgeMode.Aliased);

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン変換エラー: {ex.Message}", "IconSelectionDialog", ex);
            return null!;
        }
        #pragma warning restore CA1031
    }

    private bool IsValidIconFile(string filePath)
    {
        try
        {
            if (filePath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                     filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                // 実行ファイルにアイコンが含まれているかチェック
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                return icon != null;
            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時は無効なファイルとして扱うための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception)
        {
            // エラーが発生した場合は無効なファイルとして扱う
        }
#pragma warning restore CA1031
        return false;
    }

    private void IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is IconInfo iconInfo)
        {
            // 選択されたアイコンの情報を表示
            _currentSelectedPath = iconInfo.Path;
            _currentSelectedIconIndex = iconInfo.Index;
            SelectedIconIndex = iconInfo.Index;
            UpdateSelectedIconDisplay(iconInfo);
        }
    }

    private void UpdateSelectedIconDisplay(IconInfo iconInfo)
    {
        try
        {
            // アイコンプレビューを更新
            if (iconInfo.Icon != null)
            {
                SelectedIconPreview.Source = ConvertIconToBitmapImage(iconInfo.Icon);
            }
            else
            {
                // アイコンがnullの場合は、実行ファイルから直接読み込み
                if (System.IO.File.Exists(iconInfo.Path))
                {
                    System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(iconInfo.Path);
                    if (icon != null)
                    {
                        SelectedIconPreview.Source = ConvertIconToBitmapImage(icon);
                    }
                }
            }

            // パス情報を更新
            SelectedIconPathText.Text = iconInfo.Name;

            // 詳細情報を更新
            FileInfo fileInfo = new(iconInfo.Path);
            SelectedIconInfo.Text = $"パス: {iconInfo.Path}\nインデックス: {iconInfo.Index}\nサイズ: {fileInfo.Length:N0} bytes\n更新日: {fileInfo.LastWriteTime:yyyy/MM/dd}";

            _logService?.LogInformation($"アイコンが選択されました: {iconInfo.Path}, インデックス: {iconInfo.Index}", "IconSelectionDialog");
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はアイコン表示を諦めてUIを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン表示更新エラー: {iconInfo.Path}, {ex.Message}", "IconSelectionDialog", ex);
        }
        #pragma warning restore CA1031
    }

    private void BrowseCustomIcon_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "アイコンファイルを選択",
            Filter = "アイコンファイル (*.ico;*.exe;*.dll)|*.ico;*.exe;*.dll|画像ファイル (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|すべてのファイル (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string iconPath = openFileDialog.FileName;
            if (IsValidIconFile(iconPath) || IsImageFile(iconPath))
            {
                // 選択されたアイコンを表示
                IconInfo iconInfo = new()
                {
                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath),
                    Index = 0,
                    Path = iconPath,
                    Name = Path.GetFileName(iconPath)
                };

                UpdateSelectedIconDisplay(iconInfo);
                _currentSelectedPath = iconPath;
                _currentSelectedIconIndex = 0; // カスタムアイコンの場合はインデックス0
            }
            else
            {
                _ = LocalizedMessageBox.ShowError("Dialog.IconSelection.InvalidIconFile", "MessageBox.Error");
            }
        }
    }

    private bool IsImageFile(string filePath)
    {
        string[] imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        return imageExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentSelectedPath))
        {
            SelectedIconPath = _currentSelectedPath;
            // IconIndexも設定（現在選択されているアイコンのインデックス）
            SelectedIconIndex = _currentSelectedIconIndex;
        }
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// アイコン情報を表すクラス.
/// </summary>
public class IconInfo
{
    /// <summary>
    /// Gets or sets icon.
    /// </summary>
    public System.Drawing.Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
