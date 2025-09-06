using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using System.Security;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// プロトコルハンドラーを管理するサービス
/// </summary>
public class ProtocolHandler : IProtocolHandler
{
    private const string ProtocolName = "browserselector";
    private const string ProtocolDescription = "BrowserSelector Protocol";
    private const string RegistryKeyPath = @"SOFTWARE\Classes\" + ProtocolName;

    /// <summary>
    /// プロトコルを登録
    /// </summary>
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
            key.SetValue("", ProtocolDescription);
            key.SetValue("URL Protocol", "");

            // デフォルトアイコンの設定
            using RegistryKey defaultIconKey = key.CreateSubKey("DefaultIcon");
            defaultIconKey?.SetValue("", $"{applicationPath},0");

            // シェルコマンドの設定
            using RegistryKey shellKey = key.CreateSubKey(@"shell\open\command");
            shellKey?.SetValue("", $"\"{applicationPath}\" \"%1\"");

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
    /// プロトコルを登録解除
    /// </summary>
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
    /// プロトコルが登録されているかチェック
    /// </summary>
    public bool IsProtocolRegistered()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// プロトコルURLからパラメータを抽出
    /// </summary>
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
    /// プロトコルURLを生成
    /// </summary>
    public string CreateProtocolUrl(string url)
    {
        return $"{ProtocolName}://{url}";
    }

    /// <summary>
    /// プロトコル登録情報を取得
    /// </summary>
    public ProtocolRegistrationInfo? GetProtocolRegistrationInfo()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null)
            {
                return null;
            }

            string description = key.GetValue("") as string ?? "";
            string command = "";

            using RegistryKey? shellKey = key.OpenSubKey(@"shell\open\command");
            if (shellKey != null)
            {
                command = shellKey.GetValue("") as string ?? "";
            }

            return new ProtocolRegistrationInfo
            {
                ProtocolName = ProtocolName,
                Description = description,
                Command = command,
                IsRegistered = true
            };
        }
        catch
        {
            return null;
        }
    }
}
