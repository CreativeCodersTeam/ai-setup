using System.ComponentModel;
using System.Globalization;

namespace AiSetup.Cli.Infrastructure;

internal class KebabCaseEnumConverter<TEnum> : TypeConverter
    where TEnum : struct, Enum
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text)
        {
            return base.ConvertFrom(context, culture, value);
        }

        var compact = text.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal);

        foreach (var name in Enum.GetNames<TEnum>())
        {
            if (string.Equals(name, compact, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<TEnum>(name);
            }
        }

        throw new FormatException(
            $"'{text}' is not a valid {typeof(TEnum).Name}. Allowed values: {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }
}

internal sealed class DeployTargetConverter : KebabCaseEnumConverter<AiSetup.Lib.Models.DeployTarget>;

internal sealed class DeployModeConverter : KebabCaseEnumConverter<AiSetup.Lib.Models.DeployMode>;
