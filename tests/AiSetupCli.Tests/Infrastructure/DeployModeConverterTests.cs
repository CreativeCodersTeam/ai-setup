using System.Globalization;
using AiSetup.Cli.Infrastructure;
using AiSetup.Models;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class DeployModeConverterTests
{
    [Theory]
    [InlineData("repo", DeployMode.Repo)]
    [InlineData("local", DeployMode.Local)]
    [InlineData("REPO", DeployMode.Repo)]
    [InlineData("Local", DeployMode.Local)]
    [InlineData("  repo  ", DeployMode.Repo)]
    public void ConvertFrom_KnownTokens_ReturnsExpectedMode(string raw, DeployMode expected)
    {
        var sut = new DeployModeConverter();

        var result = sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("re po")]
    public void ConvertFrom_UnknownString_Throws(string raw)
    {
        var sut = new DeployModeConverter();

        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        act.Should().Throw<FormatException>().WithMessage("*Unknown deploy mode*");
    }

    [Fact]
    public void ConvertFrom_NonStringValue_DelegatesToBase()
    {
        var sut = new DeployModeConverter();

        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, 42);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var sut = new DeployModeConverter();

        sut.CanConvertFrom(null, typeof(string)).Should().BeTrue();
    }

    [Fact]
    public void CanConvertFrom_Int_ReturnsFalse()
    {
        var sut = new DeployModeConverter();

        sut.CanConvertFrom(null, typeof(int)).Should().BeFalse();
    }
}
