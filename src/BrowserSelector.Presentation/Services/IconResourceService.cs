using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Services;

/// <summary>
/// アイコンリソース管理サービス.
/// </summary>
public class IconResourceService
{
    private readonly string _imagesPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconResourceService"/> class.
    /// </summary>
    public IconResourceService()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourcePath = Path.GetDirectoryName(assembly.Location) ?? string.Empty;
        _imagesPath = Path.Combine(resourcePath, "Resources", "Images");
    }

    /// <summary>
    /// 不足しているアイコンファイルを作成.
    /// </summary>
    /// <param name="iconName">アイコン名（例: "Icon_Rules"）.</param>
    /// <returns>作成されたファイルのパス.</returns>
    public string CreateMissingIcon(string iconName)
    {
        try
        {
            string fileName = $"{iconName}.png";
            string filePath = Path.Combine(_imagesPath, fileName);

            // 既に存在する場合は何もしない
            if (File.Exists(filePath))
            {
                return filePath;
            }

            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(_imagesPath))
            {
                _ = Directory.CreateDirectory(_imagesPath);
            }

            // デフォルトアイコンを作成（32x32の透明PNG）
            BitmapSource defaultIcon = CreateDefaultIcon(32, 32);

            // PNGファイルとして保存
            using FileStream fileStream = new(filePath, FileMode.Create);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(defaultIcon));
            encoder.Save(fileStream);

            return filePath;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// アイコンファイルの存在確認.
    /// </summary>
    /// <param name="iconName">アイコン名.</param>
    /// <returns>存在する場合はtrue.</returns>
    public bool IconExists(string iconName)
    {
        string fileName = $"{iconName}.png";
        string filePath = Path.Combine(_imagesPath, fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// 不足しているアイコンを一括作成.
    /// </summary>
    /// <param name="iconNames">アイコン名の配列.</param>
    /// <returns>作成されたファイル数の配列.</returns>
    public int CreateMissingIcons(string[] iconNames)
    {
        int createdCount = 0;

        foreach (string iconName in iconNames)
        {
            if (!IconExists(iconName))
            {
                string createdPath = CreateMissingIcon(iconName);
                if (!string.IsNullOrEmpty(createdPath))
                {
                    createdCount++;
                }
            }
        }

        return createdCount;
    }

    /// <summary>
    /// デフォルトアイコンを作成（透明な32x32のPNG）.
    /// </summary>
    /// <param name="width">幅.</param>
    /// <param name="height">高さ.</param>
    /// <returns>作成されたBitmapSource.</returns>
    private BitmapSource CreateDefaultIcon(int width, int height)
    {
        // 透明な32x32のビットマップを作成
        System.Windows.Media.PixelFormat pixelFormat = System.Windows.Media.PixelFormats.Bgra32;
        int bytesPerPixel = pixelFormat.BitsPerPixel / 8;
        int stride = width * bytesPerPixel;
        byte[] pixels = new byte[height * stride];

        // 透明なピクセルで初期化
        for (int i = 0; i < pixels.Length; i += bytesPerPixel)
        {
            pixels[i] = 0;     // Blue
            pixels[i + 1] = 0; // Green
            pixels[i + 2] = 0; // Red
            pixels[i + 3] = 0; // Alpha (透明)
        }

        return BitmapSource.Create(width, height, 96, 96, pixelFormat, null, pixels, stride);
    }

    /// <summary>
    /// 不足しているアイコンの一覧を取得.
    /// </summary>
    /// <returns>不足しているアイコン名の配列.</returns>
    public string[] GetMissingIcons()
    {
        string[] requiredIcons = new[]
        {
            "Icon_Rules",
            "Icon_Cleanup",
            "Icon_View",
            "Icon_Clear",
            "Icon_Log"
        };

        List<string> missingIcons = [];

        foreach (string? iconName in requiredIcons)
        {
            if (!IconExists(iconName))
            {
                missingIcons.Add(iconName);
            }
        }

        return missingIcons.ToArray();
    }
}
