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
    public void ConvertFrom_WithKnownTokens_ReturnsExpectedMode(string raw, DeployMode expected)
    {
        // Arrange
        var sut = new DeployModeConverter();

        // Act
        var result = sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("re po")]
    public void ConvertFrom_WithUnknownString_ThrowsFormatException(string raw)
    {
        // Arrange
        var sut = new DeployModeConverter();

        // Act
        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        // Assert
        act.Should().Throw<FormatException>().WithMessage("*Unknown deploy mode*");
    }

    [Fact]
    public void ConvertFrom_WithNonStringValue_ThrowsNotSupportedException()
    {
        // Arrange
        var sut = new DeployModeConverter();

        // Act
        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, 42);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CanConvertFrom_WithStringType_ReturnsTrue()
    {
        // Arrange
        var sut = new DeployModeConverter();

        // Act
        var result = sut.CanConvertFrom(null, typeof(string));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvertFrom_WithIntType_ReturnsFalse()
    {
        // Arrange
        var sut = new DeployModeConverter();

        // Act
        var result = sut.CanConvertFrom(null, typeof(int));

        // Assert
        result.Should().BeFalse();
    }
}
