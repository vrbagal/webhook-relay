using System.Text.Json;
using System.Text.Json.Nodes;
using WebhookRelay.Core.Entities;

namespace WebhookRelay.Infrastructure.Delivery;

/// <summary>
/// Evaluates whether a webhook payload matches ALL routing rules on a target.
/// All rules are ANDed — a target is skipped if any rule fails.
/// Returns true (deliver) when the target has no rules.
/// </summary>
public static class RoutingRuleEvaluator
{
    public static bool Matches(string rawPayload, IEnumerable<RoutingRule> rules)
    {
        var ruleList = rules.ToList();
        if (ruleList.Count == 0) return true;

        JsonNode? root;
        try { root = JsonNode.Parse(rawPayload); }
        catch { root = null; }

        foreach (var rule in ruleList)
        {
            if (!EvaluateRule(root, rule))
                return false;
        }

        return true;
    }

    private static bool EvaluateRule(JsonNode? root, RoutingRule rule)
    {
        var node = ResolveJsonPath(root, rule.JsonPath);
        var op = rule.Operator.ToLowerInvariant();

        return op switch
        {
            "exists"       => node is not null,
            "not_exists"   => node is null,
            "equals"       => CompareString(node, rule.Value, StringComparison.OrdinalIgnoreCase) == 0,
            "not_equals"   => CompareString(node, rule.Value, StringComparison.OrdinalIgnoreCase) != 0,
            "contains"     => GetString(node)?.Contains(rule.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "not_contains" => GetString(node)?.Contains(rule.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) != true,
            "starts_with"  => GetString(node)?.StartsWith(rule.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "ends_with"    => GetString(node)?.EndsWith(rule.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            _              => false,
        };
    }

    /// <summary>
    /// Resolves a simple dot-notation path like "$.type" or "data.object.amount".
    /// Leading "$." is stripped automatically.
    /// </summary>
    private static JsonNode? ResolveJsonPath(JsonNode? root, string path)
    {
        if (root is null) return null;

        // Strip optional leading "$."
        var normalised = path.TrimStart('$').TrimStart('.');
        if (string.IsNullOrWhiteSpace(normalised)) return root;

        var current = root;
        foreach (var segment in normalised.Split('.'))
        {
            if (current is JsonObject obj && obj.TryGetPropertyValue(segment, out var child))
                current = child;
            else
                return null;
        }

        return current;
    }

    private static string? GetString(JsonNode? node) =>
        node switch
        {
            JsonValue v => v.TryGetValue<string>(out var s) ? s : node.ToString(),
            null        => null,
            _           => node.ToString(),
        };

    private static int CompareString(JsonNode? node, string? value, StringComparison comparison) =>
        string.Compare(GetString(node), value, comparison);
}
