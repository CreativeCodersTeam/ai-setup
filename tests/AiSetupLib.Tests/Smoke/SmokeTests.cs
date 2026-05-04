namespace AiSetupLib.Tests.Smoke;

public class SmokeTests
{
    [Fact]
    public void Library_assembly_should_be_loadable()
    {
        var assembly = typeof(AiSetupLib.AssemblyMarker).Assembly;
        assembly.Should().NotBeNull();
    }
}
