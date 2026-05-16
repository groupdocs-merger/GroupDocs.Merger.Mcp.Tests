using ModelContextProtocol.Client;

namespace GroupDocs.Merger.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by keyword. The server-side attribute [McpServerTool]
/// converts PascalCase method names to snake_case wire names (Merge → merge,
/// GetDocumentInfo → get_document_info). Keywords here are snake_case
/// substrings that uniquely identify each tool — see Pitfall #15 in the clone
/// prompt (`"documentinfo"` without the underscore does NOT match
/// `"get_document_info"`).
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools) => _tools = tools;

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;

    public McpClientTool Merge           => Resolve("merge");
    public McpClientTool Split           => Resolve("split");
    public McpClientTool GetDocumentInfo => Resolve("document_info");

    private McpClientTool Resolve(string keyword) =>
        _tools.FirstOrDefault(t => t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"No tool with name containing '{keyword}'. Found: {string.Join(", ", _tools.Select(t => t.Name))}");
}
