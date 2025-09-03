using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// IconSelectionDialog.xaml の相互作用ロジック
/// </summary>
public partial class IconSelectionDialog : Window
{
    public string? SelectedIconPath { get; private set; }
    private readonly List<string> _recentIcons = new();
    private readonly List<string> _systemIcons = new();
    private string? _currentSelectedPath;

    public IconSelectionDialog()
    {
        InitializeComponent();
        LoadSystemIcons();
        LoadRecentIcons();
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
                                _systemIcons.Add(file);
                                AddIconButton(file, SystemIconsPanel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // フォルダアクセスエラーは無視
                        System.Diagnostics.Debug.WriteLine($"フォルダアクセスエラー: {folder}, {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"システムアイコン読み込みエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 特定の実行ファイルからアイコンを抽出して表示
    /// </summary>
    /// <param name="executablePath">実行ファイルのパス</param>
    public void LoadExecutableIcon(string executablePath)
    {
        try
        {
            if (File.Exists(executablePath) && IsValidIconFile(executablePath))
            {
                // 既存のアイコンをクリア
                ExecutableIconsPanel.Children.Clear();
                
                // 実行ファイルのアイコンを追加
                AddIconButton(executablePath, ExecutableIconsPanel);
                
                // 実行ファイル内の複数アイコンを検索
                LoadMultipleIconsFromExecutable(executablePath);
                
                System.Diagnostics.Debug.WriteLine($"実行ファイルからアイコンを読み込み: {executablePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"実行ファイルアイコン読み込みエラー: {executablePath}, {ex.Message}");
        }
    }

    private void LoadMultipleIconsFromExecutable(string executablePath)
    {
        try
        {
            // 実行ファイル内の複数アイコンを抽出
            // この実装では基本的なアイコン抽出のみ行います
            // より高度な実装では、Win32 APIを使用してすべてのアイコンリソースを抽出できます
            
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
            if (icon != null)
            {
                using (icon)
                {
                    // メインアイコンのみを表示（複数アイコン対応は将来の拡張として）
                    System.Diagnostics.Debug.WriteLine($"実行ファイルからメインアイコンを抽出: {executablePath}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"複数アイコン抽出エラー: {executablePath}, {ex.Message}");
        }
    }

    private void LoadRecentIcons()
    {
        try
        {
            // 最近使用したアイコンのパスを設定ファイルから読み込み
            // ここではサンプルとして空のリストを使用
            _recentIcons.Clear();
            
            // 最近使用したアイコンがあれば表示
            foreach (var iconPath in _recentIcons)
            {
                if (File.Exists(iconPath))
                {
                    AddIconButton(iconPath, RecentIconsPanel);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"最近使用アイコン読み込みエラー: {ex.Message}");
        }
    }

    private void AddIconButton(string iconPath, WrapPanel panel)
    {
        try
        {
            var button = new Button
            {
                Width = 48,
                Height = 48,
                Margin = new Thickness(2),
                Tag = iconPath,
                ToolTip = Path.GetFileName(iconPath)
            };

            // アイコンをボタンに設定
            var image = new Image
            {
                Source = LoadIconImage(iconPath),
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
            System.Diagnostics.Debug.WriteLine($"アイコンボタン作成エラー: {iconPath}, {ex.Message}");
        }
    }

    private ImageSource? LoadIconImage(string iconPath)
    {
        try
        {
            if (iconPath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                return new BitmapImage(new Uri(iconPath));
            }
            else if (iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || 
                     iconPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                // 実行ファイルからアイコンを抽出
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath);
                if (icon != null)
                {
                    using (icon)
                    {
                        var bitmap = icon.ToBitmap();
                        var hBitmap = bitmap.GetHbitmap();
                        try
                        {
                            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                        finally
                        {
                            DeleteObject(hBitmap);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"アイコン読み込みエラー: {iconPath}, {ex.Message}");
        }
        return null;
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
        if (sender is Button button && button.Tag is string iconPath)
        {
            // 選択されたアイコンの情報を表示
            _currentSelectedPath = iconPath;
            UpdateSelectedIconDisplay(iconPath);
        }
    }

    private void UpdateSelectedIconDisplay(string iconPath)
    {
        try
        {
            // アイコンプレビューを更新
            SelectedIconPreview.Source = LoadIconImage(iconPath);
            
            // パス情報を更新
            SelectedIconPathText.Text = Path.GetFileName(iconPath);
            
            // 詳細情報を更新
            var fileInfo = new FileInfo(iconPath);
            SelectedIconInfo.Text = $"パス: {iconPath}\nサイズ: {fileInfo.Length:N0} bytes\n更新日: {fileInfo.LastWriteTime:yyyy/MM/dd}";
            
            // 確認ボタンを有効化
            ConfirmButton.IsEnabled = true;
            
            System.Diagnostics.Debug.WriteLine($"アイコンが選択されました: {iconPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"アイコン表示更新エラー: {iconPath}, {ex.Message}");
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
                // 最近使用したアイコンに追加
                if (!_recentIcons.Contains(iconPath))
                {
                    _recentIcons.Insert(0, iconPath);
                    if (_recentIcons.Count > 20) // 最大20個まで
                    {
                        _recentIcons.RemoveAt(_recentIcons.Count - 1);
                    }
                }
                
                // 選択されたアイコンを表示
                UpdateSelectedIconDisplay(iconPath);
                _currentSelectedPath = iconPath;
            }
            else
            {
                MessageBox.Show("選択されたファイルは有効なアイコンファイルではありません。", "エラー", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
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

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}
