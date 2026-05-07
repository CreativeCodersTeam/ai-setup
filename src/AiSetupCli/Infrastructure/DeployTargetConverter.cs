using System.ComponentModel;
using System.Globalization;
using AiSetup.Models;

namespace AiSetup.Cli.Infrastructure;

/// <summary>
/// Converts user-friendly target names (e.g. <c>claude-code</c>) to <see cref="DeployTarget"/>.
/// </summary>
internal sealed class DeployTargetConverter : TypeConverter
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
            "copilot-cli" or "copilotcli" or "copilot" => DeployTarget.CopilotCli,
            "claude-code" or "claudecode" or "claude" => DeployTarget.ClaudeCode,
            _ => throw new FormatException($"Unknown deploy target '{s}'. Expected 'copilot-cli' or 'claude-code'.")
        };
    }
}
