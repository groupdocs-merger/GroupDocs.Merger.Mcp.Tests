# Files — real document fixtures for the integration suite

Real fixtures used by the integration suite, sourced from the upstream
[GroupDocs.Merger-for-.NET Examples repo](https://github.com/groupdocs-merger/GroupDocs.Merger-for-.NET).

## Provenance

Each fixture is here because an upstream example uses it to demonstrate a
Merger operation the MCP server wraps.

| File | MCP method(s) | Upstream example | Notes |
|---|---|---|---|
| `sample.docx` | `Merge`, `GetDocumentInfo` | `BasicUsage/MultipleDocumentOperations/Merge/MergeDocx.cs` | DOCX merge input #1 (from `Resources/SampleFiles/WordProcessing/`) |
| `sample2.docx` | `Merge` | same | DOCX merge input #2 |
| `sample3.docx` | `Merge` | same | DOCX merge input #3 (exercises 3-way merge) |
| `sample-10-pages.docx` | `Split` | `BasicUsage/SingleDocumentOperations/SplitDocument/SplitToSinglePages.cs` (`Constants.SAMPLE_DOCX_10_PAGES`) | 10-page DOCX — ideal split target |
| `sample.pdf` | `Merge`, `GetDocumentInfo` | `BasicUsage/MultipleDocumentOperations/Merge/MergePdf.cs` | PDF merge input #1 (from `Resources/SampleFiles/Pdf/`) |
| `sample_simple.pdf` | `Merge` | same | PDF merge input #2 |
| `sample_protected.pdf` | (`password` parameter on `Split` / `GetDocumentInfo`) | `AdvancedUsage/Loading/LoadPasswordProtectedDocument.cs` (`Constants.SAMPLE_PASSWORD`) | Password-protected PDF. **Password: `SomePasswordString`** |
| `sample.pptx` | `GetDocumentInfo` (format variant) | `Resources/SampleFiles/Presentation/` | PowerPoint — format coverage |
| `sample.xlsx` | `GetDocumentInfo` (format variant) | `Resources/SampleFiles/Spreadsheet/` | Spreadsheet — format coverage |

## Refresh command

```bash
EX=../../GroupDocs.Merger-for-.NET/Examples/GroupDocs.Merger.Examples.CSharp/Resources/SampleFiles
cp "$EX/WordProcessing/sample.docx" "$EX/WordProcessing/sample2.docx" "$EX/WordProcessing/sample3.docx" \
   "$EX/WordProcessing/sample-10-pages.docx" "$EX/Pdf/sample.pdf" "$EX/Pdf/sample_simple.pdf" \
   "$EX/Pdf/sample_protected.pdf" "$EX/Presentation/sample.pptx" "$EX/Spreadsheet/sample.xlsx" ./Files/
```

## Wiring

The csproj's `<None Include="..\..\Files\**\*">` glob copies these to the
test output's `Files/` subfolder. `SampleDocuments.ResolveSourceSampleDocs()`
finds that folder at runtime and seeds the server's storage path.

## Adding new fixtures

1. Drop a binary into this folder.
2. Add a `public const string` to `SampleDocuments.cs`.
3. Add it to the `RealSamples` array to auto-load it into storage.
4. Write a `[Theory]`/`[Fact]` referencing the new constant.
5. Add a provenance row to the table above.
