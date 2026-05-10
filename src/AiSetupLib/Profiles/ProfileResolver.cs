using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Util;
using CreativeCoders.Core;

namespace AiSetup.Profiles;

/// <summary>
/// Default <see cref="IProfileResolver"/> that loads a named profile and resolves its asset IDs
/// against the asset repository, producing typed missing-profile/missing-asset errors with suggestions.
/// </summary>
public sealed class ProfileResolver : IProfileResolver
{
    private readonly IAssetRepository _assets;
    private readonly IProfileRepository _profiles;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="assets">Asset repository to look up IDs against.</param>
    /// <param name="profiles">Profile repository to load named profiles.</param>
    public ProfileResolver(IAssetRepository assets, IProfileRepository profiles)
    {
        _assets = Ensure.NotNull(assets);
        _profiles = Ensure.NotNull(profiles);
    }

    /// <inheritdoc />
    public ResolvedAssets Resolve(DeployOptions options)
    {
        Ensure.NotNull(options);
        Ensure.IsNotNullOrWhitespace(options.ProfileName);

        var profile = _profiles.Find(options.ProfileName)
            ?? throw new MissingProfileException(
                options.ProfileName,
                Levenshtein.SuggestSimilar(
                    options.ProfileName,
                    _profiles.All().Select(p => p.Name)));

        return new ResolvedAssets(
            Agents: ResolveIds(AssetType.Agent, profile.Agents),
            Instructions: ResolveIds(AssetType.Instruction, profile.Instructions),
            Skills: ResolveIds(AssetType.Skill, profile.Skills),
            McpConfigs: ResolveIds(AssetType.McpConfig, profile.McpConfigs));
    }

    private IReadOnlyList<AssetDefinition> ResolveIds(AssetType type, IReadOnlyList<string> ids)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var result = new List<AssetDefinition>(ids.Count);

        foreach (var id in ids)
        {
            var asset = _assets.Find(type, id)
                ?? throw new MissingAssetException(
                    type,
                    id,
                    Levenshtein.SuggestSimilar(id, _assets.All(type).Select(a => a.Id)));

            result.Add(asset);
        }

        return result;
    }
}
