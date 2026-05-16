using GroupDocs.Merger.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Merger.Mcp.IntegrationTests;

/// Split extracts specific 1-based pages from a document, saving each as its
/// own file. `sample-10-pages.docx` is the upstream Examples canonical
/// 10-page split target.
[Collection(McpServerCollection.Name)]
public class SplitTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SplitTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Split_TenPageDocx_ExtractsRequestedPages()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.Sample10PagesDocx)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.Sample10PagesDocx}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Split.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.Sample10PagesDocx },
                ["pages"] = "1,3,5",
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Split failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.DoesNotContain("Split failed for", body);
        Assert.Contains("Split", body, StringComparison.OrdinalIgnoreCase);

        // The tool reports "Split '<file>' into N file(s):" — N should be >= 1.
        Assert.Contains("file(s)", body);
    }

    [Fact]
    public async Task Split_UnparseablePages_ReturnsGracefulMessage()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Split.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SyntheticPdf },
                ["pages"] = "not-a-number",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // The tool catches the parse failure and returns a graceful hint.
        Assert.Contains("Could not parse page numbers", body);
    }

    [Fact]
    public async Task Split_ProtectedPdf_AcceptsPasswordArgument()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SampleProtectedPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.SampleProtectedPdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Split.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SampleProtectedPdf },
                ["pages"] = "1",
                ["password"] = SampleDocuments.ProtectedDocumentPassword,
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Protected-PDF split failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);
        Assert.DoesNotContain("Split failed for", body);
    }
}
