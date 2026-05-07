using AiSetup.Deploy;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Deploy;

public sealed class DeployServiceTests
{
    [Fact]
    public void Deploy_InDryRun_DoesNotInvokeFileSystem()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Create)]);

        // Act
        var report = sut.Deploy(NewOptions(dryRun: true));

        // Assert
        report.DryRun.Should().BeTrue();
        report.Plan.Actions.Should().HaveCount(1);
        A.CallTo(() => fs.WriteAllText(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_WithoutForce_SkipsOverwriteActions()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Overwrite)]);

        // Act
        var report = sut.Deploy(NewOptions(force: false));

        // Assert
        report.Skipped.Should().HaveCount(1);
        report.Executed.Should().BeEmpty();
        A.CallTo(() => fs.WriteAllText(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_WithForce_OverwritesExistingFiles()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Overwrite)]);

        // Act
        var report = sut.Deploy(NewOptions(force: true));

        // Assert
        report.Executed.Should().HaveCount(1);
        report.Skipped.Should().BeEmpty();
        A.CallTo(() => fs.WriteAllText("/a.md", "x")).MustHaveHappened();
    }

    [Fact]
    public void Deploy_WithBackupAction_AlwaysExecutesBackupEvenWithoutForce()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists("/orig")).Returns(true);

        var actions = new DeployAction[]
        {
            new BackupFileAction("/bak", "/orig", DeployActionStatus.Overwrite, "backup"),
            NewWrite("/orig", "new", DeployActionStatus.Overwrite)
        };

        var sut = NewSut(fs, planActions: actions);

        // Act
        var report = sut.Deploy(NewOptions(force: true));

        // Assert
        report.Executed.Should().Contain(a => a is BackupFileAction);
        A.CallTo(() => fs.CopyFile("/orig", "/bak")).MustHaveHappened();
    }

    [Fact]
    public void Deploy_WithCopyDirectoryAction_DelegatesToFileSystem()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions:
        [
            new CopyDirectoryAction("/src/skill", "/dst/skill", DeployActionStatus.Create, "Copy skill")
        ]);

        // Act
        var report = sut.Deploy(NewOptions());

        // Assert
        report.Executed.Should().HaveCount(1);
        A.CallTo(() => fs.CopyDirectory("/src/skill", "/dst/skill")).MustHaveHappened();
    }

    [Fact]
    public void Deploy_BackupActionWhenOriginalMissing_SkipsCopy()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists("/missing")).Returns(false);

        var sut = NewSut(fs, planActions:
        [
            new BackupFileAction("/bak", "/missing", DeployActionStatus.Create, "backup")
        ]);

        // Act
        var report = sut.Deploy(NewOptions(force: true));

        // Assert
        report.Executed.Should().HaveCount(1);
        A.CallTo(() => fs.CopyFile(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_WithMultipleFailingActions_RecordsAllAsErrors()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.WriteAllText(A<string>._, A<string>._)).Throws<IOException>();

        var sut = NewSut(fs, planActions:
        [
            NewWrite("/a", "x", DeployActionStatus.Create),
            NewWrite("/b", "y", DeployActionStatus.Create)
        ]);

        // Act
        var report = sut.Deploy(NewOptions());

        // Assert
        report.Errors.Should().HaveCount(2);
        report.Executed.Should().BeEmpty();
    }

    [Fact]
    public void Deploy_InDryRun_ReportContainsEmptyExecutedSkippedAndErrors()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions:
        [
            NewWrite("/a", "x", DeployActionStatus.Create),
            NewWrite("/b", "y", DeployActionStatus.Overwrite)
        ]);

        // Act
        var report = sut.Deploy(NewOptions(dryRun: true));

        // Assert
        report.DryRun.Should().BeTrue();
        report.Executed.Should().BeEmpty();
        report.Skipped.Should().BeEmpty();
        report.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Deploy_WithFailingAction_RecordsItAsError()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.WriteAllText("/boom", A<string>._)).Throws<IOException>();

        var sut = NewSut(fs, planActions: [NewWrite("/boom", "x", DeployActionStatus.Create)]);

        // Act
        var report = sut.Deploy(NewOptions());

        // Assert
        report.Errors.Should().HaveCount(1);
        report.Executed.Should().BeEmpty();
    }

    [Fact]
    public void Deploy_WithMixedSuccessAndFailure_RecordsBothInReport()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.WriteAllText("/ok", A<string>._)).DoesNothing();
        A.CallTo(() => fs.WriteAllText("/boom", A<string>._)).Throws<IOException>();

        var sut = NewSut(fs, planActions:
        [
            NewWrite("/ok", "x", DeployActionStatus.Create),
            NewWrite("/boom", "y", DeployActionStatus.Create)
        ]);

        // Act
        var report = sut.Deploy(NewOptions());

        // Assert
        report.Executed.Should().ContainSingle().Which.TargetPath.Should().Be("/ok");
        report.Errors.Should().ContainSingle().Which.Action.TargetPath.Should().Be("/boom");
    }

    [Fact]
    public void Deploy_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: []);

        // Act
        Action act = () => sut.Deploy(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static DeployService NewSut(IFileSystem fs, IReadOnlyList<DeployAction> planActions)
    {
        var resolver = A.Fake<IProfileResolver>();
        A.CallTo(() => resolver.Resolve(A<DeployOptions>._)).Returns(ResolvedAssets.Empty);

        var target = A.Fake<IDeployTarget>();
        A.CallTo(() => target.Target).Returns(DeployTarget.ClaudeCode);
        A.CallTo(() => target.Plan(A<DeployOptions>._, A<ResolvedAssets>._))
            .Returns(new DeployPlan(DeployTarget.ClaudeCode, DeployMode.Repo, planActions));

        var registry = A.Fake<ITargetRegistry>();
        A.CallTo(() => registry.Get(A<DeployTarget>._)).Returns(target);

        return new DeployService(resolver, registry, fs);
    }

    private static DeployOptions NewOptions(bool dryRun = false, bool force = false) => new()
    {
        Target = DeployTarget.ClaudeCode,
        Mode = DeployMode.Repo,
        SourceRepoPath = "/src",
        DestinationRepoPath = "/dest",
        DryRun = dryRun,
        Force = force
    };

    private static WriteFileAction NewWrite(string path, string content, DeployActionStatus status) =>
        new(path, content, status, $"Write {path}");
}
