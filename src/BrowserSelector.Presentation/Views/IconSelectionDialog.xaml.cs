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
/// IconSelectionDialog.xaml の相互作用ロジック
/// </summary>
public partial class IconSelectionDialog : Window
{
    public string? SelectedIconPath { get; private set; }
    public int SelectedIconIndex { get; private set; } = -1;

    private readonly List<IconInfo> _icons = new();
    private string? _currentSelectedPath;
    private string? _executablePath;
    private readonly ILogService? _logService;

    public IconSelectionDialog(ILogService? logService = null)
    {
        _logService = logService;
        InitializeComponent();
        LoadSystemIcons();
        LoadRecentIcons();
    }

    /// <summary>
    /// 特定の実行ファイルからアイコンを抽出して表示
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス</param>
    public void LoadExecutableIcon(string executablePath)
    {
        try
        {
            _executablePath = executablePath;

            if (File.Exists(executablePath))
            {
                // 既存のアイコンをクリア
                ExecutableIconsPanel.Children.Clear();

                // 実行ファイルから複数のアイコンを抽出
                var icons = ExtractIconsFromExecutable(executablePath);

                foreach (var iconInfo in icons)
                {
                    AddIconButton(iconInfo, ExecutableIconsPanel);
                }

            }
        }
        catch (Exception)
        {
            // 実行ファイルアイコン読み込みエラーは無視
        }
    }

    /// <summary>
    /// 実行ファイルから複数のアイコンを抽出
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス</param>
    /// <returns>抽出されたアイコンのリスト</returns>
    private List<IconInfo> ExtractIconsFromExecutable(string executablePath)
    {
        var icons = new List<IconInfo>();

        try
        {
            // アイコン数を取得
            var iconCount = ExtractIconEx(executablePath, -1, (IntPtr[]?)null!, (IntPtr[]?)null!, 0);

            if (iconCount > 0)
            {
                var largeIcons = new IntPtr[iconCount];
                var smallIcons = new IntPtr[iconCount];

                // すべてのアイコンを抽出
                ExtractIconEx(executablePath, 0, largeIcons, smallIcons, iconCount);

                for (int i = 0; i < iconCount; i++)
                {
                    if (largeIcons[i] != IntPtr.Zero)
                    {
                        var icon = System.Drawing.Icon.FromHandle(largeIcons[i]);
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
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
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
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン抽出エラー: {executablePath}, {ex.Message}", "IconSelectionDialog", ex);

            // フォールバック: 関連付けられたアイコンを取得
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
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
            catch (Exception fallbackEx)
            {
                _logService?.LogError($"フォールバックアイコン抽出エラー: {fallbackEx.Message}", "IconSelectionDialog", fallbackEx);
            }
        }

        return icons;
    }

    private void LoadSystemIcons()
    {
        try
        {
            // システムフォルダから一般的なアイコンファイルを検索
            var systemFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            var iconExtensions = new[] { ".ico", ".exe", ".dll" };

            foreach (var folder in systemFolders)
            {
                if (Directory.Exists(folder))
                {
                    try
                    {
                        var files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(f => iconExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                            .Take(30); // 最大30個まで

                        foreach (var file in files)
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
                    catch (Exception ex)
                    {
                        // フォルダアクセスエラーは無視
                        _logService?.LogWarning($"フォルダアクセスエラー: {folder}, {ex.Message}", "IconSelectionDialog");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"システムアイコン読み込みエラー: {ex.Message}", "IconSelectionDialog", ex);
        }
    }

    private void LoadRecentIcons()
    {
        try
        {
            // 最近使用したアイコンのパスを設定ファイルから読み込み
            // ここではサンプルとして空のリストを使用
            var recentIcons = new List<string>();

            // 最近使用したアイコンがあれば表示
            foreach (var iconPath in recentIcons)
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
        catch (Exception ex)
        {
            _logService?.LogError($"最近使用アイコン読み込みエラー: {ex.Message}", "IconSelectionDialog", ex);
        }
    }

    private void AddIconButton(IconInfo iconInfo, WrapPanel panel)
    {
        try
        {
            var button = new Button
            {
                Width = 48,
                Height = 48,
                Margin = new Thickness(2),
                Tag = iconInfo,
                ToolTip = iconInfo.Name
            };

            // 高解像度アイコンをボタンに設定
            var image = new Image
            {
                Source = iconInfo.Icon != null ? ConvertIconToBitmapImage(iconInfo.Icon) : null,
                Stretch = Stretch.Uniform,
                Width = 32,
                Height = 32
            };

            button.Content = image;
            button.Click += IconButton_Click;
            panel.Children.Add(button);
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アイコンボタン作成エラー: {iconInfo.Path}, {ex.Message}", "IconSelectionDialog", ex);
        }
    }

    /// <summary>
    /// アイコンを高解像度BitmapImageに変換
    /// </summary>
    /// <param name="icon">変換するアイコン</param>
    /// <returns>高解像度BitmapImage</returns>
    private BitmapImage ConvertIconToBitmapImage(System.Drawing.Icon icon)
    {
        try
        {
            // リサイズせずに元のアイコンをそのまま使用
            var originalSize = icon.Size;
            _logService?.LogDebug($"アイコン元サイズ: {originalSize.Width}x{originalSize.Height}", "IconSelectionDialog");

            using var stream = new MemoryStream();
            icon.Save(stream);
            stream.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            // リサイズせずに元のサイズでデコード
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン変換エラー: {ex.Message}", "IconSelectionDialog", ex);
            return null!;
        }
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
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                return icon != null;
            }
        }
        catch
        {
            // エラーが発生した場合は無効なファイルとして扱う
        }
        return false;
    }

    private void IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is IconInfo iconInfo)
        {
            // 選択されたアイコンの情報を表示
            _currentSelectedPath = iconInfo.Path;
            SelectedIconIndex = iconInfo.Index;
            UpdateSelectedIconDisplay(iconInfo);
        }
    }

    private void UpdateSelectedIconDisplay(IconInfo iconInfo)
    {
        try
        {
            // アイコンプレビューを更新
            SelectedIconPreview.Source = iconInfo.Icon != null ? ConvertIconToBitmapImage(iconInfo.Icon) : null;

            // パス情報を更新
            SelectedIconPathText.Text = iconInfo.Name;

            // 詳細情報を更新
            var fileInfo = new FileInfo(iconInfo.Path);
            SelectedIconInfo.Text = $"パス: {iconInfo.Path}\nインデックス: {iconInfo.Index}\nサイズ: {fileInfo.Length:N0} bytes\n更新日: {fileInfo.LastWriteTime:yyyy/MM/dd}";

            // 確認ボタンを有効化
            ConfirmButton.IsEnabled = true;

            _logService?.LogInformation($"アイコンが選択されました: {iconInfo.Path}, インデックス: {iconInfo.Index}", "IconSelectionDialog");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アイコン表示更新エラー: {iconInfo.Path}, {ex.Message}", "IconSelectionDialog", ex);
        }
    }

    private void BrowseCustomIcon_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "アイコンファイルを選択",
            Filter = "アイコンファイル (*.ico;*.exe;*.dll)|*.ico;*.exe;*.dll|画像ファイル (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|すべてのファイル (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var iconPath = openFileDialog.FileName;
            if (IsValidIconFile(iconPath) || IsImageFile(iconPath))
            {
                // 選択されたアイコンを表示
                var iconInfo = new IconInfo
                {
                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath),
                    Index = 0,
                    Path = iconPath,
                    Name = Path.GetFileName(iconPath)
                };

                UpdateSelectedIconDisplay(iconInfo);
                _currentSelectedPath = iconPath;
            }
            else
            {
                LocalizedMessageBox.ShowError("Dialog.IconSelection.InvalidIconFile", "MessageBox.Error");
            }
        }
    }

    private bool IsImageFile(string filePath)
    {
        var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        return imageExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private void ConfirmSelection_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentSelectedPath))
        {
            SelectedIconPath = _currentSelectedPath;
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #region Win32 API
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);
    #endregion
}

/// <summary>
/// アイコン情報を表すクラス
/// </summary>
public class IconInfo
{
    public System.Drawing.Icon? Icon { get; set; }
    public int Index { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}