using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using System.Security;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// プロトコルハンドラーを管理するサービス.
/// </summary>
public class ProtocolHandler : IProtocolHandler
{
    private const string ProtocolName = "browserselector";
    private const string ProtocolDescription = "BrowserSelector Protocol";
    private const string RegistryKeyPath = @"SOFTWARE\Classes\" + ProtocolName;

    /// <summary>
    /// プロトコルを登録.
    /// </summary>
    /// <returns></returns>
    public bool RegisterProtocol(string applicationPath)
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null)
            {
                throw new InvalidOperationException("レジストリキーの作成に失敗しました");
            }

            // プロトコル名の設定
            key.SetValue(string.Empty, ProtocolDescription);
            key.SetValue("URL Protocol", string.Empty);

            // デフォルトアイコンの設定
            using RegistryKey defaultIconKey = key.CreateSubKey("DefaultIcon");
            defaultIconKey?.SetValue(string.Empty, $"{applicationPath},0");

            // シェルコマンドの設定
            using RegistryKey shellKey = key.CreateSubKey(@"shell\open\command");
            shellKey?.SetValue(string.Empty, $"\"{applicationPath}\" \"%1\"");

            return true;
        }
        catch (SecurityException ex)
        {
            throw new SecurityException("管理者権限が必要です", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"プロトコル登録に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// プロトコルを登録解除.
    /// </summary>
    /// <returns></returns>
    public bool UnregisterProtocol()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, false);
            return true;
        }
        catch (SecurityException ex)
        {
            throw new SecurityException("管理者権限が必要です", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"プロトコル登録解除に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// プロトコルが登録されているかチェック.
    /// </summary>
    /// <returns></returns>
    public bool IsProtocolRegistered()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            return key != null;
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration check failed (UnauthorizedAccessException): {ex.Message}");
            return false;
        }
        catch (System.Security.SecurityException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration check failed (SecurityException): {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration check failed (ArgumentException): {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// プロトコルURLからパラメータを抽出.
    /// </summary>
    /// <returns></returns>
    public string? ExtractUrlFromProtocol(string protocolUrl)
    {
        if (string.IsNullOrEmpty(protocolUrl))
        {
            return null;
        }

        // browserselector:// プレフィックスを除去
        return protocolUrl.StartsWith($"{ProtocolName}://", StringComparison.OrdinalIgnoreCase)
            ? protocolUrl[$"{ProtocolName}://".Length..]
            : null;
    }

    /// <summary>
    /// プロトコルURLを生成.
    /// </summary>
    /// <returns></returns>
    public string CreateProtocolUrl(string url)
    {
        return $"{ProtocolName}://{url}";
    }

    /// <summary>
    /// プロトコルURLからパラメータを抽出（Uri版）.
    /// </summary>
    /// <returns></returns>
    public Uri? ExtractUrlFromProtocol(Uri protocolUrl)
    {
        var result = ExtractUrlFromProtocol(protocolUrl?.ToString() ?? string.Empty);
        return result != null && Uri.TryCreate(result, UriKind.Absolute, out var uri) ? uri : null;
    }

    /// <summary>
    /// プロトコルURLを生成（Uri版）.
    /// </summary>
    /// <returns></returns>
    public Uri CreateProtocolUrl(Uri url)
    {
        return new Uri($"{ProtocolName}://{url}");
    }

    /// <summary>
    /// プロトコル登録情報を取得.
    /// </summary>
    /// <returns></returns>
    public ProtocolRegistrationInfo? GetProtocolRegistrationInfo()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null)
            {
                return null;
            }

            string description = key.GetValue(string.Empty) as string ?? string.Empty;
            string command = string.Empty;

            using RegistryKey? shellKey = key.OpenSubKey(@"shell\open\command");
            if (shellKey != null)
            {
                command = shellKey.GetValue(string.Empty) as string ?? string.Empty;
            }

            return new ProtocolRegistrationInfo
            {
                ProtocolName = ProtocolName,
                Description = description,
                Command = command,
                IsRegistered = true
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration info retrieval failed (UnauthorizedAccessException): {ex.Message}");
            return null;
        }
        catch (System.Security.SecurityException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration info retrieval failed (SecurityException): {ex.Message}");
            return null;
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Protocol registration info retrieval failed (ArgumentException): {ex.Message}");
            return null;
        }
    }
}
