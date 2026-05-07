using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Util;
using CreativeCoders.Core;

namespace AiSetup.Profiles;

/// <summary>
/// Default <see cref="IProfileResolver"/> that merges profile selections with CLI overrides
/// and resolves IDs against the asset repository, producing typed missing-asset errors with suggestions.
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

        Profile? profile = null;

        if (!string.IsNullOrWhiteSpace(options.ProfileName))
        {
            profile = _profiles.Find(options.ProfileName)
                ?? throw new MissingProfileException(
                    options.ProfileName,
                    Levenshtein.SuggestSimilar(
                        options.ProfileName,
                        _profiles.All().Select(p => p.Name)));
        }

        var agentIds = Combine(profile?.Agents, options.Agents);
        var instructionIds = Combine(profile?.Instructions, options.Instructions);
        var skillIds = Combine(profile?.Skills, options.Skills);
        var mcpIds = Combine(profile?.McpConfigs, options.McpConfigs);

        return new ResolvedAssets(
            Agents: ResolveIds(AssetType.Agent, agentIds),
            Instructions: ResolveIds(AssetType.Instruction, instructionIds),
            Skills: ResolveIds(AssetType.Skill, skillIds),
            McpConfigs: ResolveIds(AssetType.McpConfig, mcpIds));
    }

    private static IReadOnlyList<string> Combine(IReadOnlyList<string>? primary, IReadOnlyList<string> overrides)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();

        if (primary is not null)
        {
            foreach (var id in primary)
            {
                if (seen.Add(id))
                {
                    result.Add(id);
                }
            }
        }

        foreach (var id in overrides)
        {
            if (seen.Add(id))
            {
                result.Add(id);
            }
        }

        return result;
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
