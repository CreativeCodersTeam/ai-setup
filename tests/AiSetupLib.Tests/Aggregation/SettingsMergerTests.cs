using AiSetup.Aggregation;
using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Tests.Aggregation;

public sealed class SettingsMergerTests
{
    [Fact]
    public void Merge_OneFragmentOntoEmptyBase_ReturnsFragment()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "model": "opus", "env": { "X": "1" } }""")],
            existingJson: null,
            McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\"model\": \"opus\"");
        json.Should().Contain("\"X\": \"1\"");
    }

    [Fact]
    public void Merge_TwoFragments_DeepMergesObjects()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "env": { "X": "1" } }"""), Settings("b", """{ "env": { "Y": "2" } }""")],
            existingJson: null,
            McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\"X\": \"1\"").And.Contain("\"Y\": \"2\"");
    }

    [Fact]
    public void Merge_ArrayValues_AreUnioned()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "permissions": { "allow": ["B"] } }""")],
            existingJson: """{ "permissions": { "allow": ["A"] } }""",
            McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\"A\"").And.Contain("\"B\"");
    }

    [Fact]
    public void Merge_ArrayValues_DoNotDuplicateExistingItems()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "allow": ["A", "B"] }""")],
            existingJson: """{ "allow": ["A"] }""",
            McpConflictResolution.Fail);

        // Assert
        var allowMatches = System.Text.RegularExpressions.Regex.Matches(json, "\"A\"");
        allowMatches.Should().HaveCount(1);
        json.Should().Contain("\"B\"");
    }

    [Fact]
    public void Merge_ScalarConflictWithFail_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge(
            [Settings("a", """{ "model": "opus" }""")],
            existingJson: """{ "model": "sonnet" }""",
            McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_ScalarConflictWithSkip_KeepsExisting()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "model": "opus" }""")],
            existingJson: """{ "model": "sonnet" }""",
            McpConflictResolution.Skip);

        // Assert
        json.Should().Contain("\"model\": \"sonnet\"");
    }

    [Fact]
    public void Merge_ScalarConflictWithOverwrite_ReplacesValue()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "model": "opus" }""")],
            existingJson: """{ "model": "sonnet" }""",
            McpConflictResolution.Overwrite);

        // Assert
        json.Should().Contain("\"model\": \"opus\"");
    }

    [Fact]
    public void Merge_IdenticalScalar_KeepsValueWithoutConflict()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "model": "opus" }""")],
            existingJson: """{ "model": "opus" }""",
            McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\"model\": \"opus\"");
    }

    [Fact]
    public void Merge_NewKey_IsAddedToBase()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge(
            [Settings("a", """{ "newKey": "value" }""")],
            existingJson: """{ "existing": "kept" }""",
            McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\"existing\": \"kept\"").And.Contain("\"newKey\": \"value\"");
    }

    [Fact]
    public void Merge_NoFragmentsWithNoExistingJson_ReturnsEmptyObject()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge([], existingJson: null, McpConflictResolution.Fail);

        // Assert
        json.Should().Be("{}");
    }

    [Fact]
    public void Merge_InvalidBaseJson_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge([Settings("a", "{}")], existingJson: "not json", McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_BaseJsonNotAnObject_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge([Settings("a", "{}")], existingJson: "[1,2,3]", McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_AssetBodyNotAJsonObject_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge([Settings("a", "[1,2,3]")], existingJson: null, McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_AssetBodyInvalidJson_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge([Settings("a", "not json")], existingJson: null, McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_NonSettingsAsset_Throws()
    {
        // Arrange
        var sut = new SettingsMerger();
        var notSettings = new AssetDefinition(
            "x", AssetType.Instruction, "x", "", [], [], "/x", null,
            new Dictionary<string, object?>(), "{}");

        // Act
        Action act = () => sut.Merge([notSettings], existingJson: null, McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_WithNullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        Action act = () => sut.Merge(null!, existingJson: null, McpConflictResolution.Fail);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Merge_Output_IsTwoSpaceIndented()
    {
        // Arrange
        var sut = new SettingsMerger();

        // Act
        var json = sut.Merge([Settings("a", """{ "env": { "X": "1" } }""")], existingJson: null, McpConflictResolution.Fail);

        // Assert
        json.Should().Contain("\n  \"env\":");
        json.Should().Contain("\n    \"X\":");
    }

    private static AssetDefinition Settings(string id, string body) => new(
        id, AssetType.Settings, id, "", [], [], "/" + id, null, new Dictionary<string, object?>(), body);
}
