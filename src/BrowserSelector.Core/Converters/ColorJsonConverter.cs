// <copyright file="ColorJsonConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace BrowserSelector.Core.Converters;

/// <summary>
/// System.Windows.Media.Color型のJSONシリアライゼーション用コンバーター.
/// </summary>
public class ColorJsonConverter : JsonConverter<Color>
{
    /// <inheritdoc/>
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // オブジェクト形式の場合
            byte a = 255, r = 0, g = 0, b = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName?.ToUpperInvariant())
                    {
                        case "A":
                            a = reader.GetByte();
                            break;
                        case "R":
                            r = reader.GetByte();
                            break;
                        case "G":
                            g = reader.GetByte();
                            break;
                        case "B":
                            b = reader.GetByte();
                            break;
                    }
                }
            }

            return Color.FromArgb(a, r, g, b);
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            // 文字列形式の場合（#AARRGGBB形式）
            string? colorString = reader.GetString();
            if (string.IsNullOrEmpty(colorString))
            {
                return Colors.Transparent;
            }

            try
            {
                return (Color)ColorConverter.ConvertFromString(colorString);
            }
            catch (FormatException)
            {
                return Colors.Transparent;
            }
            catch (ArgumentException)
            {
                return Colors.Transparent;
            }
        }

        return Colors.Transparent;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartObject();
        writer.WriteNumber("A", value.A);
        writer.WriteNumber("R", value.R);
        writer.WriteNumber("G", value.G);
        writer.WriteNumber("B", value.B);
        writer.WriteNumber("ScA", value.ScA);
        writer.WriteNumber("ScR", value.ScR);
        writer.WriteNumber("ScG", value.ScG);
        writer.WriteNumber("ScB", value.ScB);
        writer.WriteEndObject();
    }
}