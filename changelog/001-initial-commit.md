---
id: 001
date: 2026-05-16
version: 26.5.0
type: feature
---

# Initial integration-test repo for GroupDocs.Merger.Mcp 26.5.0

## What changed
- Public integration-test repo `groupdocs-merger/GroupDocs.Merger.Mcp.Tests` published.
- Exercises the **shipped** `GroupDocs.Merger.Mcp@26.5.0` NuGet via `dnx`, NOT a project reference to the server source.
- Test suites (covering all 3 advertised tools):
  - `ToolDiscoveryTests` — server handshake, tool listing (asserts exactly 3 tools: `merge`, `split`, `get_document_info`), schema sanity.
  - `MergeTests` — 2-way and 3-way DOCX merges, 2-way PDF merge; asserts the `<first>_merged.<ext>` output file lands in storage.
  - `SplitTests` — 10-page DOCX page extraction (`pages='1,3,5'`), the unparseable-pages graceful-message guard, and the protected-PDF `password` parameter.
  - `GetDocumentInfoTests` — synthetic 1-page PDF + 5 real samples, asserts the `{ fileName, fileType, pageCount, size, pages }` JSON shape, and a 10-page-count assertion on `sample-10-pages.docx`.
  - `ErrorHandlingTests` — unknown filename returns available-files hint; corrupted bytes don't crash the server; `password` parameter accepted by schema.
- **Real fixtures sourced from the upstream Examples repo** (per Step 6e of the clone prompt): 9 files copied from `GroupDocs.Merger-for-.NET/Examples/Resources/SampleFiles/`. Each fixture documented in `Files/README.md` with MCP method ↔ upstream example ↔ filename mapping. Total ~0.4 MB.
- Synthetic-fixture builder writes a minimal bare 1-page PDF at runtime as a known-shape baseline.
- Integration workflow (`.github/workflows/integration.yml`): matrix × 3 OS, nightly cron, release-smoke `repository_dispatch` listener.
- Linux runners install `libgdiplus libfontconfig1` (base apt set per Pitfall #17 tier 1 — Merger does structural page operations, not text-glyph rendering, so MS core fonts are NOT required).

## Pitfall remediations baked in
- **`ToolCatalog` keyword resolvers use snake_case substrings** (`merge`, `split`, `document_info`) — Pitfall #15 audit clean (`document_info` includes the underscore so it matches the `get_document_info` wire name).
- **Test fixtures don't assert on `IsError` semantics** — the main server's tools return `<Op> failed for '...'` descriptive error strings, not throws (Pitfall #18). Tests assert `DoesNotContain("<Op> failed for", body)` on the success path.
- **JSON consumers use `JsonDocument.Parse`** — `GetDocumentInfo` returns raw JSON (not piped through `OutputHelper.TruncateText`).
- **Fixtures provenance documented** — `Files/README.md` is the single source of truth for "which upstream example brought in which file."

## Why
Seventh product Tests repo in the GroupDocs MCP framework family (after Metadata, Conversion, Comparison, Viewer, Watermark, Parser). Validates the shipped NuGet artifact end-to-end on every release and on a nightly cron, and doubles as a reference for users deploying Merger via NuGet, Docker, Claude Desktop, or VS Code.

## Migration / impact
First release — no migration required.
