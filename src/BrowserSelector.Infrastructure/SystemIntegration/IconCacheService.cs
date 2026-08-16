// <copyright file="IconCacheService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// 実行ファイル・画像ファイルからアイコンを取得しキャッシュするサービス.
/// メモリLRUキャッシュとディスクキャッシュの2階層構成.
/// </summary>
public sealed class IconCacheService : IIconCacheService
{
    private const int MemoryCacheCapacity = 256;

    private readonly ILogService? _logService;
    private readonly string _diskCacheDirectory;
    private readonly object _lruLock = new();
    private readonly Dictionary<string, LinkedListNode<CacheEntry>> _memoryCache = new(StringComparer.Ordinal);
    private readonly LinkedList<CacheEntry> _lruOrder = new();

    /// <summary>
    /// <see cref="IconCacheService"/> クラスの新しいインスタンスを初期化します.
    /// </summary>
    /// <param name="logService">ログサービス（省略可）.</param>
    public IconCacheService(ILogService? logService = null)
    {
        _logService = logService;
        _diskCacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BrowserSelector",
            "iconcache");
    }

    /// <inheritdoc/>
    public BitmapSource? GetIcon(string filePath, int iconIndex, int size)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!File.Exists(filePath))
        {
            return null;
        }

        DateTime lastWriteTimeUtc;
        try
        {
            lastWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        string cacheKey = $"{filePath}|{iconIndex}|{size}";

        BitmapSource? memoryHit = TryGetFromMemoryCache(cacheKey);
        if (memoryHit != null)
        {
            return memoryHit;
        }

        string diskCacheFileName = BuildDiskCacheFileName(filePath, iconIndex, size, lastWriteTimeUtc);
        BitmapImage? diskHit = TryLoadFromDisk(diskCacheFileName);
        if (diskHit != null)
        {
            AddToMemoryCache(cacheKey, diskHit);
            return diskHit;
        }

        BitmapSource? extracted = ExtractIcon(filePath, iconIndex, size);
        if (extracted == null)
        {
            return null;
        }

        AddToMemoryCache(cacheKey, extracted);
        TrySaveToDisk(diskCacheFileName, extracted);

        return extracted;
    }

    /// <inheritdoc/>
    public void ClearMemoryCache()
    {
        lock (_lruLock)
        {
            _memoryCache.Clear();
            _lruOrder.Clear();
        }
    }

    private static string BuildDiskCacheFileName(string filePath, int iconIndex, int size, DateTime lastWriteTimeUtc)
    {
        string source = $"{filePath}|{iconIndex}|{size}|{lastWriteTimeUtc.Ticks}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        StringBuilder sb = new(hash.Length * 2);
        foreach (byte b in hash)
        {
            _ = sb.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return sb.ToString() + ".png";
    }

    private static void DestroyIconIfValid(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            _ = NativeMethods.DestroyIcon(handle);
        }
    }

    private static BitmapSource ConvertIconToBitmap(System.Drawing.Icon icon, int size)
    {
        using System.Drawing.Icon sized = new(icon, size, size);
        BitmapSource bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
            sized.Handle,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        return (BitmapSource)bitmap.GetAsFrozen();
    }

    private BitmapSource? TryGetFromMemoryCache(string cacheKey)
    {
        lock (_lruLock)
        {
            if (_memoryCache.TryGetValue(cacheKey, out LinkedListNode<CacheEntry>? node))
            {
                _lruOrder.Remove(node);
                _lruOrder.AddFirst(node);
                return node.Value.Bitmap;
            }
        }

        return null;
    }

    private void AddToMemoryCache(string cacheKey, BitmapSource bitmap)
    {
        lock (_lruLock)
        {
            if (_memoryCache.TryGetValue(cacheKey, out LinkedListNode<CacheEntry>? existing))
            {
                _lruOrder.Remove(existing);
                _memoryCache.Remove(cacheKey);
            }

            LinkedListNode<CacheEntry> node = new(new CacheEntry(cacheKey, bitmap));
            _lruOrder.AddFirst(node);
            _memoryCache[cacheKey] = node;

            while (_memoryCache.Count > MemoryCacheCapacity)
            {
                LinkedListNode<CacheEntry>? last = _lruOrder.Last;
                if (last == null)
                {
                    break;
                }

                _lruOrder.RemoveLast();
                _memoryCache.Remove(last.Value.CacheKey);
            }
        }
    }

    private BitmapImage? TryLoadFromDisk(string diskCacheFileName)
    {
        string fullPath = Path.Combine(_diskCacheDirectory, diskCacheFileName);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            using MemoryStream stream = new(bytes);
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (IOException ex)
        {
            _logService?.LogWarning($"アイコンディスクキャッシュの読み込みに失敗しました: {fullPath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (NotSupportedException ex)
        {
            _logService?.LogWarning($"アイコンディスクキャッシュの読み込みに失敗しました: {fullPath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogWarning($"アイコンディスクキャッシュの読み込みに失敗しました: {fullPath}", nameof(IconCacheService), ex);
            return null;
        }
    }

    private void TrySaveToDisk(string diskCacheFileName, BitmapSource bitmap)
    {
        try
        {
            _ = Directory.CreateDirectory(_diskCacheDirectory);
            string fullPath = Path.Combine(_diskCacheDirectory, diskCacheFileName);

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using MemoryStream stream = new();
            encoder.Save(stream);
            File.WriteAllBytes(fullPath, stream.ToArray());
        }
        catch (IOException ex)
        {
            // 書き込み失敗は非致命（メモリキャッシュは既に利用可能なため機能への影響はない）
            _logService?.LogWarning("アイコンディスクキャッシュの書き込みに失敗しました", nameof(IconCacheService), ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogWarning("アイコンディスクキャッシュの書き込みに失敗しました", nameof(IconCacheService), ex);
        }
    }

    private BitmapSource? ExtractIcon(string filePath, int iconIndex, int size)
    {
        bool isImageFile = !filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            && !filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

        if (isImageFile)
        {
            return TryLoadImageFile(filePath, size);
        }

        return ExtractFromExecutable(filePath, iconIndex, size);
    }

    private BitmapImage? TryLoadImageFile(string filePath, int size)
    {
        try
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.DecodePixelWidth = size;
            bitmap.DecodePixelHeight = size;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (IOException ex)
        {
            _logService?.LogWarning($"画像ファイルの読み込みに失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (NotSupportedException ex)
        {
            _logService?.LogWarning($"画像ファイルの読み込みに失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogWarning($"画像ファイルの読み込みに失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
    }

    private BitmapSource? ExtractFromExecutable(string filePath, int iconIndex, int size)
    {
        IntPtr largeIcon = IntPtr.Zero;
        IntPtr smallIcon = IntPtr.Zero;
        IntPtr countDummyLarge = IntPtr.Zero;
        IntPtr countDummySmall = IntPtr.Zero;

        try
        {
            int iconCount = NativeMethods.ExtractIconEx(filePath, -1, out countDummyLarge, out countDummySmall, 0);

            System.Drawing.Icon? icon = null;
            if (iconCount > 0 && iconIndex < iconCount
                && NativeMethods.ExtractIconEx(filePath, iconIndex, out largeIcon, out smallIcon, 1) > 0
                && largeIcon != IntPtr.Zero)
            {
                icon = System.Drawing.Icon.FromHandle(largeIcon);
            }

            // フォールバック: 標準の関連付けアイコン取得
            icon ??= System.Drawing.Icon.ExtractAssociatedIcon(filePath);

            if (icon == null)
            {
                return null;
            }

            using (icon)
            {
                return ConvertIconToBitmap(icon, size);
            }
        }
        catch (IOException ex)
        {
            _logService?.LogWarning($"アイコン抽出に失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogWarning($"アイコン抽出に失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
        catch (ExternalException ex)
        {
            _logService?.LogWarning($"アイコン抽出に失敗しました: {filePath}", nameof(IconCacheService), ex);
            return null;
        }
        finally
        {
            // ExtractIconExが返す全ハンドルを必ず解放する（GDIハンドルリーク防止）
            DestroyIconIfValid(largeIcon);
            DestroyIconIfValid(smallIcon);
            DestroyIconIfValid(countDummyLarge);
            DestroyIconIfValid(countDummySmall);
        }
    }

    private static class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int ExtractIconEx(string szFileName, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool DestroyIcon(IntPtr hIcon);
    }

    private sealed class CacheEntry
    {
        public CacheEntry(string cacheKey, BitmapSource bitmap)
        {
            CacheKey = cacheKey;
            Bitmap = bitmap;
        }

        public string CacheKey { get; }

        public BitmapSource Bitmap { get; }
    }
}
