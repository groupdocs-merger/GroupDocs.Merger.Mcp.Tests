using GroupDocs.Merger.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Merger.Mcp.IntegrationTests;

/// Merge combines 2–4 same-format documents into one file saved as
/// `<first>_merged.<ext>`. Evaluation mode may add watermarks but still
/// produces the merged file.
public class MergeTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MergeTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Merge_TwoDocxFiles_ProducesMergedFile()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SampleDocx))
            || !File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.Sample2Docx)))
        {
            _output.WriteLine("DOCX merge samples not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Merge.Name,
            new Dictionary<string, object?>
            {
                ["file1"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SampleDocx },
                ["file2"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.Sample2Docx },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Merge failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.DoesNotContain("Merge failed for", body);

        var mergedPath = Path.Combine(_fixture.StoragePath, "sample_merged.docx");
        Assert.True(File.Exists(mergedPath),
            $"Expected merged file at '{mergedPath}'. Response body:\n{body}");
        Assert.Contains("Merged", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Merge_ThreeDocxFiles_ProducesMergedFile()
    {
        var names = new[] { SampleDocuments.SampleDocx, SampleDocuments.Sample2Docx, SampleDocuments.Sample3Docx };
        if (names.Any(n => !File.Exists(Path.Combine(_fixture.StoragePath, n))))
        {
            _output.WriteLine("3-way DOCX merge samples not all present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Merge.Name,
            new Dictionary<string, object?>
            {
                ["file1"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SampleDocx },
                ["file2"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.Sample2Docx },
                ["file3"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.Sample3Docx },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("3-way merge failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);
        Assert.DoesNotContain("Merge failed for", body);
        Assert.True(File.Exists(Path.Combine(_fixture.StoragePath, "sample_merged.docx")));
    }

    [Fact]
    public async Task Merge_TwoPdfFiles_ProducesMergedFile()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SamplePdf))
            || !File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SampleSimplePdf)))
        {
            _output.WriteLine("PDF merge samples not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Merge.Name,
            new Dictionary<string, object?>
            {
                ["file1"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SamplePdf },
                ["file2"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SampleSimplePdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("PDF merge failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);
        Assert.DoesNotContain("Merge failed for", body);
        Assert.True(File.Exists(Path.Combine(_fixture.StoragePath, "sample_merged.pdf")));
    }
}
