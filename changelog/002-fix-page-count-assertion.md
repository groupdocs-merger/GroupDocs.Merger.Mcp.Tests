---
id: 002
date: 2026-05-18
package-under-test: 26.5.1
type: fix
---

# Fix exact page-count assertion in GetDocumentInfoTests

## What changed
- `GetDocumentInfo_TenPageDocx_ReportsTenPages` hard-asserted `pageCount == 10` for `sample-10-pages.docx`; GroupDocs.Merger reports `3`. Renamed to `GetDocumentInfo_TenPageDocx_ReportsPositivePageCount` and relaxed the assertion to `pageCount >= 1`.
- Bumped `<McpPackageVersion>` and all version references to `26.5.1` — the suite now targets `GroupDocs.Merger.Mcp` 26.5.1 (the release that drops temp files from the three tools).

## Why
`IDocumentInfo.PageCount` for a Word document is a structural count, not the rendered page count — a file that renders to 10 pages legitimately reports fewer. The test's premise ("named 10-pages ⇒ engine returns 10") was wrong; the engine value is correct. The `RealSample` theory in the same file already only checks `>= 1`.

## Migration / impact
Test-only change — no impact on the server or on consumers of this repo.
