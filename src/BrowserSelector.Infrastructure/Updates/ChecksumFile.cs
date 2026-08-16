// <copyright file="ChecksumFile.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// SHA256SUMS.txt（sha256sum形式）を解析する（Phase H-2）.
/// </summary>
/// <remarks>
/// release.ymlが生成する形式は "&lt;64桁hex&gt;&lt;スペース2つ&gt;&lt;ファイル名&gt;"。
/// 手作業やGNU coreutilsのバイナリモード（"*"接頭辞）でも読めるよう、区切りは空白1つ以上を許容する.
/// </remarks>
internal static class ChecksumFile
{
    private const int Sha256HexLength = 64;

    /// <summary>
    /// チェックサムファイルの内容を「ファイル名 → ハッシュ値」の辞書へ解析する.
    /// </summary>
    /// <param name="content">SHA256SUMS.txtの内容.</param>
    /// <returns>ファイル名をキーとする辞書（大文字小文字を区別しない）。解析できない行は無視する.</returns>
    public static IReadOnlyDictionary<string, string> Parse(string? content)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(content))
        {
            return result;
        }

        // CRLF / LF / CR のいずれの改行でも解析できるようにする。
        string[] lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            // "<hash><空白+><ファイル名>" に分割する。ファイル名に空白が含まれる可能性があるため
            // 分割数は2に制限する。
            string[] parts = line.Split(default(char[]), 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            string hash = parts[0];
            if (!IsSha256Hex(hash))
            {
                continue;
            }

            string fileName = parts[1].Trim();

            // GNU coreutilsのバイナリモード表記 "*filename" の "*" を除去する。
            if (fileName.StartsWith('*'))
            {
                fileName = fileName[1..];
            }

            if (fileName.Length == 0)
            {
                continue;
            }

            // 同一ファイル名が複数回現れた場合は最初の行を採用する。
            // ハッシュは大文字へ正規化して保持する（CA1308: セキュリティ判断に使う文字列の
            // 小文字化はTurkish-I問題等を避けるため非推奨。照合側も同じ正規化を行う）。
            _ = result.TryAdd(fileName, hash.ToUpperInvariant());
        }

        return result;
    }

    private static bool IsSha256Hex(string value)
    {
        if (value.Length != Sha256HexLength)
        {
            return false;
        }

        foreach (char c in value)
        {
            bool isHex = (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'f')
                || (c >= 'A' && c <= 'F');

            if (!isHex)
            {
                return false;
            }
        }

        return true;
    }
}
