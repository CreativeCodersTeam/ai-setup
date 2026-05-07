using System.ComponentModel;
using System.Globalization;
using AiSetup.Models;

namespace AiSetup.Cli.Infrastructure;

/// <summary>
/// Converts CLI mode strings (<c>repo</c>/<c>local</c>) to <see cref="DeployMode"/>.
/// </summary>
internal sealed class DeployModeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string s)
        {
            return base.ConvertFrom(context, culture, value);
        }

        return s.Trim().ToLowerInvariant() switch
        {
            "repo" => DeployMode.Repo,
            "local" => DeployMode.Local,
            _ => throw new FormatException($"Unknown deploy mode '{s}'. Expected 'repo' or 'local'.")
        };
    }
}
