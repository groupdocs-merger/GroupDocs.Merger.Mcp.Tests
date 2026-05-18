# GroupDocs.Merger.Mcp.Tests

Integration tests for the [`GroupDocs.Merger.Mcp`](https://www.nuget.org/packages/GroupDocs.Merger.Mcp)
NuGet package — an MCP server that exposes
[GroupDocs.Merger](https://products.groupdocs.com/merger) as AI-callable tools.

This repository validates the **published** NuGet artifact end-to-end: it
launches the server via `dnx`, connects as an MCP client, and exercises every
advertised tool against real document fixtures sourced from the upstream
[GroupDocs.Merger-for-.NET Examples](https://github.com/groupdocs-merger/GroupDocs.Merger-for-.NET).

## Documentation

- [how-to/](how-to/) — step-by-step guides for every deployment channel
  ([NuGet](how-to/01-install-from-nuget.md),
  [Docker](how-to/02-run-via-docker.md),
  [MCP registry](how-to/03-verify-mcp-registry.md),
  [Claude Desktop](how-to/04-use-with-claude-desktop.md),
  [VS Code / Copilot](how-to/05-use-with-vscode-copilot.md),
  [running the tests](how-to/06-run-integration-tests.md)).
- [examples/](examples/) — ready-to-paste `claude-desktop.json`,
  `vscode-mcp.json`, and `docker-compose.yml`.
- [AGENTS.md](AGENTS.md) — orientation for AI coding agents working in this repo.
- [llms.txt](llms.txt) — machine-readable summary for LLM tooling.
- [changelog/](changelog/) — one entry per change set.
- [Files/README.md](Files/README.md) — provenance of real fixtures (MCP method
  ↔ upstream example ↔ filename).

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts via `dnx` | [McpServerFixture](src/GroupDocs.Merger.Mcp.Tests/Fixtures/McpServerFixture.cs) |
| MCP handshake, server info, tool list (3 tools) | [ToolDiscoveryTests](src/GroupDocs.Merger.Mcp.Tests/ToolDiscoveryTests.cs) |
| `merge` — 2-way + 3-way DOCX, 2-way PDF | [MergeTests](src/GroupDocs.Merger.Mcp.Tests/MergeTests.cs) |
| `split` — 10-page DOCX, unparseable-pages guard, protected-PDF password | [SplitTests](src/GroupDocs.Merger.Mcp.Tests/SplitTests.cs) |
| `get_document_info` — synthetic + 5 real samples, page-count assertion | [GetDocumentInfoTests](src/GroupDocs.Merger.Mcp.Tests/GetDocumentInfoTests.cs) |
| Unknown / corrupted files, password parameter | [ErrorHandlingTests](src/GroupDocs.Merger.Mcp.Tests/ErrorHandlingTests.cs) |

## Running locally

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet test
```

Test a specific published version:

```bash
dotnet test -p:McpPackageVersion=26.5.1
# or
MCP_PACKAGE_VERSION=26.5.1 dotnet test
```

The first run downloads the NuGet package — subsequent runs are cached.

## CI

[.github/workflows/integration.yml](.github/workflows/integration.yml) runs
on push / PR / nightly cron / `workflow_dispatch` / `repository_dispatch`.
Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`. Linux runners
install `libgdiplus` + `libfontconfig1` (Merger does structural page
operations, not text-glyph rendering — no MS core fonts needed).

## Evaluation vs licensed mode

`GroupDocs.Merger` is non-blocking in evaluation mode: `merge` and `split`
still produce their output files, the engine just stamps an evaluation
watermark on them. Tests pass either way — assertions check that the output
file is produced and the absence of the per-tool `<Op> failed for ...`
diagnostic prefix, not exact content.

For CI, store a base64-encoded `.lic` file as repo secret `GROUPDOCS_LICENSE`
— the workflow decodes it into `$RUNNER_TEMP` and exports
`GROUPDOCS_LICENSE_PATH` automatically.

## Fixture documents

Real fixtures live under [Files/](Files/) and are sourced from the upstream
`GroupDocs.Merger-for-.NET/Examples/Resources/SampleFiles/` folder. Each
fixture is documented in [Files/README.md](Files/README.md) with its MCP
method ↔ upstream example ↔ filename mapping. A minimal synthetic 1-page PDF
is generated at test startup as a known-shape baseline.

## License

MIT — see [LICENSE](LICENSE)
