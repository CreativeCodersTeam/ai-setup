using AiSetup.Deploy;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Deploy;

public sealed class DeployServiceTests
{
    [Fact]
    public void Deploy_DryRun_DoesNotInvokeFileSystem()
    {
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Create)]);

        var report = sut.Deploy(NewOptions(dryRun: true));

        report.DryRun.Should().BeTrue();
        report.Plan.Actions.Should().HaveCount(1);
        A.CallTo(() => fs.WriteAllText(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_NonForce_SkipsOverwriteActions()
    {
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Overwrite)]);

        var report = sut.Deploy(NewOptions(force: false));

        report.Skipped.Should().HaveCount(1);
        report.Executed.Should().BeEmpty();
        A.CallTo(() => fs.WriteAllText(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_Force_OverwritesExistingFiles()
    {
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs, planActions: [NewWrite("/a.md", "x", DeployActionStatus.Overwrite)]);

        var report = sut.Deploy(NewOptions(force: true));

        report.Executed.Should().HaveCount(1);
        report.Skipped.Should().BeEmpty();
        A.CallTo(() => fs.WriteAllText("/a.md", "x")).MustHaveHappened();
    }

    [Fact]
    public void Deploy_BackupAction_AlwaysExecutedEvenWithoutForce()
    {
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists("/orig")).Returns(true);

        var actions = new DeployAction[]
        {
            new BackupFileAction("/bak", "/orig", DeployActionStatus.Overwrite, "backup"),
            NewWrite("/orig", "new", DeployActionStatus.Overwrite)
        };

        var sut = NewSut(fs, planActions: actions);

        var report = sut.Deploy(NewOptions(force: true));

        report.Executed.Should().Contain(a => a is BackupFileAction);
        A.CallTo(() => fs.CopyFile("/orig", "/bak")).MustHaveHappened();
    }

    [Fact]
    public void Deploy_FailingAction_RecordedAsError()
    {
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.WriteAllText("/boom", A<string>._)).Throws<IOException>();

        var sut = NewSut(fs, planActions: [NewWrite("/boom", "x", DeployActionStatus.Create)]);

        var report = sut.Deploy(NewOptions());

        report.Errors.Should().HaveCount(1);
        report.Executed.Should().BeEmpty();
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
