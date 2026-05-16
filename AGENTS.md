# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

**Integration tests** for the [`GroupDocs.Merger.Mcp`](https://www.nuget.org/packages/GroupDocs.Merger.Mcp) NuGet package — an MCP server that exposes GroupDocs.Merger for .NET as AI-callable tools.

This repo is **not** the server itself. The server lives at [groupdocs-merger/GroupDocs.Merger.Mcp](https://github.com/groupdocs-merger/GroupDocs.Merger.Mcp). This repo:

1. Consumes only the **published** NuGet artifact (no project references).
2. Launches the server via `dnx`, connects as an MCP stdio client, and exercises every advertised tool.
3. Doubles as a copy-pasteable set of example configs and how-to guides for all deployment channels (NuGet, Docker, MCP registry, Claude Desktop, VS Code).

## Folder layout

```
src/GroupDocs.Merger.Mcp.Tests/
  Fixtures/
    McpServerFixture.cs          ← launches dnx child process, wires stdio MCP client
    SampleDocuments.cs           ← builds a minimal bare PDF at runtime + copies Files/ fixtures
    ToolCatalog.cs               ← keyword-based tool name resolution (merge/split/document_info)
    ToolResponse.cs              ← CallToolResult text/JSON extraction
    CommandResolver.cs           ← cross-platform dnx.cmd resolution on Windows
    PackageVersion.cs            ← pulls version from env / assembly metadata / default
  ToolDiscoveryTests.cs          ← handshake, tools/list (3 tools), schema validation
  MergeTests.cs                  ← 2-way + 3-way DOCX merge, 2-way PDF merge
  SplitTests.cs                  ← 10-page DOCX split, unparseable-pages guard, protected-PDF password
  GetDocumentInfoTests.cs        ← synthetic + 5 real samples, 10-page count assertion
  ErrorHandlingTests.cs          ← unknown file, corrupted bytes, password parameter
  GroupDocs.Merger.Mcp.Tests.csproj
.github/workflows/integration.yml  ← matrix × 3 OS, nightly cron, release-smoke dispatch
changelog/                         ← one MD file per change (NNN-slug.md)
how-to/                            ← user-facing guides for every deployment channel
examples/                          ← claude-desktop.json, vscode-mcp.json, docker-compose.yml
Files/                             ← real fixtures from the upstream Examples repo (see Files/README.md)
Directory.Build.props              ← McpPackageVersion property (overridable)
global.json                        ← pinned to .NET 10.0.100
```

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts via `dnx` | `McpServerFixture` |
| MCP handshake, server info, tool list (3 tools) | `ToolDiscoveryTests` |
| `merge` — 2-way + 3-way DOCX, 2-way PDF | `MergeTests` |
| `split` — 10-page DOCX, unparseable-pages guard, protected-PDF password | `SplitTests` |
| `get_document_info` — synthetic + 5 real samples, page-count assertion | `GetDocumentInfoTests` |
| Unknown / corrupted files, password parameter | `ErrorHandlingTests` |

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build -c Release

# Run all 12 tests against the default package version (26.5.0)
dotnet test -c Release

# Run against a specific published version
dotnet test -c Release -p:McpPackageVersion=26.5.0
# or
MCP_PACKAGE_VERSION=26.5.0 dotnet test -c Release

# Unlock licensed-mode Split tests
GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic dotnet test -c Release

# Run just the discovery suite (fastest — no tool invocations)
dotnet test -c Release --filter "FullyQualifiedName~ToolDiscovery"
```

## Key design decisions

1. **Keyword-based tool resolution.** `ToolCatalog.Resolve("merge")` picks the tool whose name contains "merge" (case-insensitive). The MCP C# SDK converts `[McpServerTool]` method names to `snake_case` — so the actual wire names are `merge`, `split`, and `get_document_info`. Multi-word tools need the underscored keyword (`document_info`, NOT `documentinfo`) — see Pitfall #15.

2. **Synthetic + real fixtures.** `SampleDocuments.cs` builds one minimal bare 1-page PDF at runtime as a known-shape baseline. Real fixtures live in [Files/](Files/) — sourced from the upstream `GroupDocs.Merger-for-.NET/Examples/Resources/SampleFiles/`. See `Files/README.md` for the MCP method ↔ upstream example ↔ filename mapping. The csproj auto-copies everything in `Files/**` to the test output.

3. **Evaluation-mode is non-blocking.** Unlike `GroupDocs.Metadata.Save()` which throws in evaluation mode, `GroupDocs.Merger` produces the merged / split output normally — it just stamps an evaluation watermark on it. Tests run identically with or without `GROUPDOCS_LICENSE_PATH`; assertions check that the output file is produced and the absence of per-tool `<Op> failed for ...` diagnostic prefixes, not exact content. CI auto-decodes a `GROUPDOCS_LICENSE` repo secret into `$RUNNER_TEMP` to verify the no-watermark case.

4. **JSON responses are returned raw.** `GetDocumentInfo` calls `JsonSerializer.Serialize(...)` directly without piping through `OutputHelper.TruncateText` — the truncation marker is plain text and would break strict-JSON consumers. Test fixtures parse responses with `JsonDocument.Parse`.

5. **Engine errors surface diagnostically.** All three tools wrap their engine calls in `try/catch` and return `"Merge failed for ..."` / `"Split failed for ..."` / `"Document-info lookup failed for ..."` instead of bubbling to MCP's canned `"An error occurred invoking '<tool>'"` wrapper. Tests assert `DoesNotContain("<Op> failed for", body)` on the success path.

6. **No project references to the server.** The csproj only references `ModelContextProtocol` 1.1.0. If the server source breaks in the sibling repo, these tests still pass — they validate the shipped NuGet artifact.

## House rules

1. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md` (schema in `changelog/README.md`).
2. **How-to guides track deployment reality** — if the main repo publishes a new channel (e.g. new Docker registry), add a guide under `how-to/` *and* update `README.md`.
3. **Version bumps flow through `Directory.Build.props`** — `<McpPackageVersion>` is the single source of truth for "what version are we testing." CI overrides it via env var / workflow input.
4. **Tests must not require the main repo's source.** If a test needs a server-side change, file an issue there — don't work around it here.
5. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release smoke hook

The main repo's `publish_prod.yml` should fire a `repository_dispatch` with `event_type=nuget-published` after `dotnet nuget push` succeeds. The workflow in `.github/workflows/integration.yml` consumes `client_payload.package_version` and runs the matrix against the just-published version. This closes the loop: publish → smoke-test live nuget.org → fail loud if broken.

## What NOT to change

- Do not add a `ProjectReference` to the main repo's `GroupDocs.Merger.Mcp.csproj`. This repo exists to test the shipped NuGet, not the source.
- Do not hardcode tool names as string literals (`"merge"`). Use `ToolCatalog.Read.Name` / `ToolCatalog.Remove.Name`.
- Do not commit real license files. The license goes through the `GROUPDOCS_LICENSE` CI secret; fixtures in `Files/` are sourced from the public GroupDocs.Merger-for-.NET Examples repo (see `Files/README.md` for provenance).
